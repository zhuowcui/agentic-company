using System.Security.Claims;
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
    private readonly INodeRepository _nodeRepo;
    private readonly INodeMemberRepository _memberRepo;

    public DashboardController(IDashboardRepository dashboardRepo, INodeRepository nodeRepo, INodeMemberRepository memberRepo)
    {
        _dashboardRepo = dashboardRepo;
        _nodeRepo = nodeRepo;
        _memberRepo = memberRepo;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<bool> HasReadAccessAsync(Guid nodeId, CancellationToken ct)
    {
        var userId = GetUserId();
        var node = await _nodeRepo.GetByIdAsync(nodeId, ct);
        if (node is null) return false;
        var ancestorIds = node.Path.Split('.').Select(Guid.Parse).ToHashSet();
        var memberships = await _memberRepo.GetByUserIdAsync(userId, ct);
        return memberships.Any(m => ancestorIds.Contains(m.NodeId));
    }

    [HttpGet("node/{nodeId}/stats")]
    public async Task<ActionResult<NodeStatsResponse>> GetNodeStats(Guid nodeId, CancellationToken ct)
    {
        if (!await HasReadAccessAsync(nodeId, ct))
            return Forbid();

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
        if (!await HasReadAccessAsync(nodeId, ct))
            return Forbid();

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
        var userId = GetUserId();
        var memberships = await _memberRepo.GetByUserIdAsync(userId, ct);

        // Start with directly accessible node IDs
        var accessibleNodeIds = memberships.Select(m => m.NodeId).ToHashSet();

        // Expand to include all descendants of accessible nodes (inherited downward access)
        var descendantNodes = new HashSet<Guid>(accessibleNodeIds);
        foreach (var nodeId in accessibleNodeIds)
        {
            var descendants = await _nodeRepo.GetDescendantsAsync(nodeId, ct);
            foreach (var d in descendants)
                descendantNodes.Add(d.Id);
        }

        var overview = await _dashboardRepo.GetOrgOverviewAsync(descendantNodes, ct);

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
