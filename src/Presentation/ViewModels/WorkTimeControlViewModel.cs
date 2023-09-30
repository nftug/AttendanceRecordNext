using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Presentation.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class WorkTimeControlViewModel : ViewModelBase
{
    private readonly WorkTimeModel _model;

    public ReadOnlyReactivePropertySlim<TimeSpan> TotalWorkTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalRestTime { get; }
    public ReadOnlyReactivePropertySlim<bool> IsResting { get; }
    public ReadOnlyReactivePropertySlim<bool> IsOngoing { get; }
    public ReactivePropertySlim<DateTime> NowDateTime { get; }

    public AsyncReactiveCommand<object?> ToggleWork { get; }
    public AsyncReactiveCommand<object?> ToggleRest { get; }

    public WorkTimeControlViewModel(WorkTimeModel model, IDialogHelper dialogHelper)
        : base(dialogHelper)
    {
        _model = model;

        TotalWorkTime = _model.TotalWorkTime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalRestTime = _model.TotalRestTime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        IsResting = _model.IsResting.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        IsOngoing = _model.IsOngoing.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        NowDateTime = new ReactivePropertySlim<DateTime>(DateTime.Now);

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
                    if (ans != Helpers.DialogResult.Yes) return;
                }

                await CatchErrorAsync(_model.ToggleWorkAsync);
            })
            .AddTo(Disposable);

        ToggleRest = _model.IsOngoing
            .ToAsyncReactiveCommand()
            .WithSubscribe(_ => CatchErrorAsync(_model.ToggleRestAsync))
            .AddTo(Disposable);

        _model.Timer
            .ObserveOnUIDispatcher()
            .Subscribe(_ => NowDateTime.Value = DateTime.Now)
            .AddTo(Disposable);
    }
}
