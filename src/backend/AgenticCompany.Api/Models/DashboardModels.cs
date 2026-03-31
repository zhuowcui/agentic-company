namespace AgenticCompany.Api.Models;

public record NodeStatsResponse(
    Dictionary<string, int> SpecsByStatus,
    Dictionary<string, int> PlansByStatus,
    Dictionary<string, int> TasksByStatus,
    int ChildNodeCount,
    int ActivePrincipleCount
);

public record ActivityItem(
    DateTime Timestamp,
    string Type,
    string Title,
    string Status,
    Guid Id
);

public record NodeActivityResponse(
    List<ActivityItem> Activities
);

public record OrgOverviewResponse(
    Dictionary<string, int> NodesByType,
    int TotalNodes,
    int TotalSpecs,
    int TotalPlans,
    int TotalTasks,
    int MaxCascadeDepth
);
