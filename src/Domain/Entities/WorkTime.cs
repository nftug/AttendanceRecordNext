using Domain.Commands;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

// 日付を跨いだらModelからは破棄すること
// (通常は日付が変わった次点でRecreate()で再取得していればOK。日付が変わっていたら自動的に破棄されている。)
public class WorkTime
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public Duration Duration { get; private set; } = null!;
    public List<RestTime> RestDurations { get; private set; } = new();

    public DateTime RecordedDate => Duration.StartedOn.Date;
    public TimeSpan TotalTime
    {
        get
        {
            // 停止状態の場合、停止時に記録した一時停止のレコードを除外する
            var restData = Duration.IsActive ? RestDurations : RestDurations.SkipLast(1);
            return Duration.TotalTime - new TimeSpan(restData.Sum(x => x.TotalTime.Ticks));
        }
    }

    // 空の記録かどうか (出勤コマンドの有効状態に使用)
    public bool IsEmpty => Id == Guid.Empty;
    // 今日の記録かどうか
    public bool IsTodayRecord => RecordedDate == DateTime.UtcNow.Date;
    // 今日の勤務が進行中 (退勤コマンドの有効状態にも使用)
    public bool IsTodayOngoing => Duration.IsActive && IsTodayRecord;
    // 休憩中
    public bool IsResting => IsTodayOngoing && RestDurations.LastOrDefault()?.IsActive == true;
    // 勤務中 (休憩コマンドの有効状態にも使用)
    public bool IsWorking => IsTodayOngoing && !IsResting;
    // 停止中 (再開の有効状態に利用)
    public bool CanRestart => IsTodayRecord && RestDurations.LastOrDefault()?.IsActive == true;

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

    /// <summary>
    /// 通常はインスタンスのコピーを返す。記録日と呼び出し時の日付が異なる場合は、現在の状態を破棄して空の記録を返す。
    /// </summary>
    /// <returns></returns>
    public WorkTime Recreate()
        => IsTodayRecord ? new(Id, Duration, RestDurations) : CreateEmpty();

    public WorkTime EditDuration(DurationEditCommandDto command)
    {
        Duration = Duration.Edit(command);
        return Recreate();
    }

    public static WorkTime CreateEmpty()
        => new() { Id = Guid.Empty, Duration = new() };

    public static WorkTime Start()
        => new() { Duration = Duration.GetStart() };

    public WorkTime Finish()
    {
        if (!IsTodayOngoing)
            throw new DomainException("Cannot finish a record which is not ongoing.");

        // 休憩中の場合、休憩状態を終了する
        if (IsResting) FinishPause();
        // 次回再開時に正しい計測時間で再開できるよう、一時停止状態を新規作成する
        Pause();

        Duration = Duration.GetFinished();
        return Recreate();
    }

    public WorkTime ToggleRest()
    {
        if (!IsTodayOngoing)
            throw new DomainException("Cannot pause a record which is not ongoing.");

        if (IsResting)
            Pause();
        else
            FinishPause();

        return Recreate();
    }

    private void Pause() => RestDurations.Add(RestTime.Start());
    private void FinishPause() => RestDurations[^1] = RestDurations[^1].Finish();

    public WorkTime Restart()
    {
        if (!IsTodayRecord)
            throw new DomainException("Cannot restart a record which is not today's.");
        if (!CanRestart)
            throw new DomainException("Not a stopped record.");

        Duration = Duration.GetRestart();
        FinishPause();

        return Recreate();
    }
}
