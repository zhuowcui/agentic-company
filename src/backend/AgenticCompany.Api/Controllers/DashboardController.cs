using AgenticCompany.Api.Models;
using AgenticCompany.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgenticCompany.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db) => _db = db;

    [HttpGet("node/{nodeId}/stats")]
    public async Task<ActionResult<NodeStatsResponse>> GetNodeStats(Guid nodeId)
    {
        var nodeExists = await _db.Nodes.AnyAsync(n => n.Id == nodeId);
        if (!nodeExists) return NotFound();

        var specsByStatus = await _db.Specs
            .Where(s => s.NodeId == nodeId)
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var plansByStatus = await _db.Plans
            .Where(p => p.Spec.NodeId == nodeId)
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var tasksByStatus = await _db.TaskItems
            .Where(t => t.Plan.Spec.NodeId == nodeId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var childNodeCount = await _db.Nodes.CountAsync(n => n.ParentId == nodeId);

        var activePrincipleCount = await _db.Principles.CountAsync(p => p.NodeId == nodeId);

        return Ok(new NodeStatsResponse(
            specsByStatus,
            plansByStatus,
            tasksByStatus,
            childNodeCount,
            activePrincipleCount
        ));
    }

    [HttpGet("node/{nodeId}/activity")]
    public async Task<ActionResult<NodeActivityResponse>> GetNodeActivity(Guid nodeId)
    {
        var nodeExists = await _db.Nodes.AnyAsync(n => n.Id == nodeId);
        if (!nodeExists) return NotFound();

        var recentSpecs = await _db.Specs
            .Where(s => s.NodeId == nodeId)
            .OrderByDescending(s => s.UpdatedAt)
            .Take(10)
            .Select(s => new ActivityItem(
                s.UpdatedAt,
                "spec",
                s.Title,
                s.Status.ToString(),
                s.Id
            ))
            .ToListAsync();

        var recentTasks = await _db.TaskItems
            .Where(t => t.Plan.Spec.NodeId == nodeId)
            .OrderByDescending(t => t.UpdatedAt)
            .Take(10)
            .Select(t => new ActivityItem(
                t.UpdatedAt,
                "task",
                t.Title,
                t.Status.ToString(),
                t.Id
            ))
            .ToListAsync();

        var activities = recentSpecs
            .Concat(recentTasks)
            .OrderByDescending(a => a.Timestamp)
            .Take(20)
            .ToList();

        return Ok(new NodeActivityResponse(activities));
    }

    [HttpGet("org-overview")]
    public async Task<ActionResult<OrgOverviewResponse>> GetOrgOverview()
    {
        var nodesByType = await _db.Nodes
            .GroupBy(n => n.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        var totalNodes = await _db.Nodes.CountAsync();
        var totalSpecs = await _db.Specs.CountAsync();
        var totalPlans = await _db.Plans.CountAsync();
        var totalTasks = await _db.TaskItems.CountAsync();

        // Cascade depth: count longest chain of Spec→Plan→Task(Cascaded)→Spec→...
        var cascadedTasks = await _db.TaskItems
            .Where(t => t.Status == Core.Enums.TaskItemStatus.Cascaded)
            .Select(t => new { t.Id, t.Plan.Spec.SourceTaskId })
            .ToListAsync();

        var specsWithSource = await _db.Specs
            .Where(s => s.SourceTaskId != null)
            .Select(s => new { s.Id, s.SourceTaskId })
            .ToListAsync();

        // Build a map: taskId → spawned specId
        var taskToSpec = specsWithSource.ToDictionary(s => s.SourceTaskId!.Value, s => s.Id);

        // Build a map: specId → cascaded task ids from that spec's plans
        var specToCascadedTasks = cascadedTasks
            .Where(t => t.SourceTaskId != null)
            .GroupBy(t => t.SourceTaskId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(t => t.Id).ToList());

        int maxDepth = 0;
        foreach (var spec in specsWithSource)
        {
            int depth = 1;
            var visited = new HashSet<Guid> { spec.Id };
            var currentSpecId = spec.Id;

            while (specToCascadedTasks.TryGetValue(currentSpecId, out var childTasks))
            {
                bool found = false;
                foreach (var taskId in childTasks)
                {
                    if (taskToSpec.TryGetValue(taskId, out var nextSpecId) && visited.Add(nextSpecId))
                    {
                        currentSpecId = nextSpecId;
                        depth++;
                        found = true;
                        break;
                    }
                }
                if (!found) break;
            }

            if (depth > maxDepth) maxDepth = depth;
        }

        return Ok(new OrgOverviewResponse(
            nodesByType,
            totalNodes,
            totalSpecs,
            totalPlans,
            totalTasks,
            maxDepth
        ));
    }
}
