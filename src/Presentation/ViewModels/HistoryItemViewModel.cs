using Domain.Commands;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class HistoryItemViewModel : ViewModelBase
{
    internal HistoryItemModel Model { get; }

    public ReadOnlyReactivePropertySlim<DateTime> RecordedDate { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalWorkTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalRestTime { get;  }
    public ReactivePropertySlim<DateTime?> StartedOn { get; }
    public ReactivePropertySlim<DateTime?> FinishedOn { get; }

    public ReadOnlyReactiveCollection<DurationEditCommandDto> RestTimeDurations { get; }

    public HistoryItemViewModel(IDialogHelper dialogHelper, HistoryItemModel model) : base(dialogHelper)
    {
        Model = model;

        RecordedDate = Model.RecordedDate.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalWorkTime = Model.TotalWorkTime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalRestTime = Model.TotalRestTime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        StartedOn = Model.Duration.ToReactivePropertySlimAsSynchronized(x => x.Value.StartedOn).AddTo(Disposable);
        FinishedOn = Model.Duration.ToReactivePropertySlimAsSynchronized(x => x.Value.FinishedOn).AddTo(Disposable);
        RestTimeDurations = Model.RestTimeDurations.ToReadOnlyReactiveCollection().AddTo(Disposable);
    }
}
