namespace Domain.Extensions;

public static class DateTimeExtensions
{
    public static DateTime TruncateMs(this DateTime value)
        => value.AddTicks((value.Ticks % TimeSpan.TicksPerSecond) * -1);
}
