using System.ComponentModel.DataAnnotations;

namespace AgenticCompany.Api.Models;

public record CreatePlanRequest(
    [property: Required][property: MaxLength(50000)] string Content,
    [property: Required][property: MaxLength(200)] string PlanType);

public record UpdatePlanRequest(
    [property: MaxLength(50000)] string? Content,
    string? Status);

public record PlanResponse(
    Guid Id,
    Guid SpecId,
    string Content,
    string PlanType,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<TaskItemResponse>? Tasks = null
);
