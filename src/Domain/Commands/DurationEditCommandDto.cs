namespace Domain.Commands;

public record DurationEditCommandDto
{
    public DateTime? StartedOn { get; set; }
    public DateTime? FinishedOn { get; set; }
}
