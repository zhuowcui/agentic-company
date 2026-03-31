using AgenticCompany.Core.Entities;

namespace AgenticCompany.Core.Interfaces;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Plan>> GetBySpecIdAsync(Guid specId, CancellationToken ct = default);
    Task<Plan> CreateAsync(Plan plan, CancellationToken ct = default);
    Task UpdateAsync(Plan plan, CancellationToken ct = default);
}
