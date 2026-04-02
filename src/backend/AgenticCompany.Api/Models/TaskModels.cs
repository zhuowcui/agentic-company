using System.ComponentModel.DataAnnotations;

namespace AgenticCompany.Api.Models;

public record CreateTaskRequest(
    [Required][MaxLength(200)] string Title,
    [MaxLength(50000)] string? Description,
    [MaxLength(200)] string? AssignedTo,
    int Order,
    Guid? TargetNodeId);

public record UpdateTaskRequest(
    [MaxLength(200)] string? Title,
    [MaxLength(50000)] string? Description,
    string? Status,
    [MaxLength(200)] string? AssignedTo,
    Guid? TargetNodeId,
    int? Order,
    bool ClearAssignedTo = false,
    bool ClearTargetNodeId = false);

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
