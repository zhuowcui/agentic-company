using AgenticCompany.Core.Enums;

namespace AgenticCompany.Core.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    public string? AssignedTo { get; set; }
    public Guid? TargetNodeId { get; set; }
    public Guid? SpawnedSpecId { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Plan Plan { get; set; } = null!;
    public Node? TargetNode { get; set; }
    public Spec? SpawnedSpec { get; set; }
    public ICollection<TaskDependency> Dependencies { get; set; } = [];
    public ICollection<TaskDependency> Dependents { get; set; } = [];
}
