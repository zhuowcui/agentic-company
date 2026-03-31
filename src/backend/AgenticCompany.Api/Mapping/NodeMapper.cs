using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;

namespace AgenticCompany.Api.Mapping;

public static class NodeMapper
{
    public static NodeResponse ToResponse(this Node node, bool includeChildren = false)
    {
        return new NodeResponse(
            Id: node.Id,
            ParentId: node.ParentId,
            Name: node.Name,
            Type: node.Type.ToString(),
            Description: node.Description,
            Path: node.Path,
            Depth: node.Depth,
            CreatedAt: node.CreatedAt,
            UpdatedAt: node.UpdatedAt,
            Children: includeChildren
                ? node.Children?.Select(c => c.ToResponse(includeChildren: true)).ToList()
                : null
        );
    }
}
