using System.Reactive.Linq;
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

    public ReadOnlyReactivePropertySlim<DateTime> RecordedDate { get; }

    public HistoryItemModel(ISender sender, WorkTimeModel workTimeModel, WorkTime item)
    {
        _sender = sender;
        _workTimeModel = workTimeModel;
        _entity = new ReactivePropertySlim<WorkTime>(item).AddTo(Disposable);
        Entity = _entity.ToReadOnlyReactivePropertySlim(_entity.Value).AddTo(Disposable);

        RecordedDate = _entity.Select(v => v.RecordedDate).ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        Observable.CombineLatest(_workTimeModel.IsWorking, _workTimeModel.IsResting)
            .Select(_ => _workTimeModel.Entity.Value)
            .Where(v => v.RecordedDate == RecordedDate.Value)
            .Subscribe(v => _entity.Value = v)
            .AddTo(Disposable);
    }
}
