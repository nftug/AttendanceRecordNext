namespace Domain.Responses;

public class WorkTimeMonthlyTally
{
    public DateTime StartDate => Items.Min(x => x.RecordedDate);
    public DateTime EndDate => Items.Max(x => x.RecordedDate);

    public IReadOnlyList<IWorkTimeResponse> Items { get; }
    public TimeSpan WorkTimeTotal => new(Items.Sum(x => x.TotalWorkTime.Ticks));
    public TimeSpan OvertimeTotal => new(Items.Sum(x => x.Overtime.Ticks));

    public WorkTimeMonthlyTally(IEnumerable<IWorkTimeResponse> monthlyItems)
    {
        Items = monthlyItems.OrderBy(x => x.RecordedDate).ToList();
    }

    public WorkTimeMonthlyTally()
    {
        Items = new List<IWorkTimeResponse>();
    }

    public WorkTimeMonthlyTally RecreateFromClient(IWorkTimeResponse entity)
    {
        var target = Items.SingleOrDefault(x => x.Id == entity.Id);
        if (target is null) return this;

        var items = Items.ToList();
        int index = items.IndexOf(target);
        items[index] = entity;

        return new(items);
    }
}
