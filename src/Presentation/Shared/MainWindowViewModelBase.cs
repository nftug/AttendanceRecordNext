using System.ComponentModel;
using System.Windows;
using Presentation.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Application = System.Windows.Application;

namespace Presentation.Shared;

// Reference: https://github.com/fujieda/SystemTrayApp.WPF
public abstract class MainWindowViewModelBase : ViewModelBase
{
    public MainWindowViewModelBase(IDialogHelper dialogHelper) : base(dialogHelper)
    {
        NotifyRequest = new ReactivePropertySlim<NotifyIconWrapper.NotifyRequestRecord?>().AddTo(Disposable);
        WindowState = new ReactivePropertySlim<WindowState>().AddTo(Disposable);
        Visibility = new ReactivePropertySlim<Visibility>(System.Windows.Visibility.Visible).AddTo(Disposable);

        LoadedCommand = new ReactiveCommandSlim<object?>().AddTo(Disposable);
        ClosingCommand = new ReactiveCommandSlim<CancelEventArgs?>()
            .WithSubscribe(Closing)
            .AddTo(Disposable);
        NotifyIconOpenCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(_ =>
            {
                Visibility.Value = System.Windows.Visibility.Visible;
                WindowState.Value = System.Windows.WindowState.Normal;
            })
            .AddTo(Disposable);
        NotifyIconExitCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(_ => Application.Current.Shutdown())
            .AddTo(Disposable);
    }

    public ReactivePropertySlim<NotifyIconWrapper.NotifyRequestRecord?> NotifyRequest { get; }
    public ReactivePropertySlim<WindowState> WindowState { get; }
    public ReactivePropertySlim<Visibility> Visibility { get; }

    public ReactiveCommandSlim<CancelEventArgs?> ClosingCommand { get; }
    public ReactiveCommandSlim<object?> LoadedCommand { get; }
    public ReactiveCommandSlim<object?> NotifyIconOpenCommand { get; }
    public ReactiveCommandSlim<object?> NotifyIconExitCommand { get; }

    protected void Closing(CancelEventArgs? e)
    {
        if (e is null)
            return;
        e.Cancel = true;

        Visibility.Value = System.Windows.Visibility.Hidden;
    }
}
