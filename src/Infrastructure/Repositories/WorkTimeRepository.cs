using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.DataModels;
using Infrastructure.Shared;

namespace Infrastructure.Repositories;

public class WorkTimeRepository : IWorkTimeRepository
{
    public async Task CreateAsync(WorkTime entity)
    {
        using var workTimes = new LiteDbCollection<WorkTimeDataModel>();
        var data = WorkTimeDataModel.Create(entity);
        await workTimes.Collection.InsertAsync(data);
    }

    public async Task UpdateAsync(WorkTime entity)
    {
        using var workTimes = new LiteDbCollection<WorkTimeDataModel>();
        var data = WorkTimeDataModel.Create(entity);
        await workTimes.Collection.UpdateAsync(data);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var workTimes = new LiteDbCollection<WorkTimeDataModel>();
        await workTimes.Collection.DeleteAsync(id);
    }

    public async Task<WorkTime?> FindByIdAsync(Guid id)
    {
        using var workTimes = new LiteDbCollection<WorkTimeDataModel>();
        var data = await workTimes.Collection.FindByIdAsync(id);
        return data?.ToEntity();
    }

    public async Task<WorkTime?> FindByDateAsync(DateTime date)
    {
        using var workTimes = new LiteDbCollection<WorkTimeDataModel>();
        var data = await workTimes.Collection.Query()
             .Where(x => x.StartedOn.Date == date.Date)
             .FirstOrDefaultAsync();
        return data?.ToEntity();
    }

    public async Task<List<WorkTime>> FindAllByMonthAsync(DateTime date)
    {
        var startDate = new DateTime(date.Year, date.Month, 1);
        var endDate = startDate.AddMonths(1);

        using var workTimes = new LiteDbCollection<WorkTimeDataModel>();
        var data = await workTimes.Collection.Query()
            .Where(x => x.StartedOn.Date >= startDate && x.StartedOn.Date < endDate)
            .ToListAsync();
        return data.Select(x => x.ToEntity()).ToList();
    }
}
