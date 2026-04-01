using AgenticCompany.Core.Entities;

namespace AgenticCompany.Core.Interfaces;

public interface INodeRepository
{
    Task<Node?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Node?> GetWithChildrenAsync(Guid id, CancellationToken ct = default);
    Task<List<Node>> GetRootsAsync(CancellationToken ct = default);
    Task<List<Node>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<List<Node>> GetAncestorsAsync(Guid nodeId, CancellationToken ct = default);
    Task<List<Node>> GetDescendantsAsync(Guid nodeId, CancellationToken ct = default);
    Task<List<Node>> GetDescendantsByPathPrefixAsync(string pathPrefix, CancellationToken ct = default);
    Task<Node> CreateAsync(Node node, CancellationToken ct = default);
    Task UpdateAsync(Node node, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<Node> nodes, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
