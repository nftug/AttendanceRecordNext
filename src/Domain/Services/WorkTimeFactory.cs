using Domain.Entities;
using Domain.Interfaces;
using Domain.Responses;

namespace Domain.Services;

public class WorkTimeFactory
{
    private readonly IAppConfigRepository _appConfigRepository;
    private readonly IWorkTimeRepository _workTimeRepository;

    public WorkTimeFactory(IAppConfigRepository appConfigRepository, IWorkTimeRepository workTimeRepository)
    {
        _appConfigRepository = appConfigRepository;
        _workTimeRepository = workTimeRepository;
    }

    public async Task<WorkTime?> FindByDateAsync(DateTime date)
    {
        var entity = await _workTimeRepository.FindByDateAsync(date);
        ApplyAppConfig(entity);
        return entity;
    }

    public async Task<WorkTime?> FindByIdAsync(Guid itemId)
    {
        var entity = await _workTimeRepository.FindByIdAsync(itemId);
        ApplyAppConfig(entity);
        return entity;
    }

    public async Task<WorkTimeMonthlyTally> FindAllByMonthAsync(DateTime date)
    {
        var entities = await _workTimeRepository.FindAllByMonthAsync(date);
        if (!entities.Any()) return new();

        var appConfig = _appConfigRepository.Config;
        entities = entities.Select(x => x.ApplyAppConfig(appConfig)).ToList();
        return new(entities);
    }

    private void ApplyAppConfig(WorkTime? entity)
    {
        if (entity is null) return;
        var appConfig = _appConfigRepository.Config;
        entity.ApplyAppConfig(appConfig);
    }
}
