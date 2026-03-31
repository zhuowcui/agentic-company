using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;

namespace AgenticCompany.Api.Mapping;

public static class TaskMapper
{
    public static TaskItemResponse ToResponse(this TaskItem task)
    {
        return new TaskItemResponse(
            Id: task.Id,
            PlanId: task.PlanId,
            Title: task.Title,
            Description: task.Description,
            Status: task.Status.ToString(),
            AssignedTo: task.AssignedTo,
            TargetNodeId: task.TargetNodeId,
            SpawnedSpecId: task.SpawnedSpecId,
            Order: task.Order,
            CreatedAt: task.CreatedAt,
            UpdatedAt: task.UpdatedAt
        );
    }
}
