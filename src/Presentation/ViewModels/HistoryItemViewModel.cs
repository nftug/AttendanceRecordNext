using Domain.Commands;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;

namespace Presentation.ViewModels;

public class HistoryItemViewModel : ViewModelBase
{
    internal HistoryItemModel Model { get; }

    public ReadOnlyReactivePropertySlim<DateTime> RecordedDate { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalWorkTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalRestTime { get; }
    public ReactivePropertySlim<DateTime> StartedOn { get; }
    public ReactivePropertySlim<DateTime?> FinishedOn { get; }
    public ReadOnlyReactiveCollection<RestTimeEditCommandDto> RestTimes { get; }
    public ReactivePropertySlim<RestTimeEditCommandDto?> SelectedRestItem { get; }

    public AsyncReactiveCommand<object?> SaveItemCommand { get; }
    public AsyncReactiveCommand<object?> DeleteItemCommand { get; }
    public ReactiveCommandSlim<object?> RemoveSelectedRestItemCommand { get; }
    public ReactiveCommandSlim<object?> AddRestItemCommand { get; }

    public HistoryItemViewModel(IDialogHelper dialogHelper, HistoryItemModel model) : base(dialogHelper)
    {
        Model = model;

        RecordedDate = Model.RecordedDate.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalWorkTime = Model.TotalWorkTime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalRestTime = Model.TotalRestTime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        StartedOn = Model.Duration.ToReactivePropertySlimAsSynchronized(x => x.Value.StartedOn).AddTo(Disposable);
        FinishedOn = Model.Duration.ToReactivePropertySlimAsSynchronized(x => x.Value.FinishedOn).AddTo(Disposable);
        RestTimes = Model.RestTimes.ToReadOnlyReactiveCollection().AddTo(Disposable);
        SelectedRestItem = Model.SelectedRestItem.ToReactivePropertySlimAsSynchronized(x => x.Value).AddTo(Disposable);

        SaveItemCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ =>
            {
                var ans = await _dialogHelper.ShowDialogAsync(
                    $"{RecordedDate.Value:yyyy/MM/dd}の記録を保存しますか？",
                    "記録の保存",
                    DialogButton.YesNo, DialogImage.Question
                );
                if (ans != Helpers.DialogResult.Yes) return;

                await CatchErrorAsync(Model.SaveItemAsync);
            })
            .AddTo(Disposable);

        DeleteItemCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ =>
            {
                var ans = await _dialogHelper.ShowDialogAsync(
                    $"{RecordedDate.Value:yyyy/MM/dd}の記録をすべて削除しますか？",
                    "記録の削除",
                    DialogButton.YesNo, DialogImage.Question
                );
                if (ans != Helpers.DialogResult.Yes) return;

                await CatchErrorAsync(Model.DeleteItemAsync);
            })
            .AddTo(Disposable);

        RemoveSelectedRestItemCommand = SelectedRestItem
            .Select(v => v != null)
            .ToReactiveCommandSlim()
            .WithSubscribe(_ => Model.RemoveSelectedRestItem())
            .AddTo(Disposable);

        AddRestItemCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(_ => Model.AddRestItem())
            .AddTo(Disposable);
    }
}
