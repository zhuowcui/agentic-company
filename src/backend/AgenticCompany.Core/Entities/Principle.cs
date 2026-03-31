namespace AgenticCompany.Core.Entities;

public class Principle
{
    public Guid Id { get; set; }
    public Guid NodeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsOverride { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Node Node { get; set; } = null!;
}
