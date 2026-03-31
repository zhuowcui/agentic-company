using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;

namespace AgenticCompany.Api.Mapping;

public static class PlanMapper
{
    public static PlanResponse ToResponse(this Plan plan, bool includeTasks = false)
    {
        return new PlanResponse(
            Id: plan.Id,
            SpecId: plan.SpecId,
            Content: plan.Content,
            PlanType: plan.PlanType.ToString(),
            Status: plan.Status.ToString(),
            CreatedAt: plan.CreatedAt,
            UpdatedAt: plan.UpdatedAt,
            Tasks: includeTasks
                ? plan.Tasks?.Select(t => t.ToResponse()).ToList()
                : null
        );
    }
}
