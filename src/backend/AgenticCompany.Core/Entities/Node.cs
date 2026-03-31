using AgenticCompany.Core.Enums;

namespace AgenticCompany.Core.Entities;

public class Node
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public NodeType Type { get; set; }
    public string? Description { get; set; }
    public string Path { get; set; } = string.Empty; // ltree-style path
    public int Depth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Node? Parent { get; set; }
    public ICollection<Node> Children { get; set; } = [];
    public ICollection<Principle> Principles { get; set; } = [];
    public ICollection<Spec> Specs { get; set; } = [];
    public ICollection<NodeMember> Members { get; set; } = [];
}
