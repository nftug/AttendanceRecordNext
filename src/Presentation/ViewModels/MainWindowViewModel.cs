using System.Reactive.Linq;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Presentation.Views;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using DialogResult = Presentation.Helpers.DialogResult;

namespace Presentation.ViewModels;

public class MainWindowViewModel : MainWindowViewModelBase
{
    private readonly WorkTimeModel _model;
    private readonly IDialogService _dialogService;

    public ReadOnlyReactivePropertySlim<TimeSpan> TotalWorkTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalRestTime { get; }
    public ReadOnlyReactivePropertySlim<bool> IsResting { get; }
    public ReadOnlyReactivePropertySlim<bool> IsOngoing { get; }
    public ReactivePropertySlim<DateTime> NowDateTime { get; }

    public ReactiveCommandSlim<object?> LoadedCommand { get; }
    public AsyncReactiveCommand<object?> ToggleWork { get; }
    public AsyncReactiveCommand<object?> ToggleRest { get; }
    public ReactiveCommandSlim<object?> HistoryDialogCommand { get; }

    public MainWindowViewModel(WorkTimeModel model, IDialogHelper dialogHelper, IDialogService dialogService)
        : base(dialogHelper)
    {
        _model = model;
        _dialogService = dialogService;

        TotalWorkTime = _model.TotalWorkTime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalRestTime = _model.TotalRestTime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        IsResting = _model.IsResting.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        IsOngoing = _model.IsOngoing.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        NowDateTime = new ReactivePropertySlim<DateTime>(DateTime.Now);

        LoadedCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(async _ => await CatchErrorAsync(_model.LoadDataAsync))
            .AddTo(Disposable);

        ToggleWork = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ =>
            {
                if (_model.IsOngoing.Value)
                {
                    var ans = _dialogHelper.ShowDialog(
                        "本日の勤務を終了しますか？",
                        "確認",
                        DialogButton.YesNo, DialogImage.Question
                    );
                    if (ans != DialogResult.Yes) return;
                }

                await CatchErrorAsync(_model.ToggleWorkAsync);
            })
            .AddTo(Disposable);

        ToggleRest = _model.IsOngoing
            .ToAsyncReactiveCommand()
            .WithSubscribe(async _ => await CatchErrorAsync(_model.ToggleRestAsync))
            .AddTo(Disposable);

        HistoryDialogCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(_ => _dialogService.Show(nameof(HistoryDialog)))
            .AddTo(Disposable);

        _model.Timer
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(_ => NowDateTime.Value = DateTime.Now)
            .AddTo(Disposable);
    }
}
