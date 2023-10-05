namespace System;

public static class DateTimeExtensions
{
    public static DateTime TruncateMs(this DateTime value)
        => value.AddTicks((value.Ticks % TimeSpan.TicksPerSecond) * -1);

    public static bool IsSameMonth(this DateTime origin, DateTime target)
        => origin.Year == target.Year && origin.Month == target.Month;
}
