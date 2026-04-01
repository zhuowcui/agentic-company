using System.ComponentModel.DataAnnotations;

namespace AgenticCompany.Api.Models;

public record CreateTaskRequest(
    [property: Required][property: MaxLength(200)] string Title,
    [property: MaxLength(50000)] string? Description,
    [property: MaxLength(200)] string? AssignedTo,
    int Order,
    Guid? TargetNodeId);

public record UpdateTaskRequest(
    [property: MaxLength(200)] string? Title,
    [property: MaxLength(50000)] string? Description,
    string? Status,
    [property: MaxLength(200)] string? AssignedTo,
    Guid? TargetNodeId,
    int? Order);

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
