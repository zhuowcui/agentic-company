using AgenticCompany.Core.Enums;

namespace AgenticCompany.Api.Models;

public record CreateNodeRequest(string Name, NodeType Type, string? Description, Guid? ParentId);
public record UpdateNodeRequest(string Name, string? Description);
public record MoveNodeRequest(Guid NewParentId);

public record NodeResponse(
    Guid Id,
    Guid? ParentId,
    string Name,
    string Type,
    string? Description,
    string Path,
    int Depth,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<NodeResponse>? Children = null
);
