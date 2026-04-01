using System.Security.Claims;
using AgenticCompany.Api.Mapping;
using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Enums;
using AgenticCompany.Core.Interfaces;
using AgenticCompany.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgenticCompany.Api.Controllers;

[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskItemRepository _taskRepo;
    private readonly IPlanRepository _planRepo;
    private readonly ISpecRepository _specRepo;
    private readonly INodeRepository _nodeRepo;
    private readonly INodeMemberRepository _memberRepo;
    private readonly AppDbContext _db;

    public TasksController(
        ITaskItemRepository taskRepo,
        IPlanRepository planRepo,
        ISpecRepository specRepo,
        INodeRepository nodeRepo,
        INodeMemberRepository memberRepo,
        AppDbContext db)
    {
        _taskRepo = taskRepo;
        _planRepo = planRepo;
        _specRepo = specRepo;
        _nodeRepo = nodeRepo;
        _memberRepo = memberRepo;
        _db = db;
    }

    private async Task<bool> IsNodeMemberAsync(Guid nodeId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var membership = await _memberRepo.GetAsync(nodeId, userId, ct);
        return membership != null && membership.Role != NodeRole.Viewer;
    }

    [HttpGet("api/plans/{planId:guid}/tasks")]
    public async Task<ActionResult<List<TaskItemResponse>>> GetByPlan(Guid planId, CancellationToken ct)
    {
        var plan = await _planRepo.GetByIdAsync(planId, ct);
        if (plan is null) return NotFound(new { error = $"Plan {planId} not found." });

        var tasks = await _taskRepo.GetByPlanIdAsync(planId, ct);
        return Ok(tasks.Select(t => t.ToResponse()).ToList());
    }

    [HttpGet("api/tasks/{id:guid}")]
    public async Task<ActionResult<TaskItemResponse>> GetById(Guid id, CancellationToken ct)
    {
        var task = await _taskRepo.GetByIdAsync(id, ct);
        if (task is null) return NotFound();
        return Ok(task.ToResponse());
    }

    [HttpPost("api/plans/{planId:guid}/tasks")]
    public async Task<ActionResult<TaskItemResponse>> Create(Guid planId, [FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var plan = await _planRepo.GetByIdAsync(planId, ct);
        if (plan is null) return NotFound("Plan not found");

        var spec = await _specRepo.GetByIdAsync(plan.SpecId, ct);
        if (spec is null) return NotFound("Spec not found");

        if (!await IsNodeMemberAsync(spec.NodeId, ct))
            return Forbid();

        if (request.TargetNodeId.HasValue)
        {
            var targetNode = await _nodeRepo.GetByIdAsync(request.TargetNodeId.Value, ct);
            if (targetNode is null) return BadRequest("Target node not found.");
        }

        var task = new TaskItem
        {
            PlanId = planId,
            Title = request.Title,
            Description = request.Description,
            AssignedTo = request.AssignedTo,
            Order = request.Order,
            TargetNodeId = request.TargetNodeId,
            Status = TaskItemStatus.Pending,
        };

        var created = await _taskRepo.CreateAsync(task, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToResponse());
    }

    [HttpPut("api/tasks/{id:guid}")]
    public async Task<ActionResult<TaskItemResponse>> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        var task = await _taskRepo.GetByIdAsync(id, ct);
        if (task is null) return NotFound();

        var plan = await _planRepo.GetByIdAsync(task.PlanId, ct);
        if (plan is null) return NotFound("Plan not found");

        var spec = await _specRepo.GetByIdAsync(plan.SpecId, ct);
        if (spec is null) return NotFound("Spec not found");

        if (!await IsNodeMemberAsync(spec.NodeId, ct))
            return Forbid();

        if (request.Title != null) task.Title = request.Title;
        if (request.Description != null) task.Description = request.Description;
        if (request.AssignedTo != null) task.AssignedTo = request.AssignedTo;
        if (request.TargetNodeId.HasValue)
        {
            var targetNode = await _nodeRepo.GetByIdAsync(request.TargetNodeId.Value, ct);
            if (targetNode is null) return BadRequest("Target node not found.");
            task.TargetNodeId = request.TargetNodeId;
        }
        if (request.Order.HasValue) task.Order = request.Order.Value;
        if (request.Status != null)
        {
            if (!Enum.TryParse<TaskItemStatus>(request.Status, true, out var status))
                return BadRequest($"Invalid status '{request.Status}'. Valid values: {string.Join(", ", Enum.GetNames<TaskItemStatus>())}");
            if (status == TaskItemStatus.Cascaded)
                return BadRequest("Status 'Cascaded' can only be set via the cascade endpoint.");
            if (task.Status == TaskItemStatus.Cascaded)
                return BadRequest("Cannot change status of a cascaded task.");
            task.Status = status;
        }

        await _taskRepo.UpdateAsync(task, ct);
        return Ok(task.ToResponse());
    }

    /// <summary>
    /// Cascade a task to a child node — creates a new Spec on the target node
    /// linked back to this task. This is the core multi-layer feature.
    /// </summary>
    [HttpPost("api/tasks/{id:guid}/cascade")]
    public async Task<ActionResult<CascadeResponse>> Cascade(Guid id, [FromBody] CascadeRequest request, CancellationToken ct)
    {
        var task = await _taskRepo.GetByIdAsync(id, ct);
        if (task is null) return NotFound("Task not found");

        if (task.SpawnedSpecId.HasValue)
            return BadRequest("Task has already been cascaded.");

        var targetNode = await _nodeRepo.GetByIdAsync(request.TargetNodeId, ct);
        if (targetNode is null) return BadRequest("Target node not found");

        // Validate that target node is a descendant of the spec's owning node
        var plan = await _planRepo.GetByIdAsync(task.PlanId, ct);
        if (plan is null) return NotFound("Plan not found");
        
        var spec = await _specRepo.GetByIdAsync(plan.SpecId, ct);
        if (spec is null) return NotFound("Spec not found");
        
        var sourceNode = await _nodeRepo.GetByIdAsync(spec.NodeId, ct);
        if (sourceNode is null) return NotFound("Source node not found");
        
        if (!targetNode.Path.StartsWith(sourceNode.Path + "."))
            return BadRequest("Target node must be a descendant of the spec's owning node");

        if (!await IsNodeMemberAsync(spec.NodeId, ct))
            return Forbid();

        if (!await IsNodeMemberAsync(request.TargetNodeId, ct))
            return Forbid();

        using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            // Update the task's TargetNodeId to the value from the request
            task.TargetNodeId = request.TargetNodeId;

            // Create a new spec on the target node from this task
            var newSpec = new Spec
            {
                NodeId = request.TargetNodeId,
                Title = task.Title,
                Status = SpecStatus.Draft,
                SourceTaskId = task.Id,
            };

            var createdSpec = await _specRepo.CreateAsync(newSpec, ct);

            // Add initial version with the task description as content
            createdSpec.Versions.Add(new SpecVersion
            {
                Id = Guid.NewGuid(),
                SpecId = createdSpec.Id,
                Version = 1,
                Content = task.Description ?? task.Title,
                CreatedAt = DateTime.UtcNow,
            });
            await _specRepo.UpdateAsync(createdSpec, ct);

            // Update the task to mark it as cascaded
            task.SpawnedSpecId = createdSpec.Id;
            task.Status = TaskItemStatus.Cascaded;
            await _taskRepo.UpdateAsync(task, ct);

            await transaction.CommitAsync(ct);

            return Created($"/api/specs/{createdSpec.Id}", new CascadeResponse(task.ToResponse(), createdSpec.ToResponse(includeVersions: true)));
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(ct);
            return Conflict("Task was already cascaded by another request.");
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
