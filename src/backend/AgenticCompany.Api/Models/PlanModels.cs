namespace AgenticCompany.Api.Models;

public record CreatePlanRequest(string Content, string PlanType);
public record UpdatePlanRequest(string Content, string? Status);

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
