using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Services;

public class WorkTimeService
{
    private IWorkTimeRepository _repository;

    public WorkTimeService(IWorkTimeRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> CheckEntityAllowed(WorkTime entity)
    {
        var latest = await _repository.FindByDateAsync(entity.RecordedDate);
        return latest != null;
    }
}
