namespace Domain.Config;

public record AppConfig
{
    public int StandardWorkMinutes { get; set; } = 480;
    public WorkTimeAlarmConfig WorkTimeAlarm { get; set; } = new();
    public RestTimeAlarmConfig RestTimeAlarm { get; set; } = new();
    public StatusFormatConfig StatusFormat { get; set; } = new();
    public bool ResidentNotificationEnabled { get; set; } = true;

    public record WorkTimeAlarmConfig
    {
        public bool IsEnabled { get; set; } = true;
        public int RemainingMinutes { get; set; } = 15;
        public int SnoozeMinutes { get; set; } = 5;
    }

    public record RestTimeAlarmConfig
    {
        public bool IsEnabled { get; set; } = true;
        public int ElapsedMinutes { get; set; } = 240;
        public int SnoozeMinutes { get; set; } = 5;
    }

    public record StatusFormatConfig
    {
        public string StatusFormat { get; set; } =
@"・勤務時間: {daily_work}
・休憩時間: {daily_rest}
・本日の残業時間: {daily_over}
・今月の残業時間: {monthly_over}";

        public string TimeSpanFormat { get; set; } = "h'時間'm'分'";
    }
}
