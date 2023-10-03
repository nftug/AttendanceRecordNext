using System.Collections.Specialized;
using System.Reactive.Linq;
using Domain.Commands;
using Domain.Entities;
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
    private readonly ReactivePropertySlim<WorkTime> _entity;
    public ReadOnlyReactivePropertySlim<WorkTime> Entity { get; }

    public ReadOnlyReactivePropertySlim<DateTime> RecordedDate { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalWorkTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalRestTime { get; }
    public ReactivePropertySlim<DurationEditCommandDto> Duration { get; }
    public ReactiveCollection<RestTimeEditCommandDto> RestTimes { get; }
    public ReactivePropertySlim<RestTimeEditCommandDto?> SelectedRestItem { get; }

    public HistoryItemModel(ISender sender, WorkTimeModel workTimeModel, WorkTime item)
    {
        _sender = sender;
        _workTimeModel = workTimeModel;
        _entity = new ReactivePropertySlim<WorkTime>(item).AddTo(Disposable);
        Entity = _entity.ToReadOnlyReactivePropertySlim(_entity.Value).AddTo(Disposable);

        RecordedDate = _entity.Select(v => v.RecordedDate).ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        TotalWorkTime = _entity.Select(x => x.TotalWorkTime).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalRestTime = _entity.Select(x => x.TotalRestTime).ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        RestTimes = new ReactiveCollection<RestTimeEditCommandDto>().AddTo(Disposable);
        SetRestTimes(item);

        SelectedRestItem = new ReactivePropertySlim<RestTimeEditCommandDto?>().AddTo(Disposable);

        Duration = new ReactivePropertySlim<DurationEditCommandDto>(item.Duration.ToCommand()).AddTo(Disposable);

        // 現在の記録をタイマーで更新する
        _workTimeModel.Timer
            .ObserveOnUIDispatcher()
            .Select(_ => _workTimeModel.Entity.Value)
            .Where(v => v.RecordedDate == RecordedDate.Value)
            .Subscribe(v => _entity.Value = v)
            .AddTo(Disposable);

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
        _entity.Value = await _sender.Send(new EditWorkTime.Command(command));
        // ソートした状態で休憩時間リストを更新
        SetRestTimes(_entity.Value);

        // 現在の日時の記録が変更された時に更新する
        await _workTimeModel.LoadDataAsync();
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

    private void SetRestTimes(WorkTime entity)
    {
        RestTimes.ClearOnScheduler();
        RestTimes.AddRangeOnScheduler(entity.RestDurationsAll.Select(x => x.ToCommand()));
    }
}
