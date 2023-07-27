using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Domain.Services;

public class WorkTimeService
{
    private readonly IWorkTimeRepository _repository;
    private readonly RestTimeService _restTimeService;

    public WorkTimeService(IWorkTimeRepository repository, RestTimeService restTimeService)
    {
        _repository = repository;
        _restTimeService = restTimeService;
    }

    public async Task<bool> CheckEntityAllowedAsync(WorkTime entity)
        => (await _repository.FindByDateAsync(entity.RecordedDate)) != null;

    public async Task<WorkTime> ToggleRestAsync(WorkTime entity)
    {
        if (!entity.IsTodayOngoing)
            throw new DomainException("Cannot pause a record which is not ongoing.");

        if (!entity.IsResting)
        {
            // 新規の休憩が開始された→休憩レコードを追加
            await _restTimeService.StartAsync(entity);
        }
        else
        {
            // 休憩が完了→休憩レコードに終了時刻を記録
            await _restTimeService.FinishAsync(entity);
        }

        return entity.Recreate();
    }

    public async Task<WorkTime> ToggleWorkAsync()
    {
        var workToday = await _repository.FindByDateAsync(DateTime.Today);
        if (workToday != null)
        {
            if (workToday.IsTodayOngoing)
                await FinishAsync(workToday);    // 退勤
            else
                await RestartAsync(workToday);   // 退勤後の勤務再開
        }
        else
        {
            workToday = await StartAsync();    // 勤務開始
        }

        return workToday.Recreate();
    }

    private async Task<WorkTime> StartAsync()
    {
        var entity = WorkTime.Start();
        await _repository.CreateAsync(entity);
        return entity;
    }

    private async Task FinishAsync(WorkTime entity)
    {
        if (!entity.IsTodayOngoing)
            throw new DomainException("Cannot finish a record which is not ongoing.");

        // 休憩中の場合、休憩状態を終了する
        if (entity.IsResting)
            await _restTimeService.FinishAsync(entity);

        // 次回再開時に正しい計測時間で再開できるよう、一時停止状態を新規作成する
        await _restTimeService.StartAsync(entity);

        entity.Finish();
        await _repository.UpdateAsync(entity);
    }

    private async Task RestartAsync(WorkTime entity)
    {
        if (!entity.IsTodayRecord)
            throw new DomainException("Cannot restart a record which is not today's.");
        if (!entity.CanRestart)
            throw new DomainException("Not a stopped record.");

        await _restTimeService.FinishAsync(entity);

        entity.Restart();
        await _repository.UpdateAsync(entity);
    }
}
