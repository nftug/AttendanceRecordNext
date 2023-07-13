using Domain.Commands;
using Domain.Exceptions;
using Domain.Extensions;

namespace Domain.ValueObjects;

public record Duration
{
    public DateTime StartedOn { get; set; }
    public DateTime? FinishedOn { get; set; }

    public Duration() { }

    public Duration Edit(DurationEditCommandDto command)
    {
        if (command.StartedOn > DateTime.UtcNow || command.FinishedOn > DateTime.UtcNow)
            throw new DomainException("Cannot set the future date.");

        if (command.StartedOn > command.FinishedOn)
            throw new DomainException("StartedOn is larger than FinishedOn");

        if (command.FinishedOn is null && StartedOn.Date != DateTime.UtcNow.Date)
            throw new DomainException("Cannot set blank time on FinishedOn");

        StartedOn = command.StartedOn ?? StartedOn;
        FinishedOn = command.FinishedOn != default(DateTime) ? command.FinishedOn : FinishedOn;

        return Recreate();
    }

    public static Duration GetStart()
        => new() { StartedOn = DateTime.UtcNow.TruncateMs() };

    public Duration GetFinished()
        => new() { StartedOn = StartedOn, FinishedOn = DateTime.UtcNow.TruncateMs() };

    public Duration Recreate() => new() { StartedOn = StartedOn, FinishedOn = FinishedOn };

    public bool IsActive => StartedOn != default && FinishedOn == null;

    public TimeSpan TotalTime
    {
        get
        {
            if (IsActive)
            {
                var (startDate, now) = (StartedOn.Date, DateTime.UtcNow.TruncateMs());
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
}