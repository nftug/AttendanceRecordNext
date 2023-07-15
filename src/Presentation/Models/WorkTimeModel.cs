using System.Reactive.Linq;
using Domain.Entities;
using MediatR;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using UseCase;

namespace Presentation.Models;

public class WorkTimeModel : BindableBase
{
    private readonly ISender _sender;
    private readonly ReactivePropertySlim<WorkTime> _entity;
    public ReactiveTimer Timer { get; }
    private WorkTime? _nextEntity;

    public ReadOnlyReactivePropertySlim<TimeSpan> TotalWorkTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalRestTime { get; }
    public ReadOnlyReactivePropertySlim<bool> IsOngoing { get; }
    public ReadOnlyReactivePropertySlim<bool> IsResting { get; }
    public ReadOnlyReactivePropertySlim<bool> IsWorking { get; }

    public WorkTimeModel(ISender sender)
    {
        _sender = sender;
        _entity = new ReactivePropertySlim<WorkTime>(WorkTime.CreateEmpty());

        TotalWorkTime = _entity.Select(x => x.TotalWorkTime).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalRestTime = _entity.Select(x => x.TotalRestTime).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        IsOngoing = _entity.Select(x => x.IsTodayOngoing).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        IsResting = _entity.Select(x => x.IsResting).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        IsWorking = _entity.Select(x => x.IsWorking).ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        Timer = new ReactiveTimer(TimeSpan.FromSeconds(1)).AddTo(Disposable);
        Timer
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(_ =>
            {
                _entity.Value = _nextEntity ?? _entity.Value.Recreate();
                _nextEntity = null;
            });
        Timer.Start();

        Task.Run(async () => _entity.Value = await _sender.Send(new GetWorkToday.Query()));
    }

    public async Task ToggleWorkAsync()
        => _nextEntity = await _sender.Send(new ToggleWork.Command());

    public async Task ToggleRestAsync()
    {
        if (!IsOngoing.Value) return;
        _nextEntity = await _sender.Send(new ToggleRest.Command());
    }
}
