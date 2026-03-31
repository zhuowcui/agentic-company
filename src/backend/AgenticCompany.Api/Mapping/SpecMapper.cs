using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;

namespace AgenticCompany.Api.Mapping;

public static class SpecMapper
{
    public static SpecResponse ToResponse(this Spec spec, bool includeVersions = false)
    {
        return new SpecResponse(
            Id: spec.Id,
            NodeId: spec.NodeId,
            Title: spec.Title,
            Status: spec.Status.ToString(),
            SourceTaskId: spec.SourceTaskId,
            CreatedAt: spec.CreatedAt,
            UpdatedAt: spec.UpdatedAt,
            Versions: includeVersions
                ? spec.Versions?.Select(v => v.ToResponse()).ToList()
                : null
        );
    }

    public static SpecVersionResponse ToResponse(this SpecVersion version)
    {
        return new SpecVersionResponse(
            Id: version.Id,
            Version: version.Version,
            Content: version.Content,
            CreatedBy: version.CreatedBy,
            CreatedAt: version.CreatedAt
        );
    }
}
