using System.ComponentModel.DataAnnotations;

namespace AgenticCompany.Api.Models;

public record CreatePrincipleRequest(
    [Required][MaxLength(200)] string Title,
    [Required][MaxLength(50000)] string Content,
    int Order,
    bool IsOverride = false);

public record UpdatePrincipleRequest(
    [Required][MaxLength(200)] string Title,
    [Required][MaxLength(50000)] string Content,
    int Order,
    bool IsOverride);

public record PrincipleResponse(
    Guid Id,
    Guid NodeId,
    string Title,
    string Content,
    int Order,
    bool IsOverride,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record EffectivePrincipleResponse(
    PrincipleResponse Principle,
    Guid SourceNodeId,
    bool IsInherited,
    bool IsOverridden
);

public record PrincipleConflictResponse(string LocalTitle, string InheritedTitle, string Reason);
