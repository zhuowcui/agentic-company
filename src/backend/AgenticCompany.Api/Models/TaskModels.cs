namespace AgenticCompany.Api.Models;

public record CreateTaskRequest(string Title, string? Description, string? AssignedTo, int Order, Guid? TargetNodeId);
public record UpdateTaskRequest(string? Title, string? Description, string? Status, string? AssignedTo, Guid? TargetNodeId, int? Order);

public record CascadeRequest(Guid TargetNodeId);
public record CascadeResponse(TaskItemResponse Task, SpecResponse SpawnedSpec);

public record TaskItemResponse(
    Guid Id,
    Guid PlanId,
    string Title,
    string? Description,
    string Status,
    string? AssignedTo,
    Guid? TargetNodeId,
    Guid? SpawnedSpecId,
    int Order,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
