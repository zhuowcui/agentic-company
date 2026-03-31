using AgenticCompany.Core.Enums;

namespace AgenticCompany.Core.Entities;

public class Plan
{
    public Guid Id { get; set; }
    public Guid SpecId { get; set; }
    public string Content { get; set; } = string.Empty;
    public PlanType PlanType { get; set; }
    public PlanStatus Status { get; set; } = PlanStatus.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Spec Spec { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
