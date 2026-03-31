using AgenticCompany.Core.Enums;

namespace AgenticCompany.Core.Entities;

public class NodeMember
{
    public Guid Id { get; set; }
    public Guid NodeId { get; set; }
    public Guid UserId { get; set; }
    public NodeRole Role { get; set; }
    public DateTime JoinedAt { get; set; }

    public Node Node { get; set; } = null!;
    public User User { get; set; } = null!;
}
