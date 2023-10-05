using Domain.Commands;
using Domain.Entities;
using Domain.Events;
using Domain.Exceptions;

namespace Domain.Services;

public class WorkTimeService
{
    private readonly EntityEventSubscriber<WorkTime> _workTimeSubscriber;
    private readonly WorkTimeFactory _workTimeFactory;
    // private readonly EntityEventSubscriber<RestTime> _restTimeSubscriber;

    public WorkTimeService(
        EntityEventSubscriber<WorkTime> workTimeSubscriber,
        WorkTimeFactory workTimeFactory
    // EntityEventSubscriber<RestTime> restTimeSubscriber
    )
    {
        _workTimeSubscriber = workTimeSubscriber;
        _workTimeFactory = workTimeFactory;
        // _restTimeSubscriber = restTimeSubscriber;
    }

    public async Task<WorkTime> ToggleWorkAsync(EventPublisher eventPublisher)
    {
        // eventPublisher.Subscribe(_workTimeSubscriber, _restTimeSubscriber);
        eventPublisher.Subscribe(_workTimeSubscriber);

        var workToday = await _workTimeFactory.FindByDateAsync(DateTime.Today);
        if (workToday != null)
        {
            if (workToday.IsTodayOngoing)
                workToday.Finish(eventPublisher);    // 退勤
            else
                workToday.Restart(eventPublisher);   // 退勤後の勤務再開
        }
        else
        {
            workToday = _workTimeFactory.Start();        // 勤務開始
        }

        eventPublisher.Publish(EntityEvent<WorkTime>.Saved(workToday));

        return workToday;
    }

    public async Task<WorkTime> ToggleRestAsync(EventPublisher eventPublisher)
    {
        // eventPublisher.Subscribe(_workTimeSubscriber, _restTimeSubscriber);
        eventPublisher.Subscribe(_workTimeSubscriber);

        var latest =
            await _workTimeFactory.FindByDateAsync(DateTime.Today)
            ?? throw new DomainException("There is no available work item.");
        return latest.ToggleRest(eventPublisher);
    }

    public async Task<WorkTime> SaveAsync(WorkTimeEditCommandDto command, EventPublisher eventPublisher)
    {
        eventPublisher.Subscribe(_workTimeSubscriber);

        var item = await _workTimeFactory.FindByIdAsync(command.ItemId);
        if (item is null)
        {
            // 新規作成の場合、日付が被っている記録がないかを確認する
            if (await _workTimeFactory.FindByDateAsync(command.Duration.StartedOn) != null)
                throw new DomainException("Already exist of a record of the same day.");

            item = _workTimeFactory.Create();
        }

        item.Edit(command);

        eventPublisher.Publish(EntityEvent<WorkTime>.Saved(item));
        return item;
    }

    public async Task DeleteAsync(Guid itemId, EventPublisher eventPublisher)
    {
        eventPublisher.Subscribe(_workTimeSubscriber);
        var item = await _workTimeFactory.FindByIdAsync(itemId)
            ?? throw new DomainException("Not found work time item");

        eventPublisher.Publish(EntityEvent<WorkTime>.Deleted(item));
    }
}
