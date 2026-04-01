using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Interfaces;
using AgenticCompany.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenticCompany.Infrastructure.Repositories;

public class NodeMemberRepository : INodeMemberRepository
{
    private readonly AppDbContext _db;

    public NodeMemberRepository(AppDbContext db) => _db = db;

    public async Task<NodeMember?> GetAsync(Guid nodeId, Guid userId, CancellationToken ct = default)
        => await _db.NodeMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.NodeId == nodeId && m.UserId == userId, ct);

    public async Task<List<NodeMember>> GetByNodeIdAsync(Guid nodeId, CancellationToken ct = default)
        => await _db.NodeMembers
            .Include(m => m.User)
            .Where(m => m.NodeId == nodeId)
            .OrderBy(m => m.Role)
            .ThenBy(m => m.JoinedAt)
            .ToListAsync(ct);

    public async Task<NodeMember> CreateAsync(NodeMember member, CancellationToken ct = default)
    {
        member.Id = member.Id == Guid.Empty ? Guid.NewGuid() : member.Id;
        member.JoinedAt = DateTime.UtcNow;

        _db.NodeMembers.Add(member);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(member).Reference(m => m.User).LoadAsync(ct);
        return member;
    }

    public async Task DeleteAsync(Guid nodeId, Guid userId, CancellationToken ct = default)
    {
        var member = await _db.NodeMembers
            .FirstOrDefaultAsync(m => m.NodeId == nodeId && m.UserId == userId, ct);
        if (member != null)
        {
            _db.NodeMembers.Remove(member);
            await _db.SaveChangesAsync(ct);
        }
    }
}
