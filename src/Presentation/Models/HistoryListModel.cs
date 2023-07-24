using System.Reactive.Linq;
using Domain.Entities;
using MediatR;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using UseCase;

namespace Presentation.Models;

public class HistoryListModel : BindableBase
{
    private readonly ISender _sender;
    private readonly WorkTimeModel _workTimeModel;

    private readonly ReactiveCollection<HistoryItemModel> _items;
    public ReadOnlyReactiveCollection<HistoryItemModel> Items { get; }

    public HistoryListModel(ISender sender, WorkTimeModel workTimeModel)
    {
        _sender = sender;
        _workTimeModel = workTimeModel;

        _items = new ReactiveCollection<HistoryItemModel>().AddTo(Disposable);
        Items = _items.ToReadOnlyReactiveCollection().AddTo(Disposable);

        _workTimeModel.IsEmpty.Inverse()
            .Select(_ => _workTimeModel.Entity.Value)
            .Subscribe(x => _items.AddOnScheduler(new(_sender, _workTimeModel, x)))
            .AddTo(Disposable);
    }

    public async Task LoadMonthlyAsync(DateTime? date = null)
    {
        date ??= DateTime.Today;
        var items = await _sender.Send(new GetMonthlyAll.Query { Date = (DateTime)date });
        SetItems(items);
    }

    private void SetItems(IEnumerable<WorkTime> items)
    {
        var models = items.Select(x => new HistoryItemModel(_sender, _workTimeModel, x));
        _items.ClearOnScheduler();
        _items.AddRangeOnScheduler(models);
    }
}
