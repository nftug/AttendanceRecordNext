using Domain.Entities;
using Domain.Events;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Domain.Services;

public class WorkTimeService
{
    private readonly IWorkTimeRepository _repository;
    private readonly EntityEventSubscriber<WorkTime> _workTimeSubscriber;
    // private readonly EntityEventSubscriber<RestTime> _restTimeSubscriber;

    public WorkTimeService(
        IWorkTimeRepository repository,
        EntityEventSubscriber<WorkTime> workTimeSubscriber
    // EntityEventSubscriber<RestTime> restTimeSubscriber
    )
    {
        _repository = repository;
        _workTimeSubscriber = workTimeSubscriber;
        // _restTimeSubscriber = restTimeSubscriber;
    }

    public async Task<bool> CheckEntityAllowedAsync(WorkTime entity)
        => (await _repository.FindByDateAsync(entity.RecordedDate)) != null;

    public async Task<WorkTime> ToggleWorkAsync(EventPublisher eventPublisher)
    {
        // eventPublisher.Subscribe(_workTimeSubscriber, _restTimeSubscriber);
        eventPublisher.Subscribe(_workTimeSubscriber);

        var workToday = await _repository.FindByDateAsync(DateTime.Today);
        if (workToday != null)
        {
            if (workToday.IsTodayOngoing)
                workToday.Finish(eventPublisher);    // 退勤
            else
                workToday.Restart(eventPublisher);   // 退勤後の勤務再開
        }
        else
        {
            workToday = WorkTime.Start();        // 勤務開始
        }

        eventPublisher.Publish(EntityEvent<WorkTime>.Saved(workToday));

        return workToday;
    }

    public async Task<WorkTime> ToggleRestAsync(EventPublisher eventPublisher)
    {
        // eventPublisher.Subscribe(_workTimeSubscriber, _restTimeSubscriber);
        eventPublisher.Subscribe(_workTimeSubscriber);

        var latest =
            await _repository.FindByDateAsync(DateTime.Today)
            ?? throw new DomainException("There is no available work item.");
        return latest.ToggleRest(eventPublisher);
    }
}
