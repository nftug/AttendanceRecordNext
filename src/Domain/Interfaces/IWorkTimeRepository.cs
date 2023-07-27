using Domain.Entities;

namespace Domain.Interfaces;

public interface IWorkTimeRepository : IRepository<WorkTime>
{
    Task<WorkTime?> FindByDateAsync(DateTime date);
    Task<List<WorkTime>> FindAllByMonthAsync(DateTime date);
}
