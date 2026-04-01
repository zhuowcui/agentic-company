namespace AgenticCompany.Core.Interfaces;

public record NodeStats(
    Dictionary<string, int> SpecsByStatus,
    Dictionary<string, int> PlansByStatus,
    Dictionary<string, int> TasksByStatus,
    int ChildNodeCount,
    int ActivePrincipleCount
);

public record ActivityItemDto(
    DateTime Timestamp,
    string Type,
    string Title,
    string Status,
    Guid Id
);

public record OrgOverview(
    Dictionary<string, int> NodesByType,
    int TotalNodes,
    int TotalSpecs,
    int TotalPlans,
    int TotalTasks,
    int MaxCascadeDepth
);

public interface IDashboardRepository
{
    Task<NodeStats?> GetNodeStatsAsync(Guid nodeId, CancellationToken ct = default);
    Task<List<ActivityItemDto>?> GetNodeActivityAsync(Guid nodeId, CancellationToken ct = default);
    Task<OrgOverview> GetOrgOverviewAsync(ISet<Guid> accessibleNodeIds, CancellationToken ct = default);
}
