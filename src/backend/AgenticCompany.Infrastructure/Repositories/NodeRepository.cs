using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Interfaces;
using AgenticCompany.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenticCompany.Infrastructure.Repositories;

public class NodeRepository : INodeRepository
{
    private readonly AppDbContext _db;

    public NodeRepository(AppDbContext db) => _db = db;

    public async Task<Node?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Nodes.FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task<Node?> GetWithChildrenAsync(Guid id, CancellationToken ct = default)
    {
        var root = await _db.Nodes.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (root is null) return null;

        // Load the full subtree using the materialized path, then assemble in memory
        var descendants = await _db.Nodes
            .Where(n => n.Path.StartsWith(root.Path + "."))
            .OrderBy(n => n.Depth)
            .ToListAsync(ct);

        var lookup = new Dictionary<Guid, Node> { [root.Id] = root };
        root.Children = new List<Node>();

        foreach (var node in descendants)
        {
            node.Children = new List<Node>();
            lookup[node.Id] = node;

            if (node.ParentId.HasValue && lookup.TryGetValue(node.ParentId.Value, out var parent))
                parent.Children.Add(node);
        }

        return root;
    }

    public async Task<List<Node>> GetRootsAsync(CancellationToken ct = default)
        => await _db.Nodes
            .Where(n => n.ParentId == null)
            .OrderBy(n => n.Name)
            .ToListAsync(ct);

    public async Task<List<Node>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        if (idList.Count == 0) return [];
        return await _db.Nodes
            .Where(n => idList.Contains(n.Id))
            .OrderBy(n => n.Name)
            .ToListAsync(ct);
    }

    public async Task<List<Node>> GetAncestorsAsync(Guid nodeId, CancellationToken ct = default)
    {
        var node = await _db.Nodes.FirstOrDefaultAsync(n => n.Id == nodeId, ct);
        if (node == null) return [];

        // Parse ancestor IDs from the materialized path (all segments except the last = self)
        var pathSegments = node.Path.Split('.');
        var ancestorIds = pathSegments
            .Take(pathSegments.Length - 1)
            .Select(s => Guid.TryParse(s, out var id) ? id : (Guid?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        if (ancestorIds.Count == 0) return [];

        // Single batch query instead of N+1
        var ancestors = await _db.Nodes
            .Where(n => ancestorIds.Contains(n.Id))
            .ToListAsync(ct);

        // Return in path order (root first → immediate parent last)
        return ancestorIds
            .Select(id => ancestors.FirstOrDefault(n => n.Id == id))
            .Where(n => n is not null)
            .Select(n => n!)
            .ToList();
    }

    public async Task<List<Node>> GetDescendantsAsync(Guid nodeId, CancellationToken ct = default)
    {
        var node = await _db.Nodes.FirstOrDefaultAsync(n => n.Id == nodeId, ct);
        if (node == null) return [];

        return await _db.Nodes
            .Where(n => n.Path.StartsWith(node.Path + ".") && n.Id != nodeId)
            .OrderBy(n => n.Depth)
            .ThenBy(n => n.Name)
            .ToListAsync(ct);
    }

    public async Task<List<Node>> GetDescendantsByPathPrefixAsync(string pathPrefix, CancellationToken ct = default)
    {
        return await _db.Nodes
            .Where(n => n.Path.StartsWith(pathPrefix + "."))
            .OrderBy(n => n.Depth)
            .ThenBy(n => n.Name)
            .ToListAsync(ct);
    }

    public async Task<Node> CreateAsync(Node node, CancellationToken ct = default)
    {
        node.Id = node.Id == Guid.Empty ? Guid.NewGuid() : node.Id;
        node.CreatedAt = DateTime.UtcNow;
        node.UpdatedAt = DateTime.UtcNow;

        if (node.ParentId.HasValue)
        {
            var parent = await _db.Nodes.FirstOrDefaultAsync(n => n.Id == node.ParentId.Value, ct)
                ?? throw new InvalidOperationException($"Parent node {node.ParentId} not found");
            node.Path = $"{parent.Path}.{node.Id}";
            node.Depth = parent.Depth + 1;
        }
        else
        {
            node.Path = node.Id.ToString();
            node.Depth = 0;
        }

        _db.Nodes.Add(node);
        await _db.SaveChangesAsync(ct);
        return node;
    }

    public async Task UpdateAsync(Node node, CancellationToken ct = default)
    {
        node.UpdatedAt = DateTime.UtcNow;
        _db.Nodes.Update(node);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateRangeAsync(IEnumerable<Node> nodes, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        foreach (var node in nodes)
        {
            node.UpdatedAt = now;
        }
        _db.Nodes.UpdateRange(nodes);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var node = await _db.Nodes.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (node != null)
        {
            _db.Nodes.Remove(node);
            await _db.SaveChangesAsync(ct);
        }
    }
}
