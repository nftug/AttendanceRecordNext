using System.Collections.Specialized;
using System.Reactive.Linq;
using Domain.Entities;
using Domain.Responses;
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
    private readonly DateTime _nowMonth;

    private readonly ReactiveCollection<HistoryItemModel> _items;
    public ReadOnlyReactiveCollection<HistoryItemModel> Items { get; }
    public ReactivePropertySlim<HistoryItemModel?> SelectedItem { get; }
    public ReactivePropertySlim<DateTime> CurrentMonth { get; }
    public ReactivePropertySlim<WorkTimeMonthlyTally> MonthlyTally { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> MonthlyOvertime { get; }

    public HistoryListModel(ISender sender, WorkTimeModel workTimeModel)
    {
        _sender = sender;
        _workTimeModel = workTimeModel;

        _items = new ReactiveCollection<HistoryItemModel>().AddTo(Disposable);
        Items = _items.ToReadOnlyReactiveCollection().AddTo(Disposable);

        SelectedItem = new ReactivePropertySlim<HistoryItemModel?>().AddTo(Disposable);

        _nowMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        CurrentMonth = new ReactivePropertySlim<DateTime>(_nowMonth).AddTo(Disposable);

        MonthlyTally = new ReactivePropertySlim<WorkTimeMonthlyTally>(new()).AddTo(Disposable);
        MonthlyOvertime = MonthlyTally.Select(x => x.OvertimeTotal).ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        // 記録の新規追加時にリストも更新する
        _workTimeModel.IsEmpty.Inverse()
            .Select(_ => _workTimeModel.Entity.Value)
            .Subscribe(x => _items.AddOnScheduler(new(_sender, _workTimeModel, this, x)))
            .AddTo(Disposable);

        // リアルタイムで集計を更新する
        _workTimeModel.Timer
            .ObserveOnUIDispatcher()
            .Select(_ => _workTimeModel.Entity.Value)
            .Where(v => v.RecordedDate.IsSameMonth(CurrentMonth.Value))
            .Subscribe(currentWorkTime => MonthlyTally.Value = MonthlyTally.Value.RecreateFromClient(currentWorkTime))
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

    public async Task LoadMonthlyAsync(bool shouldRefreshList = true)
    {
        MonthlyTally.Value = await _sender.Send(new GetMonthlyAll.Query(CurrentMonth.Value));

        if (shouldRefreshList)
        {
            var models = MonthlyTally.Value.Items.Select(x => new HistoryItemModel(_sender, _workTimeModel, this, x));
            _items.ClearOnScheduler();
            _items.AddRangeOnScheduler(models);
        }
    }

    public async Task LoadPreviousMonthAsync()
    {
        CurrentMonth.Value = CurrentMonth.Value.AddMonths(-1);
        await LoadMonthlyAsync();
    }

    public async Task LoadNextMonthAsync()
    {
        CurrentMonth.Value = CurrentMonth.Value.AddMonths(1);
        await LoadMonthlyAsync();
    }

    public async Task LoadNowMonthAsync()
    {
        CurrentMonth.Value = _nowMonth;
        await LoadMonthlyAsync();
    }

    public async Task SelectByDateAsync(DateTime date)
    {
        if (!date.IsSameMonth(CurrentMonth.Value))
        {
            // 作成する対象月のリストを取得する
            CurrentMonth.Value = new(date.Year, date.Month, 1);
            await LoadMonthlyAsync();
        }

        var targetItemModel = Items.FirstOrDefault(x => x.RecordedDate.Value == date);
        if (targetItemModel != null)
        {
            // 既にこの日付の項目が存在する場合は選択して終了
            SelectedItem.Value = targetItemModel;
            return;
        }

        var newItem = WorkTime.CreateWithDate(date);
        var newItemModel = new HistoryItemModel(_sender, _workTimeModel, this, newItem);
        _items.AddOnScheduler(newItemModel);

        SelectedItem.Value = newItemModel;
    }
}
