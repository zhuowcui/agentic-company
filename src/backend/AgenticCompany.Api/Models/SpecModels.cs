using System.ComponentModel.DataAnnotations;

namespace AgenticCompany.Api.Models;

public record CreateSpecRequest(
    [Required][MaxLength(200)] string Title,
    [Required][MaxLength(50000)] string Content);

public record UpdateSpecRequest(
    [MaxLength(200)] string? Title,
    [Required][MaxLength(50000)] string Content);

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
