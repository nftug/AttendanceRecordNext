using Domain.Entities;
using Infrastructure.Shared;
using LiteDB;

namespace Infrastructure.DataModels;

public class WorkTimeDataModel : IDataModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime StartedOn { get; set; }
    public DateTime? FinishedOn { get; set; }
    public List<RestTimeDataModel> RestTimes { get; set; } = new();

    [BsonIgnore] public string TableName => "WorkTimes";

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
            RestTimes = entity._restDurations.Select(RestTimeDataModel.Create).ToList()
        };
}
