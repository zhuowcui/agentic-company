using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Interfaces;
using AgenticCompany.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenticCompany.Infrastructure.Repositories;

public class PrincipleRepository : IPrincipleRepository
{
    private readonly AppDbContext _db;

    public PrincipleRepository(AppDbContext db) => _db = db;

    public async Task<List<Principle>> GetByNodeIdAsync(Guid nodeId, CancellationToken ct = default)
        => await _db.Principles
            .Where(p => p.NodeId == nodeId)
            .OrderBy(p => p.Order)
            .ToListAsync(ct);

    public async Task<List<Principle>> GetEffectiveAsync(Guid nodeId, CancellationToken ct = default)
    {
        var node = await _db.Nodes.FirstOrDefaultAsync(n => n.Id == nodeId, ct);
        if (node == null) return [];

        var nodeIds = new List<Guid> { nodeId };
        var currentId = node.ParentId;
        while (currentId.HasValue)
        {
            nodeIds.Add(currentId.Value);
            var parent = await _db.Nodes.FirstOrDefaultAsync(n => n.Id == currentId.Value, ct);
            if (parent == null) break;
            currentId = parent.ParentId;
        }

        return await _db.Principles
            .Where(p => nodeIds.Contains(p.NodeId))
            .OrderBy(p => p.Order)
            .ToListAsync(ct);
    }

    public async Task<Principle> CreateAsync(Principle principle, CancellationToken ct = default)
    {
        principle.Id = principle.Id == Guid.Empty ? Guid.NewGuid() : principle.Id;
        principle.CreatedAt = DateTime.UtcNow;
        principle.UpdatedAt = DateTime.UtcNow;

        _db.Principles.Add(principle);
        await _db.SaveChangesAsync(ct);
        return principle;
    }

    public async Task UpdateAsync(Principle principle, CancellationToken ct = default)
    {
        principle.UpdatedAt = DateTime.UtcNow;
        _db.Principles.Update(principle);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var principle = await _db.Principles.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (principle != null)
        {
            _db.Principles.Remove(principle);
            await _db.SaveChangesAsync(ct);
        }
    }
}
