using AgenticCompany.Core.Enums;

namespace AgenticCompany.Core.Entities;

public class NodeMember
{
    public Guid Id { get; set; }
    public Guid NodeId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }

    public Node Node { get; set; } = null!;
}
