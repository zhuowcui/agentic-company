using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Interfaces;
using AgenticCompany.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenticCompany.Infrastructure.Repositories;

public class TaskItemRepository : ITaskItemRepository
{
    private readonly AppDbContext _db;
    public TaskItemRepository(AppDbContext db) => _db = db;

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<List<TaskItem>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default)
        => await _db.TaskItems
            .Where(t => t.PlanId == planId)
            .OrderBy(t => t.Order)
            .ToListAsync(ct);

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default)
    {
        task.Id = task.Id == Guid.Empty ? Guid.NewGuid() : task.Id;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        _db.TaskItems.Add(task);
        await _db.SaveChangesAsync(ct);
        return task;
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _db.TaskItems.Update(task);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var task = await _db.TaskItems.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (task != null)
        {
            _db.TaskItems.Remove(task);
            await _db.SaveChangesAsync(ct);
        }
    }
}
