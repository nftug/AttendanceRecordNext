using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.DataModels;
using Infrastructure.Shared;
using LiteDB;
using LiteDB.Async;

namespace Infrastructure.Repositories;

public class WorkTimeRepository : RepositoryBase<WorkTime, WorkTimeDataModel>, IWorkTimeRepository
{
    protected override ILiteQueryableAsync<WorkTimeDataModel> GetCollectionForQuery(LiteDbCollection<WorkTime, WorkTimeDataModel> db)
        => db.Collection.Include(x => x.RestTimes).Query();

    public async Task<WorkTime?> FindByDateAsync(DateTime date)
    {
        using var workTimes = Context;
        var data = await GetCollectionForQuery(workTimes)
             .Where(x => x.StartedOn.Date == date.Date)
             .FirstOrDefaultAsync();
        return data?.ToEntity();
    }

    public async Task<List<WorkTime>> FindAllByMonthAsync(DateTime date)
    {
        var startDate = new DateTime(date.Year, date.Month, 1);
        var endDate = startDate.AddMonths(1);

        using var workTimes = Context;
        var data = await GetCollectionForQuery(workTimes)
            .Where(x => x.StartedOn.Date >= startDate && x.StartedOn.Date < endDate)
            .ToListAsync();
        return data.Select(x => x.ToEntity()).ToList();
    }
}
