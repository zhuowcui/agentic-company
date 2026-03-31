using AgenticCompany.Api.Models;
using AgenticCompany.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgenticCompany.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardRepository _dashboardRepo;

    public DashboardController(IDashboardRepository dashboardRepo) => _dashboardRepo = dashboardRepo;

    [HttpGet("node/{nodeId}/stats")]
    public async Task<ActionResult<NodeStatsResponse>> GetNodeStats(Guid nodeId, CancellationToken ct)
    {
        var stats = await _dashboardRepo.GetNodeStatsAsync(nodeId, ct);
        if (stats is null) return NotFound();

        return Ok(new NodeStatsResponse(
            stats.SpecsByStatus,
            stats.PlansByStatus,
            stats.TasksByStatus,
            stats.ChildNodeCount,
            stats.ActivePrincipleCount
        ));
    }

    [HttpGet("node/{nodeId}/activity")]
    public async Task<ActionResult<NodeActivityResponse>> GetNodeActivity(Guid nodeId, CancellationToken ct)
    {
        var activities = await _dashboardRepo.GetNodeActivityAsync(nodeId, ct);
        if (activities is null) return NotFound();

        var items = activities
            .Select(a => new ActivityItem(a.Timestamp, a.Type, a.Title, a.Status, a.Id))
            .ToList();

        return Ok(new NodeActivityResponse(items));
    }

    [HttpGet("org-overview")]
    public async Task<ActionResult<OrgOverviewResponse>> GetOrgOverview(CancellationToken ct)
    {
        var overview = await _dashboardRepo.GetOrgOverviewAsync(ct);

        return Ok(new OrgOverviewResponse(
            overview.NodesByType,
            overview.TotalNodes,
            overview.TotalSpecs,
            overview.TotalPlans,
            overview.TotalTasks,
            overview.MaxCascadeDepth
        ));
    }
}
