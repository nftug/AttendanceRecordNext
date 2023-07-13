using Domain.Commands;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

// 日付を跨いだらModelからは破棄すること
public class WorkTime
{
    public Guid Id { get; } = Guid.NewGuid();
    public Duration Duration { get; private set; } = null!;
    public List<RestTime> RestDurations { get; private set; } = new();

    public DateTime RecordedDate => Duration.StartedOn.Date;
    public TimeSpan TotalTime
    {
        get
        {
            var restData = Duration.IsActive ? RestDurations : RestDurations.SkipLast(1);
            return Duration.TotalTime - new TimeSpan(restData.Sum(x => x.TotalTime.Ticks));
        }
    }

    // 今日の記録かどうか
    public bool IsTodayRecord => RecordedDate == DateTime.UtcNow.Date;
    // 今日の勤務が進行中 (退勤コマンドの有効状態にも使用)
    public bool IsTodayOngoing => Duration.IsActive && IsTodayRecord;
    // 休憩中
    public bool IsResting => IsTodayOngoing && RestDurations.LastOrDefault()?.IsActive == true;
    // 勤務中 (休憩コマンドの有効状態にも使用)
    public bool IsWorking => IsTodayOngoing && !IsResting;

    /// <summary>
    /// インフラ層と再生成専用のコンストラクタ
    /// </summary>
    /// <param name="id"></param>
    /// <param name="record"></param>
    /// <param name="restRecords"></param>
    public WorkTime(Guid id, Duration record, IReadOnlyList<RestTime> restRecords)
    {
        Id = id;
        Duration = record;
        RestDurations = restRecords.OrderBy(x => x.Duration.StartedOn).ToList();
    }

    private WorkTime() { }

    public WorkTime Recreate() => new(Id, Duration, RestDurations);

    public WorkTime EditDuration(DurationEditCommandDto command)
    {
        Duration = Duration.Edit(command);
        return Recreate();
    }

    public static WorkTime Start()
        => new() { Duration = Duration.GetStart() };

    public WorkTime Finish()
    {
        if (!IsTodayOngoing)
            throw new DomainException("Cannot finish a record which is not ongoing.");

        // 休憩中の場合、休憩状態を終了する
        if (IsResting) ToggleRest();

        Duration = Duration.GetFinished();
        return Recreate();
    }

    public WorkTime ToggleRest()
    {
        if (!IsTodayOngoing)
            throw new DomainException("Cannot pause a record which is not ongoing.");

        if (IsResting)
            RestDurations[^1] = RestDurations[^1].Finish();
        else
            RestDurations.Add(RestTime.Start());

        return Recreate();
    }
}
