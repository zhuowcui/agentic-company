namespace AgenticCompany.Api.Models;

public record CreateSpecRequest(string Title, string Content);
public record UpdateSpecRequest(string Content);

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
