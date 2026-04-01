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

    public PlansController(IPlanRepository planRepo, ISpecRepository specRepo)
    {
        _planRepo = planRepo;
        _specRepo = specRepo;
    }

    [HttpGet("api/specs/{specId:guid}/plans")]
    public async Task<ActionResult<List<PlanResponse>>> GetBySpec(Guid specId, CancellationToken ct)
    {
        var spec = await _specRepo.GetByIdAsync(specId, ct);
        if (spec is null) return NotFound(new { error = $"Spec {specId} not found." });

        var plans = await _planRepo.GetBySpecIdAsync(specId, ct);
        return Ok(plans.Select(p => p.ToResponse()).ToList());
    }

    [HttpGet("api/plans/{id:guid}")]
    public async Task<ActionResult<PlanResponse>> GetById(Guid id, CancellationToken ct)
    {
        var plan = await _planRepo.GetByIdAsync(id, ct);
        if (plan is null) return NotFound();
        return Ok(plan.ToResponse(includeTasks: true));
    }

    [HttpPost("api/specs/{specId:guid}/plans")]
    public async Task<ActionResult<PlanResponse>> Create(Guid specId, [FromBody] CreatePlanRequest request, CancellationToken ct)
    {
        var spec = await _specRepo.GetByIdAsync(specId, ct);
        if (spec is null) return NotFound("Spec not found");

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
