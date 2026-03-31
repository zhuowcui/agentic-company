using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Interfaces;
using AgenticCompany.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenticCompany.Infrastructure.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly AppDbContext _db;
    public PlanRepository(AppDbContext db) => _db = db;

    public async Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Plans
            .Include(p => p.Tasks.OrderBy(t => t.Order))
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<List<Plan>> GetBySpecIdAsync(Guid specId, CancellationToken ct = default)
        => await _db.Plans
            .Where(p => p.SpecId == specId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    public async Task<Plan> CreateAsync(Plan plan, CancellationToken ct = default)
    {
        plan.Id = plan.Id == Guid.Empty ? Guid.NewGuid() : plan.Id;
        plan.CreatedAt = DateTime.UtcNow;
        plan.UpdatedAt = DateTime.UtcNow;
        _db.Plans.Add(plan);
        await _db.SaveChangesAsync(ct);
        return plan;
    }

    public async Task UpdateAsync(Plan plan, CancellationToken ct = default)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        _db.Plans.Update(plan);
        await _db.SaveChangesAsync(ct);
    }
}
