namespace Domain.Commands;

public record WorkTimeEditCommandDto
{
    public Guid ItemId { get; set; }
    public DurationEditCommandDto Duration { get; set; } = new();
    public List<RestTimeEditCommandDto> RestTimes { get; set; } = new();
}

public record RestTimeEditCommandDto
{
    public Guid ItemId { get; set; }
    public DurationEditCommandDto Duration { get; set; } = new();
}
