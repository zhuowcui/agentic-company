using System.Security.Claims;
using AgenticCompany.Api.Mapping;
using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Enums;
using AgenticCompany.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgenticCompany.Api.Controllers;

[ApiController]
[Authorize]
public class PlansController : ControllerBase
{
    private readonly IPlanRepository _planRepo;
    private readonly ISpecRepository _specRepo;
    private readonly INodeRepository _nodeRepo;
    private readonly INodeMemberRepository _memberRepo;

    public PlansController(IPlanRepository planRepo, ISpecRepository specRepo, INodeRepository nodeRepo, INodeMemberRepository memberRepo)
    {
        _planRepo = planRepo;
        _specRepo = specRepo;
        _nodeRepo = nodeRepo;
        _memberRepo = memberRepo;
    }

    private async Task<bool> IsNodeMemberAsync(Guid nodeId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return false;
        var ancestorIds = node.Path.Split('.').Select(Guid.Parse).ToHashSet();
        var memberships = await _memberRepo.GetByUserIdAsync(userId, ct);
        return memberships.Any(m => ancestorIds.Contains(m.NodeId) && m.Role != NodeRole.Viewer);
    }

    private async Task<bool> HasReadAccessAsync(Guid nodeId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return false;
        var ancestorIds = node.Path.Split('.').Select(Guid.Parse).ToHashSet();
        var memberships = await _memberRepo.GetByUserIdAsync(userId, ct);
        if (memberships.Any(m => ancestorIds.Contains(m.NodeId)))
            return true;
        var nodePathPrefix = node.Path + ".";
        return memberships.Any(m => m.Node.Path.StartsWith(nodePathPrefix));
    }

    [HttpGet("api/specs/{specId:guid}/plans")]
    public async Task<ActionResult<List<PlanResponse>>> GetBySpec(Guid specId, CancellationToken ct)
    {
        var spec = await _specRepo.GetByIdAsync(specId, ct);
        if (spec is null) return NotFound(new { error = $"Spec {specId} not found." });

        if (!await HasReadAccessAsync(spec.NodeId, ct))
            return Forbid();

        var plans = await _planRepo.GetBySpecIdAsync(specId, ct);
        return Ok(plans.Select(p => p.ToResponse()).ToList());
    }

    [HttpGet("api/plans/{id:guid}")]
    public async Task<ActionResult<PlanResponse>> GetById(Guid id, CancellationToken ct)
    {
        var plan = await _planRepo.GetByIdAsync(id, ct);
        if (plan is null) return NotFound();

        var spec = await _specRepo.GetByIdAsync(plan.SpecId, ct);
        if (spec is null) return NotFound();

        if (!await HasReadAccessAsync(spec.NodeId, ct))
            return Forbid();

        return Ok(plan.ToResponse(includeTasks: true));
    }

    [HttpPost("api/specs/{specId:guid}/plans")]
    public async Task<ActionResult<PlanResponse>> Create(Guid specId, [FromBody] CreatePlanRequest request, CancellationToken ct)
    {
        var spec = await _specRepo.GetByIdAsync(specId, ct);
        if (spec is null) return NotFound("Spec not found");

        if (!await IsNodeMemberAsync(spec.NodeId, ct))
            return Forbid();

        if (!Enum.TryParse<PlanType>(request.PlanType, true, out var planType))
            return BadRequest($"Invalid plan type. Valid values: {string.Join(", ", Enum.GetNames<PlanType>())}");

        var plan = new Plan
        {
            SpecId = specId,
            Content = request.Content,
            PlanType = planType,
            Status = PlanStatus.Draft,
        };

        var created = await _planRepo.CreateAsync(plan, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToResponse());
    }

    [HttpPut("api/plans/{id:guid}")]
    public async Task<ActionResult<PlanResponse>> Update(Guid id, [FromBody] UpdatePlanRequest request, CancellationToken ct)
    {
        var plan = await _planRepo.GetByIdAsync(id, ct);
        if (plan is null) return NotFound();

        var spec = await _specRepo.GetByIdAsync(plan.SpecId, ct);
        if (spec is null) return NotFound("Spec not found");

        if (!await IsNodeMemberAsync(spec.NodeId, ct))
            return Forbid();

        if (request.Content is not null)
            plan.Content = request.Content;
        if (request.Status != null)
        {
            if (!Enum.TryParse<PlanStatus>(request.Status, true, out var status))
                return BadRequest($"Invalid status '{request.Status}'. Valid values: {string.Join(", ", Enum.GetNames<PlanStatus>())}");
            plan.Status = status;
        }

        await _planRepo.UpdateAsync(plan, ct);
        return Ok(plan.ToResponse());
    }
}
