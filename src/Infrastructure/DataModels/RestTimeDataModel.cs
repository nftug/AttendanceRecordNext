using Domain.Entities;

namespace Infrastructure.DataModels;

public class RestTimeDataModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime StartedOn { get; set; }
    public DateTime? FinishedOn { get; set; }

    public RestTime ToEntity()
        => new(Id, new() { StartedOn = StartedOn, FinishedOn = FinishedOn });

    public static RestTimeDataModel Create(RestTime entity)
        => new()
        {
            Id = entity.Id,
            StartedOn = entity.Duration.StartedOn,
            FinishedOn = entity.Duration.FinishedOn
        };

    public override string ToString()
        => $"Id={Id}, StartedOn={StartedOn}, FinishedOn={FinishedOn}";
}
