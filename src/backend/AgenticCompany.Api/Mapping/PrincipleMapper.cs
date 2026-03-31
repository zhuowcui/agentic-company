using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Services;

namespace AgenticCompany.Api.Mapping;

public static class PrincipleMapper
{
    public static PrincipleResponse ToResponse(this Principle p)
    {
        return new PrincipleResponse(
            Id: p.Id,
            NodeId: p.NodeId,
            Title: p.Title,
            Content: p.Content,
            Order: p.Order,
            IsOverride: p.IsOverride,
            CreatedAt: p.CreatedAt,
            UpdatedAt: p.UpdatedAt
        );
    }

    public static EffectivePrincipleResponse ToResponse(this PrincipleInheritanceService.EffectivePrinciple ep)
    {
        return new EffectivePrincipleResponse(
            Principle: ep.Principle.ToResponse(),
            SourceNodeId: ep.SourceNodeId,
            IsInherited: ep.IsInherited,
            IsOverridden: ep.IsOverridden
        );
    }
}
