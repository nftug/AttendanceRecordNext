using Domain.Entities;
using Infrastructure.Shared;

namespace Infrastructure.DataModels;

public class WorkTimeDataModel : IDataModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime StartedOn { get; set; }
    public DateTime? FinishedOn { get; set; }
    public List<RestTimeDataModel> RestTimes { get; set; } = new();

    public string TableName => "WorkTimes";

    public WorkTime ToEntity()
        => new(
                Id,
                new() { StartedOn = StartedOn, FinishedOn = FinishedOn },
                RestTimes.Select(x => x.ToEntity()).ToList()
            );

    public static WorkTimeDataModel Create(WorkTime entity)
        => new()
        {
            Id = entity.Id,
            StartedOn = entity.Duration.StartedOn,
            FinishedOn = entity.Duration.FinishedOn,
            RestTimes = entity.RestDurations.Select(RestTimeDataModel.Create).ToList()
        };

    public override string ToString()
        => $"Id={Id}, StartedOn={StartedOn}, FinishedOn={FinishedOn}, RestTimesCount={RestTimes.Count}";
}
