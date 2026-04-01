using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Interfaces;
using AgenticCompany.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenticCompany.Infrastructure.Repositories;

public class SpecRepository : ISpecRepository
{
    private readonly AppDbContext _db;

    public SpecRepository(AppDbContext db) => _db = db;

    public async Task<Spec?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Specs
            .Include(s => s.Versions.OrderByDescending(v => v.Version))
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<List<Spec>> GetByNodeIdAsync(Guid nodeId, CancellationToken ct = default)
        => await _db.Specs
            .Where(s => s.NodeId == nodeId)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(ct);

    public async Task<Spec> CreateAsync(Spec spec, CancellationToken ct = default)
    {
        spec.Id = spec.Id == Guid.Empty ? Guid.NewGuid() : spec.Id;
        spec.CreatedAt = DateTime.UtcNow;
        spec.UpdatedAt = DateTime.UtcNow;

        _db.Specs.Add(spec);
        await _db.SaveChangesAsync(ct);
        return spec;
    }

    public async Task UpdateAsync(Spec spec, CancellationToken ct = default)
    {
        spec.UpdatedAt = DateTime.UtcNow;
        // Don't call _db.Specs.Update(spec) — it marks the entire graph as Modified,
        // which causes new SpecVersions (with non-default GUIDs) to be treated as
        // existing entities rather than inserts. Since the spec is already tracked
        // by the change tracker, SaveChangesAsync detects modifications and additions.
        await _db.SaveChangesAsync(ct);
    }
}
