namespace Domain.Commands;

public record DurationEditCommandDto
{
    public DateTime StartedOn { get; set; }
    public DateTime? FinishedOn { get; set; }

    public DurationEditCommandDto WithBaseDate(DateTime baseDate) =>
        new()
        {
            StartedOn = (DateTime)GetDateTimeWithBase(StartedOn, baseDate)!,
            FinishedOn = GetDateTimeWithBase(FinishedOn, baseDate)
        };

    private static DateTime? GetDateTimeWithBase(DateTime? time, DateTime baseDate)
    {
        if (time is not DateTime dt) return null;
        return new(baseDate.Year, baseDate.Month, baseDate.Day, dt.Hour, dt.Minute, dt.Second);
    }
}
