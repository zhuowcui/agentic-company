using AgenticCompany.Core.Entities;

namespace AgenticCompany.Core.Interfaces;

public interface IPrincipleRepository
{
    Task<List<Principle>> GetByNodeIdAsync(Guid nodeId, CancellationToken ct = default);
    Task<List<Principle>> GetEffectiveAsync(Guid nodeId, CancellationToken ct = default);
    Task<Principle> CreateAsync(Principle principle, CancellationToken ct = default);
    Task UpdateAsync(Principle principle, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
