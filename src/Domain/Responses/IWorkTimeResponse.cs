using Domain.ValueObjects;

namespace Domain.Responses;

public interface IWorkTimeResponse
{
    Guid Id { get; }
    Duration Duration { get; }
    IReadOnlyList<IRestTimeResponse> RestDurationsAll { get; }

    DateTime RecordedDate { get; }
    TimeSpan TotalWorkTime { get; }
    TimeSpan TotalRestTime { get; }
    TimeSpan Overtime { get; }

    bool IsEmpty { get; }
    bool IsTodayRecord { get; }
    bool IsTodayOngoing { get; }
    bool IsResting { get; }
    bool IsWorking { get; }
    bool CanRestart { get; }

    IWorkTimeResponse RecreateForClient();
}
