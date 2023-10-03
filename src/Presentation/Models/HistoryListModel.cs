using System.Collections.Specialized;
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
    public ReactivePropertySlim<HistoryItemModel?> SelectedItem { get; }
    public ReactivePropertySlim<DateTime> CurrentMonth { get; }

    public HistoryListModel(ISender sender, WorkTimeModel workTimeModel)
    {
        _sender = sender;
        _workTimeModel = workTimeModel;

        _items = new ReactiveCollection<HistoryItemModel>().AddTo(Disposable);
        Items = _items.ToReadOnlyReactiveCollection().AddTo(Disposable);

        SelectedItem = new ReactivePropertySlim<HistoryItemModel?>().AddTo(Disposable);

        DateTime currentMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        CurrentMonth = new ReactivePropertySlim<DateTime>(currentMonth).AddTo(Disposable);

        // 記録の新規追加時にリストも更新する
        _workTimeModel.IsEmpty.Inverse()
            .Select(_ => _workTimeModel.Entity.Value)
            .Subscribe(x => _items.AddOnScheduler(new(_sender, _workTimeModel, x)))
            .AddTo(Disposable);

        // SelectedItemを指す記録がリストから消去された時、SelectedItemをクリアする
        Items.ToCollectionChanged()
            .Where(x =>
                (x.Action == NotifyCollectionChangedAction.Remove
                && x.Value == SelectedItem.Value)
                || x.Action == NotifyCollectionChangedAction.Reset)
            .Subscribe(_ => SelectedItem.Value = null)
            .AddTo(Disposable);
    }

    public async Task LoadMonthlyAsync()
    {
        var items = await _sender.Send(new GetMonthlyAll.Query { Date = CurrentMonth.Value });
        SetItems(items);
    }

    private void SetItems(IEnumerable<WorkTime> items)
    {
        var models = items.Select(x => new HistoryItemModel(_sender, _workTimeModel, x));
        _items.ClearOnScheduler();
        _items.AddRangeOnScheduler(models);
    }
}
