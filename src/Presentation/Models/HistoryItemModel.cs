using System.Collections.Specialized;
using System.Reactive.Linq;
using Domain.Commands;
using Domain.Responses;
using MediatR;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using UseCase;

namespace Presentation.Models;

public class HistoryItemModel : BindableBase
{
    private readonly ISender _sender;
    private readonly WorkTimeModel _workTimeModel;
    private readonly HistoryListModel _parentListModel;

    private readonly ReactivePropertySlim<IWorkTimeResponse> _entity;
    public ReadOnlyReactivePropertySlim<IWorkTimeResponse> Entity { get; }

    public ReadOnlyReactivePropertySlim<DateTime> RecordedDate { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalWorkTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalRestTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> Overtime { get; }
    public ReactivePropertySlim<DurationEditCommandDto> Duration { get; }
    public ReactiveCollection<RestTimeEditCommandDto> RestTimes { get; }
    public ReactivePropertySlim<RestTimeEditCommandDto?> SelectedRestItem { get; }

    public HistoryItemModel(ISender sender, WorkTimeModel workTimeModel, HistoryListModel parentListModel, IWorkTimeResponse item)
    {
        _sender = sender;
        _workTimeModel = workTimeModel;
        _parentListModel = parentListModel;

        _entity = new ReactivePropertySlim<IWorkTimeResponse>(item).AddTo(Disposable);
        Entity = _entity.ToReadOnlyReactivePropertySlim(_entity.Value).AddTo(Disposable);

        RecordedDate = _entity.Select(v => v.RecordedDate).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalWorkTime = _entity.Select(x => x.TotalWorkTime).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalRestTime = _entity.Select(x => x.TotalRestTime).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        Overtime = _entity.Select(x => x.Overtime).ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        Duration = new ReactivePropertySlim<DurationEditCommandDto>().AddTo(Disposable);
        RestTimes = new ReactiveCollection<RestTimeEditCommandDto>().AddTo(Disposable);
        SelectedRestItem = new ReactivePropertySlim<RestTimeEditCommandDto?>().AddTo(Disposable);

        // リストから自身が選択されたら、エンティティをもとにフォームの内容をセットする
        _parentListModel.SelectedItem
            .Where(x => x == this)
            .Subscribe(_ =>
            {
                SetRestTimes();
                Duration.Value = _entity.Value.Duration.ToCommand();
            })
            .AddTo(Disposable);

        // 現在の記録をタイマーで更新する
        _workTimeModel.Timer
            .ObserveOnUIDispatcher()
            .Select(_ => _workTimeModel.Entity.Value)
            .Where(v => v.RecordedDate == RecordedDate.Value)
            .Subscribe(v => _entity.Value = v)
            .AddTo(Disposable);

        // RestTimeリストの選択解除に関する設定
        RestTimes.ToCollectionChanged()
            .Where(x =>
                (x.Action == NotifyCollectionChangedAction.Remove
                && x.Value == SelectedRestItem.Value)
                || x.Action == NotifyCollectionChangedAction.Reset)
            .Subscribe(_ => SelectedRestItem.Value = null)
            .AddTo(Disposable);
    }

    public async Task SaveItemAsync()
    {
        DateTime baseDate = RecordedDate.Value;

        // TODO: ここはひどいのでなんとかする
        var command = new WorkTimeEditCommandDto()
        {
            ItemId = _entity.Value.Id,
            Duration = Duration.Value.WithBaseDate(baseDate),
            RestTimes = RestTimes
                .Select(v => v with { Duration = v.Duration.WithBaseDate(baseDate) })
                .ToList()
        };

        // 対象のアイテムの画面表示を更新
        _entity.Value = await _sender.Send(new SaveWorkTime.Command(command));

        // ソートした状態で休憩時間リストを更新
        SetRestTimes();

        // データの再読み込み
        await _workTimeModel.LoadDataAsync();

        // 親モデルのデータ再読み込み。現在の編集画面も閉じてしまうので、リストは再読込しない。
        await _parentListModel.LoadMonthlyAsync(shouldRefreshList: false);
    }

    public async Task DeleteItemAsync()
    {
        // アイテムの削除
        if (_entity.Value.Id != default)
            await _sender.Send(new DeleteWorkTime.Command(_entity.Value.Id));

        // データの再読み込み
        await _workTimeModel.LoadDataAsync();
        await _parentListModel.LoadMonthlyAsync();
    }

    public void RemoveSelectedRestItem()
    {
        if (SelectedRestItem.Value is null) return;
        RestTimes.RemoveOnScheduler(SelectedRestItem.Value);
    }

    public void AddRestItem()
    {
        RestTimes.AddOnScheduler(new() { ItemId = Guid.NewGuid() });
    }

    public void SelectThisItemInList()
    {
        _parentListModel.SelectedItem.Value = this;
    }

    private void SetRestTimes()
    {
        RestTimes.ClearOnScheduler();
        RestTimes.AddRangeOnScheduler(_entity.Value.RestDurationsAll.Select(x => x.ToCommand()));
    }
}
