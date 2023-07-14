using Domain.Entities;

namespace Domain.Interfaces;

public interface IWorkTimeRepository
{
    Task CreateAsync(WorkTime entity);
    Task UpdateAsync(WorkTime entity);
    Task DeleteAsync(Guid entity);
    Task<WorkTime?> FindByIdAsync(Guid id);
    Task<WorkTime?> FindByDateAsync(DateTime date);
    // Task<List<WorkTime>> FindAllByMonthAsync(DateTime month);
}
