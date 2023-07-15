using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Services;

public class WorkTimeService
{
    private readonly IWorkTimeRepository _repository;

    public WorkTimeService(IWorkTimeRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> CheckEntityAllowedAsync(WorkTime entity)
        => (await _repository.FindByDateAsync(entity.RecordedDate)) != null;
}
