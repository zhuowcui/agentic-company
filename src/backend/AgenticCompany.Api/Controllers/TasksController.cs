using AgenticCompany.Api.Mapping;
using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Enums;
using AgenticCompany.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgenticCompany.Api.Controllers;

[ApiController]
public class TasksController : ControllerBase
{
    private readonly ITaskItemRepository _taskRepo;
    private readonly IPlanRepository _planRepo;
    private readonly ISpecRepository _specRepo;
    private readonly INodeRepository _nodeRepo;

    public TasksController(
        ITaskItemRepository taskRepo,
        IPlanRepository planRepo,
        ISpecRepository specRepo,
        INodeRepository nodeRepo)
    {
        _taskRepo = taskRepo;
        _planRepo = planRepo;
        _specRepo = specRepo;
        _nodeRepo = nodeRepo;
    }

    [HttpGet("api/plans/{planId:guid}/tasks")]
    public async Task<ActionResult<List<TaskItemResponse>>> GetByPlan(Guid planId, CancellationToken ct)
    {
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

        var task = new TaskItem
        {
            PlanId = planId,
            Title = request.Title,
            Description = request.Description,
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

        if (request.Title != null) task.Title = request.Title;
        if (request.Description != null) task.Description = request.Description;
        if (request.AssignedTo != null) task.AssignedTo = request.AssignedTo;
        if (request.Status != null && Enum.TryParse<TaskItemStatus>(request.Status, true, out var status))
            task.Status = status;

        await _taskRepo.UpdateAsync(task, ct);
        return Ok(task.ToResponse());
    }

    /// <summary>
    /// Cascade a task to a child node — creates a new Spec on the target node
    /// linked back to this task. This is the core multi-layer feature.
    /// </summary>
    [HttpPost("api/tasks/{id:guid}/cascade")]
    public async Task<ActionResult<SpecResponse>> Cascade(Guid id, CancellationToken ct)
    {
        var task = await _taskRepo.GetByIdAsync(id, ct);
        if (task is null) return NotFound("Task not found");

        if (!task.TargetNodeId.HasValue)
            return BadRequest("Task has no target node. Set targetNodeId first.");

        if (task.SpawnedSpecId.HasValue)
            return BadRequest("Task has already been cascaded.");

        var targetNode = await _nodeRepo.GetByIdAsync(task.TargetNodeId.Value, ct);
        if (targetNode is null) return BadRequest("Target node not found");

        // Create a new spec on the target node from this task
        var spec = new Spec
        {
            NodeId = task.TargetNodeId.Value,
            Title = task.Title,
            Status = SpecStatus.Draft,
            SourceTaskId = task.Id,
        };

        var createdSpec = await _specRepo.CreateAsync(spec, ct);

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

        return Created($"/api/specs/{createdSpec.Id}", createdSpec.ToResponse(includeVersions: true));
    }
}
