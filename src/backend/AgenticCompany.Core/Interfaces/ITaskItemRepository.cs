using AgenticCompany.Core.Entities;

namespace AgenticCompany.Core.Interfaces;

public interface ITaskItemRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<TaskItem>> GetByPlanIdAsync(Guid planId, CancellationToken ct = default);
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default);
    Task UpdateAsync(TaskItem task, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
