using System.ComponentModel;
using System.Windows;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Services;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

// Reference: https://github.com/fujieda/SystemTrayApp.WPF
public class MainWindowViewModel : ViewModelBase
{
    private readonly MainWindowModel _model;
    private readonly InitAppService _init;
    private readonly WorkTimeModel _workTimeModel;

    public ReactivePropertySlim<string?> HeaderTitle { get; }
    public ReactivePropertySlim<string?> WindowTitle { get; }

    public ReactivePropertySlim<NotifyIconWrapper.NotifyRequestRecord?> NotifyRequest { get; }
    public ReactivePropertySlim<WindowState> WindowState { get; }
    public ReactivePropertySlim<Visibility> Visibility { get; }

    public ReactiveCommandSlim<CancelEventArgs?> ClosingCommand { get; }
    public ReactiveCommandSlim<object?> LoadedCommand { get; }
    public ReactiveCommandSlim<object?> StateChangedCommand { get; }
    public ReactiveCommandSlim<object?> OpenCommand { get; }
    public ReactiveCommandSlim<object?> ExitCommand { get; }

    public ReactiveCommandSlim<object?> ActivateCommand { get; }

    public MainWindowViewModel(IDialogHelper dialogHelper, MainWindowModel model, InitAppService init, WorkTimeModel workTimeModel)
        : base(dialogHelper)
    {
        _model = model;
        _init = init;
        _workTimeModel = workTimeModel;

        HeaderTitle = _model.HeaderTitle.ToReactivePropertySlimAsSynchronized(x => x.Value).AddTo(Disposable);
        WindowTitle = _model.WindowTitle.ToReactivePropertySlimAsSynchronized(x => x.Value).AddTo(Disposable);

        NotifyRequest = new ReactivePropertySlim<NotifyIconWrapper.NotifyRequestRecord?>().AddTo(Disposable);
        WindowState = _model.WindowState.ToReactivePropertySlimAsSynchronized(x => x.Value).AddTo(Disposable);
        Visibility = _model.Visibility.ToReactivePropertySlimAsSynchronized(x => x.Value).AddTo(Disposable);

        LoadedCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(async _ => await CatchErrorAsync(_init.InitAppAsync))
            .AddTo(Disposable);

        ExitCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(async _ =>
            {
                if (_model.Visibility.Value == System.Windows.Visibility.Hidden)
                    _model.Reopen();

                var ans = await _dialogHelper.ShowCustomDialogAsync(
                    $"アプリを終了しますか？{(_workTimeModel.IsOngoing.Value ? "\n（終了後も時間のカウントは継続されます。）" : "")}",
                    "アプリの終了",
                    primaryButtonText: "終了",
                    secondaryButtonText: "最小化",
                    closeButtonText: "キャンセル"
                );

                if (ans == Helpers.DialogResult.Primary)
                    _model.Shutdown();
                else if (ans == Helpers.DialogResult.Secondary)
                    _model.Closing(null);
                else
                    _model.Shutdown();
            })
            .AddTo(Disposable);

        ClosingCommand = new ReactiveCommandSlim<CancelEventArgs?>()
            .WithSubscribe(e =>
            {
                if (e != null) e.Cancel = true;
                ExitCommand.Execute(null);
            })
            .AddTo(Disposable);

        StateChangedCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(_ => _model.StateChanged())
            .AddTo(Disposable);

        OpenCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(_ => _model.Reopen())
            .AddTo(Disposable);

        ActivateCommand = new ReactiveCommandSlim<object?>().AddTo(Disposable);
        _model.ActivateCommand.Subscribe(_ => ActivateCommand.Execute(null)).AddTo(Disposable);
    }
}
