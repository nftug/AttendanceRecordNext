using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Services;

public class RestTimeService
{
    private readonly IRepository<RestTime> _repository;
    private readonly IWorkTimeRepository _workTimeRepository;

    public RestTimeService(IRepository<RestTime> repository, IWorkTimeRepository workTimeRepository)
    {
        _repository = repository;
        _workTimeRepository = workTimeRepository;
    }

    internal async Task StartAsync(WorkTime workTime)
    {
        var newRest = RestTime.Start();
        await _repository.CreateAsync(newRest);

        workTime.AddNewRest(newRest);
        await _workTimeRepository.UpdateAsync(workTime);
    }

    internal async Task FinishAsync(WorkTime workTime)
    {
        workTime.RestLatest.Finish();
        await _repository.UpdateAsync(workTime.RestLatest);
    }
}
