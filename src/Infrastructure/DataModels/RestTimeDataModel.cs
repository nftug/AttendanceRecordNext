using Domain.Entities;
using Infrastructure.Shared;
using LiteDB;

namespace Infrastructure.DataModels;

public class RestTimeDataModel : IDataModel<RestTime, RestTimeDataModel>
{
    [BsonId] public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime StartedOn { get; set; }
    public DateTime? FinishedOn { get; set; }

    public RestTime ToEntity()
        => new(Id, new() { StartedOn = StartedOn, FinishedOn = FinishedOn });

    public RestTimeDataModel Transfer(RestTime entity)
    {
        Id = entity.Id;
        StartedOn = entity.Duration.StartedOn;
        FinishedOn = entity.Duration.FinishedOn;
        return this;
    }
}
