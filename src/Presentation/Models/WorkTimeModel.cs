using System.Reactive.Linq;
using Domain.Entities;
using Domain.Responses;
using MediatR;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using UseCase;

namespace Presentation.Models;

public class WorkTimeModel : BindableBase
{
    private readonly ISender _sender;

    private readonly ReactivePropertySlim<IWorkTimeResponse> _entity;
    public ReactiveTimer Timer { get; }
    private IWorkTimeResponse? _nextEntity;

    public ReadOnlyReactivePropertySlim<IWorkTimeResponse> Entity;
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalWorkTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalRestTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> Overtime { get; }
    public ReactivePropertySlim<WorkTimeMonthlyTally> MonthlyTally { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> MonthlyOvertime { get; }
    public ReadOnlyReactivePropertySlim<bool> IsEmpty { get; }
    public ReadOnlyReactivePropertySlim<bool> IsOngoing { get; }
    public ReadOnlyReactivePropertySlim<bool> IsResting { get; }
    public ReadOnlyReactivePropertySlim<bool> IsWorking { get; }

    public WorkTimeModel(ISender sender)
    {
        _sender = sender;

        _entity = new ReactivePropertySlim<IWorkTimeResponse>(WorkTime.CreateEmpty());
        Entity = _entity.ToReadOnlyReactivePropertySlim(_entity.Value).AddTo(Disposable);

        TotalWorkTime = _entity.Select(x => x.TotalWorkTime).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalRestTime = _entity.Select(x => x.TotalRestTime).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        Overtime = _entity.Select(x => x.Overtime).ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        MonthlyTally = new ReactivePropertySlim<WorkTimeMonthlyTally>(new()).AddTo(Disposable);
        MonthlyOvertime = MonthlyTally.Select(x => x.OvertimeTotal).ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        IsEmpty = _entity.Select(x => x.IsEmpty).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        IsOngoing = _entity.Select(x => x.IsTodayOngoing).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        IsResting = _entity.Select(x => x.IsResting).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        IsWorking = _entity.Select(x => x.IsWorking).ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        Timer = new ReactiveTimer(TimeSpan.FromSeconds(1)).AddTo(Disposable);
        Timer
            .ObserveOnUIDispatcher()
            .Subscribe(_ =>
            {
                _entity.Value = _nextEntity ?? _entity.Value.RecreateForClient();
                _nextEntity = null;

                MonthlyTally.Value = MonthlyTally.Value.RecreateFromClient(_entity.Value);
            });
        Timer.Start();
    }

    public async Task LoadDataAsync()
    {
        _entity.Value = await _sender.Send(new GetWorkToday.Query());
        MonthlyTally.Value = await _sender.Send(new GetMonthlyAll.Query(_entity.Value.RecordedDate));
    }

    public async Task ToggleWorkAsync() =>
        _nextEntity = await _sender.Send(new ToggleWork.Command());

    public async Task ToggleRestAsync()
    {
        if (!IsOngoing.Value) return;
        _nextEntity = await _sender.Send(new ToggleRest.Command());
    }
}
