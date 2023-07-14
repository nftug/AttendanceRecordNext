﻿using Domain.Commands;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class RestTime
{
    public Guid Id { get; } = Guid.NewGuid();
    public Duration Duration { get; private set; } = null!;

    public DateTime RecordedDate => Duration.StartedOn.Date;
    public TimeSpan TotalTime => Duration.TotalTime;

    // 今日の記録かどうか
    public bool IsTodayRecord => RecordedDate == DateTime.Now.Date;
    // 休憩が進行中
    public bool IsActive => Duration.IsActive && IsTodayRecord;

    /// <summary>
    /// インフラ層と再生成専用のコンストラクタ
    /// </summary>
    /// <param name="id"></param>
    /// <param name="record"></param>
    public RestTime(Guid id, Duration record)
    {
        Id = id;
        Duration = record;
    }

    private RestTime() { }

    public RestTime Recreate() => new(Id, Duration);

    public RestTime EditDuration(DurationEditCommandDto command)
    {
        Duration = Duration.Edit(command);
        return Recreate();
    }

    public static RestTime Start()
        => new() { Duration = Duration.GetStart() };

    public RestTime Finish()
    {
        if (!IsActive)
            throw new DomainException("Cannot finish the inactive rest time record.");

        Duration = Duration.GetFinished();
        return Recreate();
    }
}