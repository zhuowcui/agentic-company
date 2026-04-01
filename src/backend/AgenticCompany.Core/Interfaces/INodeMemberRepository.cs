using AgenticCompany.Core.Entities;

namespace AgenticCompany.Core.Interfaces;

public interface INodeMemberRepository
{
    Task<NodeMember?> GetAsync(Guid nodeId, Guid userId, CancellationToken ct = default);
    Task<List<NodeMember>> GetByNodeIdAsync(Guid nodeId, CancellationToken ct = default);
    Task<List<NodeMember>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<NodeMember> CreateAsync(NodeMember member, CancellationToken ct = default);
    Task DeleteAsync(Guid nodeId, Guid userId, CancellationToken ct = default);
}
