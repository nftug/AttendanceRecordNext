using Domain.Entities;
using Domain.Interfaces;
using Domain.Responses;

namespace Domain.Services;

public class WorkTimeFactory
{
    private readonly IAppConfigRepository _configRepository;
    private readonly IWorkTimeRepository _workTimeRepository;

    public WorkTimeFactory(IAppConfigRepository configRepository, IWorkTimeRepository workTimeRepository)
    {
        _configRepository = configRepository;
        _workTimeRepository = workTimeRepository;
    }

    public async Task<WorkTime?> FindByDateAsync(DateTime date)
    {
        var entity = await _workTimeRepository.FindByDateAsync(date);
        entity?.ApplyAppConfig(_configRepository.Config);
        return entity;
    }

    public async Task<WorkTime?> FindByIdAsync(Guid itemId)
    {
        var entity = await _workTimeRepository.FindByIdAsync(itemId);
        entity?.ApplyAppConfig(_configRepository.Config);
        return entity;
    }

    public async Task<WorkTimeMonthlyTally> FindAllByMonthAsync(DateTime date)
    {
        var entities = await _workTimeRepository.FindAllByMonthAsync(date);
        if (!entities.Any()) return new();

        var appConfig = _configRepository.Config;
        entities = entities.Select(x => x.ApplyAppConfig(appConfig)).ToList();
        return new(entities);
    }

    public WorkTime Create() => WorkTime.CreateEmpty().ApplyAppConfig(_configRepository.Config);

    public WorkTime Start() => WorkTime.Start().ApplyAppConfig(_configRepository.Config);
}
