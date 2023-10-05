using Domain.Commands;
using Domain.ValueObjects;

namespace Domain.Responses;

public interface IRestTimeResponse
{
    Guid Id { get; }
    Duration Duration { get; }
    TimeSpan TotalTime { get; }
    bool IsActive { get; }
    RestTimeEditCommandDto ToCommand();
}
