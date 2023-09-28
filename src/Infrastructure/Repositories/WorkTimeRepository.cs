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
        => db.Collection.Query();

    public Task<WorkTime?> FindByDateAsync(DateTime date)
        => UseCollectionQuery(async query =>
        {
            var data = await query.Where(x => x.StartedOn.Date == date.Date).FirstOrDefaultAsync();
            return data?.ToEntity();
        });

    public Task<List<WorkTime>> FindAllByMonthAsync(DateTime date)
        => UseCollectionQuery(async query =>
        {
            var data = await query
                .Where(x => date.Year == x.StartedOn.Year && date.Month == x.StartedOn.Month)
                .ToListAsync();
            return data.Select(x => x.ToEntity()).ToList();
        });
}
