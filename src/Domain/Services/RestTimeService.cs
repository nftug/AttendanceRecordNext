using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Services;

public class RestTimeService
{
    private readonly IRepository<RestTime> _repository;

    public RestTimeService(IRepository<RestTime> repository)
    {
        _repository = repository;
    }

    internal async Task StartAsync(WorkTime workTime)
    {
        var newRest = RestTime.Start();
        await _repository.CreateAsync(newRest);
        workTime.AddNewRest(newRest);
    }

    internal async Task FinishAsync(WorkTime workTime)
    {
        workTime.RestLatest.Finish();
        await _repository.UpdateAsync(workTime.RestLatest);
    }
}
