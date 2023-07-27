using Domain.Commands;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

// 日付を跨いだらModelからは破棄すること
// (通常は日付が変わった次点でRecreate()で再取得していればOK。日付が変わっていたら自動的に破棄されている。)
public class WorkTime : IEntity
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public Duration Duration { get; private set; } = null!;
    public List<RestTime> RestDurationsAll { get; } = new();

    // 停止状態の場合、停止時に記録した一時停止のレコードを除外する
    public IReadOnlyList<RestTime> RestDurations
        => (Duration.IsActive ? RestDurationsAll : RestDurationsAll.SkipLast(1)).ToList();

    public DateTime RecordedDate => Duration.RecordedDate;

    /// <summary>
    /// 総休憩時間
    /// </summary>
    public TimeSpan TotalRestTime => new(RestDurations.Sum(x => x.TotalTime.Ticks));

    /// <summary>
    /// 総勤務時間
    /// </summary>
    public TimeSpan TotalWorkTime => Duration.TotalTime - TotalRestTime;

    /// <summary>
    ///  空の記録かどうか (出勤コマンドの有効状態に使用)
    /// </summary>
    public bool IsEmpty => Id == Guid.Empty;

    /// <summary>
    /// 今日の記録かどうか
    /// </summary>
    public bool IsTodayRecord => RecordedDate == DateTime.Today;

    /// <summary>
    /// 今日の勤務が進行中 (退勤コマンドの有効状態にも使用)
    /// </summary>
    public bool IsTodayOngoing => Duration.IsActive && IsTodayRecord;

    /// <summary>
    /// 休憩中
    /// </summary>
    public bool IsResting => IsTodayOngoing && RestDurationsAll.LastOrDefault()?.IsActive == true;

    /// <summary>
    /// 勤務中
    /// </summary>
    public bool IsWorking => IsTodayOngoing && !IsResting;

    /// <summary>
    /// 停止中 (再開の有効状態に利用)
    /// </summary>
    public bool CanRestart => IsTodayRecord && RestDurationsAll.LastOrDefault()?.IsActive == true;

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
        RestDurationsAll = restRecords.OrderBy(x => x.Duration.StartedOn).ToList();
    }

    private WorkTime() { }

    /// <summary>
    /// 通常はインスタンスのコピーを返す。記録日と呼び出し時の日付が異なる場合は、現在の状態を破棄して空の記録を返す。
    /// </summary>
    /// <returns></returns>
    public WorkTime Recreate()
        => IsTodayRecord ? new(Id, Duration, RestDurationsAll) : CreateEmpty();

    public WorkTime EditDuration(DurationEditCommandDto command)
    {
        Duration = Duration.Edit(command);
        return Recreate();
    }

    // ドメインサービスが永続化のために使う最新の休憩レコード
    internal RestTime RestLatest => RestDurationsAll[^1];

    public static WorkTime CreateEmpty()
        => new() { Id = Guid.Empty, Duration = new() };

    internal static WorkTime Start()
        => new() { Duration = Duration.GetStart() };

    internal WorkTime Finish()
    {
        if (!IsTodayOngoing)
            throw new DomainException("Cannot finish a record which is not ongoing.");

        Duration = Duration.GetFinished();
        return Recreate();
    }

    internal void AddNewRest(RestTime restTime) => RestDurationsAll.Add(restTime);
    // internal void SetFinishedRest(RestTime restTime) => RestDurationsAll[^1] = restTime;

    internal WorkTime Restart()
    {
        if (!IsTodayRecord)
            throw new DomainException("Cannot restart a record which is not today's.");

        Duration = Duration.GetRestart();
        return Recreate();
    }
}
