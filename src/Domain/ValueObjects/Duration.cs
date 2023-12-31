﻿using Domain.Commands;
using Domain.Exceptions;

namespace Domain.ValueObjects;

public record Duration
{
    public DateTime StartedOn { get; init; }
    public DateTime? FinishedOn { get; init; }

    public DateTime RecordedDate => StartedOn.Date;

    public Duration() { }

    public Duration Edit(DurationEditCommandDto command)
    {
        if (command.StartedOn > DateTime.Now || command.FinishedOn > DateTime.Now)
            throw new DomainException("未来の日付を指定することはできません。");

        if (command.StartedOn > command.FinishedOn)
            throw new DomainException("開始日が終了日よりも後に指定されています。");

        // if (command.FinishedOn is null && StartedOn.Date != DateTime.Today)
        //    throw new DomainException("Cannot set blank time on FinishedOn.");

        // if (command.StartedOn is DateTime st && st.Date != RecordedDate)
        //   throw new DomainException("Cannot set this date.");

        return new() { StartedOn = command.StartedOn, FinishedOn = command.FinishedOn };
    }

    public static Duration GetStart()
        => new() { StartedOn = DateTime.Now.TruncateMs() };

    public static Duration GetStartWithDate(DateTime date)
        => new() { StartedOn = date.Date };

    public Duration GetFinished()
        => new() { StartedOn = StartedOn, FinishedOn = DateTime.Now.TruncateMs() };

    public Duration GetRestart()
        => new() { StartedOn = StartedOn };

    public bool IsActive => !IsEmpty && FinishedOn == null;
    public bool IsEmpty => StartedOn == default;

    public TimeSpan TotalTime
    {
        get
        {
            if (IsEmpty)
            {
                return TimeSpan.Zero;
            }
            else if (IsActive)
            {
                var (startDate, now) = (StartedOn.Date, DateTime.Now.TruncateMs());
                return
                    startDate == now.Date
                    ? now - StartedOn
                    : startDate.AddDays(1) - StartedOn;
            }
            else
            {
                return (DateTime)FinishedOn! - StartedOn;
            }
        }
    }

    public DurationEditCommandDto ToCommand() => new()
    {
        StartedOn = StartedOn,
        FinishedOn = FinishedOn
    };
}