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

    public ReadOnlyReactivePropertySlim<string?> HeaderTitle { get; }
    public ReadOnlyReactivePropertySlim<string?> WindowTitle { get; }

    public ReactivePropertySlim<NotifyIconWrapper.NotifyRequestRecord?> NotifyRequest { get; }
    public ReactivePropertySlim<WindowState> WindowState { get; }
    public ReactivePropertySlim<Visibility> Visibility { get; }

    public ReactiveCommandSlim<CancelEventArgs?> ClosingCommand { get; }
    public ReactiveCommandSlim<object?> LoadedCommand { get; }
    public ReactiveCommandSlim<object?> StateChangedCommand { get; }
    public ReactiveCommandSlim<object?> NotifyIconOpenCommand { get; }
    public ReactiveCommandSlim<object?> NotifyIconExitCommand { get; }

    public MainWindowViewModel(IDialogHelper dialogHelper, MainWindowModel model, InitAppService init)
        : base(dialogHelper)
    {
        _model = model;
        _init = init;

        HeaderTitle = _model.HeaderTitle.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        WindowTitle = _model.WindowTitle.ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        NotifyRequest = new ReactivePropertySlim<NotifyIconWrapper.NotifyRequestRecord?>().AddTo(Disposable);
        WindowState = _model.WindowState.ToReactivePropertySlimAsSynchronized(x => x.Value).AddTo(Disposable);
        Visibility = _model.Visibility.ToReactivePropertySlimAsSynchronized(x => x.Value).AddTo(Disposable);

        LoadedCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(async _ => await CatchErrorAsync(_init.InitAppAsync))
            .AddTo(Disposable);
        ClosingCommand = new ReactiveCommandSlim<CancelEventArgs?>()
            .WithSubscribe(e => _model.Closing(e))
            .AddTo(Disposable);
        StateChangedCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(_ => _model.StateChanged())
            .AddTo(Disposable);
        NotifyIconOpenCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(_ => _model.Reopen())
            .AddTo(Disposable);
        NotifyIconExitCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(_ => System.Windows.Application.Current.Shutdown())
            .AddTo(Disposable);
    }
}
