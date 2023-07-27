using Domain.Entities;
using Domain.Events;
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

    public async Task<WorkTime> ToggleWorkAsync(DomainEventPublisher eventPublisher)
    {
        var workToday = await _repository.FindByDateAsync(DateTime.Today);
        if (workToday != null)
        {
            if (workToday.IsTodayOngoing)
                workToday.Finish(eventPublisher);    // 退勤
            else
                workToday.Restart(eventPublisher);   // 退勤後の勤務再開

            eventPublisher.Publish(EntityEvent<WorkTime>.Updated(workToday));
        }
        else
        {
            workToday = WorkTime.Start();        // 勤務開始
            eventPublisher.Publish(EntityEvent<WorkTime>.Added(workToday));
        }

        return workToday.Recreate();
    }
}
