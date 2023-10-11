using System.ComponentModel;
using System.Windows;
using Infrastructure.Shared;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.Models;

public class MainWindowModel : BindableBase
{
    private readonly IAppInfo _appInfo;

    public ReactivePropertySlim<string?> HeaderTitle { get; }
    public ReactivePropertySlim<string?> WindowTitle { get; }
    public string AppName { get; }

    public ReactivePropertySlim<WindowState> WindowState { get; }
    private ReactivePropertySlim<WindowState> PreviousWindowState { get; }
    public ReactivePropertySlim<Visibility> Visibility { get; }

    public MainWindowModel(IAppInfo appInfo)
    {
        _appInfo = appInfo;
        AppName = _appInfo.AppName;

        HeaderTitle = new ReactivePropertySlim<string?>().AddTo(Disposable);
        WindowTitle = new ReactivePropertySlim<string?>(_appInfo.AppName).AddTo(Disposable);

        WindowState = new ReactivePropertySlim<WindowState>().AddTo(Disposable);
        PreviousWindowState = new ReactivePropertySlim<WindowState>().AddTo(Disposable);
        Visibility = new ReactivePropertySlim<Visibility>(System.Windows.Visibility.Visible).AddTo(Disposable);
    }

    public void Reopen()
    {
        WindowState.Value = PreviousWindowState.Value;
        Visibility.Value = System.Windows.Visibility.Visible;
    }

    public void Closing(CancelEventArgs? e)
    {
        if (e != null) e.Cancel = true;

        PreviousWindowState.Value =
            WindowState.Value == System.Windows.WindowState.Minimized
            ? System.Windows.WindowState.Normal
            : WindowState.Value;

        Visibility.Value = System.Windows.Visibility.Hidden;
    }

    public void StateChanged()
    {
        if (WindowState.Value != System.Windows.WindowState.Minimized) return;

        WindowState.Value = PreviousWindowState.Value;
        Closing(null);
    }
}
