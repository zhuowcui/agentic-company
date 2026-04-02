using System.ComponentModel.DataAnnotations;
using AgenticCompany.Core.Enums;

namespace AgenticCompany.Api.Models;

public record CreateNodeRequest(
    [Required][MaxLength(200)] string Name,
    NodeType Type,
    [MaxLength(50000)] string? Description,
    Guid? ParentId);

public record UpdateNodeRequest(
    [Required][MaxLength(200)] string Name,
    [MaxLength(50000)] string? Description);

public record MoveNodeRequest(Guid NewParentId);

public record AddNodeMemberRequest(Guid UserId, [Required] string Role);

public record NodeMemberResponse(Guid UserId, string Email, string DisplayName, string Role, DateTime JoinedAt);

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
