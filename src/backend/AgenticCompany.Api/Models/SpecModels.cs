using System.ComponentModel.DataAnnotations;

namespace AgenticCompany.Api.Models;

public record CreateSpecRequest(
    [property: Required][property: MaxLength(200)] string Title,
    [property: Required][property: MaxLength(50000)] string Content);

public record UpdateSpecRequest(
    [property: MaxLength(200)] string? Title,
    [property: Required][property: MaxLength(50000)] string Content);

public record SpecResponse(
    Guid Id,
    Guid NodeId,
    string Title,
    string Status,
    Guid? SourceTaskId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<SpecVersionResponse>? Versions = null
);

public record SpecVersionResponse(
    Guid Id,
    int Version,
    string Content,
    string? CreatedBy,
    DateTime CreatedAt
);
