using System.ComponentModel;
using System.Windows;
using Infrastructure.Shared;
using Presentation.Helpers;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.Models;

public class MainWindowModel : BindableBase
{
    private readonly IAppInfo _appInfo;
    private readonly IToastHelper _toastHelper;
    private readonly SettingsModel _settingsModel;

    public ReactivePropertySlim<string?> HeaderTitle { get; }
    public ReactivePropertySlim<string?> WindowTitle { get; }
    public string AppName { get; }

    public ReactivePropertySlim<WindowState> WindowState { get; }
    private ReactivePropertySlim<WindowState> PreviousWindowState { get; }
    public ReactivePropertySlim<Visibility> Visibility { get; }

    public ReactiveCommandSlim<object?> ActivateCommand { get; }

    public MainWindowModel(IAppInfo appInfo, IToastHelper toastHelper, SettingsModel settingsModel)
    {
        _appInfo = appInfo;
        AppName = _appInfo.AppName;
        _toastHelper = toastHelper;
        _settingsModel = settingsModel;

        HeaderTitle = new ReactivePropertySlim<string?>().AddTo(Disposable);
        WindowTitle = new ReactivePropertySlim<string?>(_appInfo.AppName).AddTo(Disposable);

        WindowState = new ReactivePropertySlim<WindowState>().AddTo(Disposable);
        PreviousWindowState = new ReactivePropertySlim<WindowState>().AddTo(Disposable);
        Visibility = new ReactivePropertySlim<Visibility>(System.Windows.Visibility.Visible).AddTo(Disposable);

        ActivateCommand = new ReactiveCommandSlim<object?>().AddTo(Disposable);

        Task.Run(() => NamedPipeServer.ReceiveMessageAsync(message =>
        {
            if (message?.Text == "Reopen")
                Reopen();
        }));
    }

    public void Reopen()
    {
        WindowState.Value = PreviousWindowState.Value;
        Visibility.Value = System.Windows.Visibility.Visible;
        Activate();
    }

    public void Closing(CancelEventArgs? e)
    {
        if (e != null) e.Cancel = true;

        PreviousWindowState.Value =
            WindowState.Value == System.Windows.WindowState.Minimized
            ? System.Windows.WindowState.Normal
            : WindowState.Value;

        Visibility.Value = System.Windows.Visibility.Hidden;

        if (_settingsModel.ResidentNotificationEnabled.Value)
        {
            var action = new ToastAction(
                ToastMessage.Create(typeof(SettingsModel), SettingsModel.DisableResidentNotificationMessage),
                "今後表示しない"
            );

            _toastHelper.ShowToast(
                "アプリは常駐しています",
                "再表示と終了を行うには、システムトレイのアイコンを右クリックします。\n"
                + "アイコンをダブルクリックすると再表示できます。",
                enableDismiss: true,
                toastActions: action
            );
        }
    }

    public void StateChanged()
    {
        if (WindowState.Value != System.Windows.WindowState.Minimized)
            return;

        WindowState.Value = PreviousWindowState.Value;
        Closing(null);
    }

    public void Activate() => ActivateCommand.Execute(null);

    public void Shutdown() => System.Windows.Application.Current.Shutdown();
}
