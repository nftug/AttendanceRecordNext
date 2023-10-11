using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.DataModels;
using Infrastructure.Shared;
using LiteDB;

namespace Infrastructure.Repositories;

public class WorkTimeRepository : RepositoryBase<WorkTime, WorkTimeDataModel>, IWorkTimeRepository
{
    public WorkTimeRepository(IAppInfo appInfo) : base(appInfo)
    {
    }

    public async Task<WorkTime?> FindByDateAsync(DateTime date)
        => await UseCollectionQuery(async query =>
        {
            var data = await query.Where(x => x.StartedOn.Date == date.Date).FirstOrDefaultAsync();
            return data?.ToEntity();
        });

    public async Task<List<WorkTime>> FindAllByMonthAsync(DateTime date)
        => await UseCollectionQuery(async query =>
        {
            var data = await query
                .Where(x => date.Year == x.StartedOn.Year && date.Month == x.StartedOn.Month)
                .OrderBy(x => x.StartedOn)
                .ToListAsync();
            return data.Select(x => x.ToEntity()).ToList();
        });
}
