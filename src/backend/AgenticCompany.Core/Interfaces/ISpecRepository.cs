using AgenticCompany.Core.Entities;

namespace AgenticCompany.Core.Interfaces;

public interface ISpecRepository
{
    Task<Spec?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Spec>> GetByNodeIdAsync(Guid nodeId, CancellationToken ct = default);
    Task<Spec> CreateAsync(Spec spec, CancellationToken ct = default);
    Task UpdateAsync(Spec spec, CancellationToken ct = default);
}
