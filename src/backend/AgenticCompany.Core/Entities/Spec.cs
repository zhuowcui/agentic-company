using AgenticCompany.Core.Enums;

namespace AgenticCompany.Core.Entities;

public class Spec
{
    public Guid Id { get; set; }
    public Guid NodeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public SpecStatus Status { get; set; } = SpecStatus.Draft;
    public Guid? SourceTaskId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Node Node { get; set; } = null!;
    public TaskItem? SourceTask { get; set; }
    public ICollection<SpecVersion> Versions { get; set; } = [];
    public ICollection<Plan> Plans { get; set; } = [];
}
