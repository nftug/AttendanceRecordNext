using System.Reactive.Linq;
using Domain.Commands;
using Domain.Entities;
using MediatR;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.Models;

public class HistoryItemModel : BindableBase
{
    private readonly ISender _sender;
    private readonly WorkTimeModel _workTimeModel;
    private readonly ReactivePropertySlim<WorkTime> _entity;
    public ReadOnlyReactivePropertySlim<WorkTime> Entity { get; }

    public ReactiveCollection<DurationEditCommandDto> RestTimeDurations { get; }

    public ReadOnlyReactivePropertySlim<DateTime> RecordedDate { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalWorkTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalRestTime { get; }

    public ReactivePropertySlim<DurationEditCommandDto> Duration { get; }

    public HistoryItemModel(ISender sender, WorkTimeModel workTimeModel, WorkTime item)
    {
        _sender = sender;
        _workTimeModel = workTimeModel;
        _entity = new ReactivePropertySlim<WorkTime>(item).AddTo(Disposable);
        Entity = _entity.ToReadOnlyReactivePropertySlim(_entity.Value).AddTo(Disposable);

        RecordedDate = _entity.Select(v => v.RecordedDate).ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        TotalWorkTime = _entity.Select(x => x.TotalWorkTime).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalRestTime = _entity.Select(x => x.TotalRestTime).ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        RestTimeDurations = new ReactiveCollection<DurationEditCommandDto>().AddTo(Disposable);
        SetRestTimeDurations(item);

        Duration = new ReactivePropertySlim<DurationEditCommandDto>(item.Duration.ToCommand()).AddTo(Disposable);

        // 現在の記録をタイマーで更新する
        _workTimeModel.Timer
            .ObserveOnUIDispatcher()
            .Select(_ => _workTimeModel.Entity.Value)
            .Where(v => v.RecordedDate == RecordedDate.Value)
            .Subscribe(v => _entity.Value = v)
            .AddTo(Disposable);

        // 外部から記録が更新された場合、フォームの内容も更新する
        Observable.CombineLatest(_workTimeModel.IsWorking, _workTimeModel.IsResting)
            .Select(_ => _workTimeModel.Entity.Value)
            .Where(v => v.RecordedDate == RecordedDate.Value)
            .Subscribe(v =>
            {
                Duration.Value = v.Duration.ToCommand();
                SetRestTimeDurations(v);
            })
            .AddTo(Disposable);
    }

    private void SetRestTimeDurations(WorkTime entity)
    {
        RestTimeDurations.ClearOnScheduler();
        RestTimeDurations.AddRangeOnScheduler(entity.RestDurationsAll.Select(x => x.Duration.ToCommand()));
    }
}
