using AgenticCompany.Core.Enums;
using AgenticCompany.Core.Interfaces;
using AgenticCompany.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenticCompany.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _db;

    public DashboardRepository(AppDbContext db) => _db = db;

    public async Task<NodeStats?> GetNodeStatsAsync(Guid nodeId, CancellationToken ct = default)
    {
        var nodeExists = await _db.Nodes.AnyAsync(n => n.Id == nodeId, ct);
        if (!nodeExists) return null;

        var specsByStatus = await _db.Specs
            .Where(s => s.NodeId == nodeId)
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, ct);

        var plansByStatus = await _db.Plans
            .Where(p => p.Spec.NodeId == nodeId)
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, ct);

        var tasksByStatus = await _db.TaskItems
            .Where(t => t.Plan.Spec.NodeId == nodeId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, ct);

        var childNodeCount = await _db.Nodes.CountAsync(n => n.ParentId == nodeId, ct);

        var activePrincipleCount = await _db.Principles.CountAsync(p => p.NodeId == nodeId, ct);

        return new NodeStats(
            specsByStatus,
            plansByStatus,
            tasksByStatus,
            childNodeCount,
            activePrincipleCount
        );
    }

    public async Task<List<ActivityItemDto>?> GetNodeActivityAsync(Guid nodeId, CancellationToken ct = default)
    {
        var nodeExists = await _db.Nodes.AnyAsync(n => n.Id == nodeId, ct);
        if (!nodeExists) return null;

        var recentSpecs = await _db.Specs
            .Where(s => s.NodeId == nodeId)
            .OrderByDescending(s => s.UpdatedAt)
            .Take(10)
            .Select(s => new ActivityItemDto(
                s.UpdatedAt,
                "spec",
                s.Title,
                s.Status.ToString(),
                s.Id
            ))
            .ToListAsync(ct);

        var recentTasks = await _db.TaskItems
            .Where(t => t.Plan.Spec.NodeId == nodeId)
            .OrderByDescending(t => t.UpdatedAt)
            .Take(10)
            .Select(t => new ActivityItemDto(
                t.UpdatedAt,
                "task",
                t.Title,
                t.Status.ToString(),
                t.Id
            ))
            .ToListAsync(ct);

        return recentSpecs
            .Concat(recentTasks)
            .OrderByDescending(a => a.Timestamp)
            .Take(20)
            .ToList();
    }

    public async Task<OrgOverview> GetOrgOverviewAsync(ISet<Guid> accessibleNodeIds, CancellationToken ct = default)
    {
        var nodeIdList = accessibleNodeIds.ToList();

        var nodesByType = await _db.Nodes
            .Where(n => nodeIdList.Contains(n.Id))
            .GroupBy(n => n.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, ct);

        var totalNodes = await _db.Nodes.CountAsync(n => nodeIdList.Contains(n.Id), ct);
        var totalSpecs = await _db.Specs.CountAsync(s => nodeIdList.Contains(s.NodeId), ct);
        var totalPlans = await _db.Plans.CountAsync(p => nodeIdList.Contains(p.Spec.NodeId), ct);
        var totalTasks = await _db.TaskItems.CountAsync(t => nodeIdList.Contains(t.Plan.Spec.NodeId), ct);

        // Cascade depth: count longest chain of Spec→Plan→Task(cascaded)→Spec→...

        // Step 1: Find all specs that were spawned from a task (cascaded specs)
        var cascadedSpecs = await _db.Specs
            .Where(s => s.SourceTaskId != null && nodeIdList.Contains(s.NodeId))
            .Select(s => new { ChildSpecId = s.Id, s.SourceTaskId })
            .ToListAsync(ct);

        // Step 2: For each source task, find its parent spec (the spec containing the plan containing that task)
        var sourceTaskIds = cascadedSpecs
            .Where(cs => cs.SourceTaskId.HasValue)
            .Select(cs => cs.SourceTaskId!.Value)
            .ToList();

        var taskParentSpecs = await _db.TaskItems
            .Where(t => sourceTaskIds.Contains(t.Id))
            .Select(t => new { TaskId = t.Id, ParentSpecId = t.Plan.SpecId })
            .ToListAsync(ct);

        var taskToParentSpec = taskParentSpecs.ToDictionary(x => x.TaskId, x => x.ParentSpecId);

        // Step 3: Build parent→child graph of spec IDs
        var childToParent = new Dictionary<Guid, Guid>();
        foreach (var cs in cascadedSpecs)
        {
            if (cs.SourceTaskId.HasValue && taskToParentSpec.TryGetValue(cs.SourceTaskId.Value, out var parentSpecId))
            {
                childToParent[cs.ChildSpecId] = parentSpecId;
            }
        }

        // Step 4: Find max depth by tracing each cascaded spec up to its root
        int maxDepth = 0;
        foreach (var specId in childToParent.Keys)
        {
            int depth = 0;
            var current = specId;
            var visited = new HashSet<Guid>();
            while (childToParent.TryGetValue(current, out var parent) && visited.Add(current))
            {
                depth++;
                current = parent;
            }
            if (depth > maxDepth) maxDepth = depth;
        }

        return new OrgOverview(
            nodesByType,
            totalNodes,
            totalSpecs,
            totalPlans,
            totalTasks,
            maxDepth
        );
    }
}
