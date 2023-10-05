using Domain.Entities;
using Infrastructure.Shared;
using LiteDB;

namespace Infrastructure.DataModels;

public class WorkTimeDataModel : IDataModel<WorkTime, WorkTimeDataModel>
{
    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime StartedOn { get; set; }
    public DateTime? FinishedOn { get; set; }
    // [BsonRef(nameof(RestTime))] public List<RestTimeDataModel> RestTimes { get; set; } = null!;
    public List<RestTimeDataModel> RestTimes { get; set; } = null!;

    public WorkTime ToEntity()
        => new(
                Id,
                new() { StartedOn = StartedOn, FinishedOn = FinishedOn },
                RestTimes?.Select(x => x.ToEntity()).ToList() ?? new()
            );

    public WorkTimeDataModel Transfer(WorkTime entity)
    {
        Id = entity.Id;
        StartedOn = entity.Duration.StartedOn;
        FinishedOn = entity.Duration.FinishedOn;

        // BsonRefを指定しているため、IDのみ保存される
        // RestTimes = entity.RestDurationsAll.Select(x => new RestTimeDataModel { Id = x.Id }).ToList();

        RestTimes = entity.RestDurationsAll
            .OfType<RestTime>()
            .Select(x => new RestTimeDataModel().Transfer(x))
            .ToList();

        return this;
    }
}
