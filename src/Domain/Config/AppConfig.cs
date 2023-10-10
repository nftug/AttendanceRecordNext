namespace Domain.Config;

public record AppConfig
{
    public int StandardWorkMinutes { get; set; } = 480;
    public WorkTimeAlarmConfig WorkTimeAlarm { get; set; } = new();
    public RestTimeAlarmConfig RestTimeAlarm { get; set; } = new();

    public record WorkTimeAlarmConfig
    {
        public bool IsEnabled { get; set; } = true;
        public int BeforeMinutes { get; set; } = 15;
        public int SnoozeMinutes { get; set; } = 5;
    }

    public record RestTimeAlarmConfig
    {
        public bool IsEnabled { get; set; } = true;
        public int AfterMinutes { get; set; } = 240;
        public int SnoozeMinutes { get; set; } = 5;
    }
}
