using System.Reactive.Linq;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Presentation.Helpers;
using Presentation.Services;

namespace Presentation.Models;

public abstract class AlarmModelBase<TSelf> : BindableBase, IToastMessageSubscriber
    where TSelf : AlarmModelBase<TSelf>
{
    protected readonly WorkTimeModel _workTimeModel;
    protected readonly IDialogHelper _dialogHelper;
    protected readonly SettingsModel _settingsModel;
    protected readonly IToastHelper _toastHelper;
    protected readonly ToastMessagePublisher _toastMessagePublisher;
    protected readonly MainWindowModel _mainWindowModel;

    protected ReactiveTimer AlarmTimer { get; }
    protected ReactiveTimer SnoozeTimer { get; }
    public ReadOnlyReactivePropertySlim<bool> IsAlarmEnabled { get; protected set; } = null!;
    public ReadOnlyReactivePropertySlim<bool> IsSnoozeEnabled { get; protected set; } = null!;
    protected ReadOnlyReactivePropertySlim<string?> OvertimeMessage { get; set; } = null!;

    protected abstract string MessageTitle { get; }
    protected abstract string ActionName { get; }

    public static readonly string SnoozeMessage = "Snooze";
    public static readonly string ActMessage = "Act";

    protected AlarmModelBase(
        WorkTimeModel workTimeModel,
        SettingsModel settingsModel,
        IDialogHelper dialogHelper,
        IToastHelper toastHelper,
        ToastMessagePublisher toastMessagePublisher,
        MainWindowModel mainWindowModel
    )
    {
        _workTimeModel = workTimeModel;
        _settingsModel = settingsModel;
        _dialogHelper = dialogHelper;
        _toastHelper = toastHelper;
        _toastMessagePublisher = toastMessagePublisher;
        _mainWindowModel = mainWindowModel;

        AlarmTimer = new ReactiveTimer(TimeSpan.FromSeconds(1)).AddTo(Disposable);
        AlarmTimer
            .ObserveOnUIDispatcher()
            .Where(_ => IsAlarmEnabled!.Value)
            .Subscribe(_ => InvokeWorkTimeAlarm())
            .AddTo(Disposable);
        _workTimeModel.IsOngoing.Where(v => v).Subscribe(_ => AlarmTimer.Start()).AddTo(Disposable);

        SnoozeTimer = new ReactiveTimer(TimeSpan.FromSeconds(1)).AddTo(Disposable);
        SnoozeTimer
            .ObserveOnUIDispatcher()
            .Where(_ => IsAlarmEnabled!.Value && IsSnoozeEnabled.Value)
            .Subscribe(_ => InvokeWorkTimeSnooze())
            .AddTo(Disposable);

        _toastMessagePublisher.Subscribe((TSelf)this);
    }

    protected void InvokeWorkTimeAlarm()
    {
        AlarmTimer.Stop();

        if (IsSnoozeEnabled.Value)
        {
            SnoozeTimer.Start();
        }
        else
        {
            _toastHelper.ShowToast(MessageTitle, OvertimeMessage.Value!, ToastType.Alarm, enableDismiss: true);
        }
    }

    protected void InvokeWorkTimeSnooze()
    {
        SnoozeTimer.Stop();

        var snooze = new ToastAction(ToastMessage.Create(typeof(TSelf), SnoozeMessage), "スヌーズ");
        var action = new ToastAction(ToastMessage.Create(typeof(TSelf), ActMessage), ActionName);

        _toastHelper.ShowToast(
            MessageTitle,
            OvertimeMessage.Value!,
            type: ToastType.Alarm,
            enableDismiss: true,
            snooze,
            action
         );
    }

    public async void HandleToastMessage(string message)
    {
        if (message == SnoozeMessage) Snooze();
        else if (message == ActMessage) await ActAsync();
    }

    protected void Snooze()
    {
        int snoozeMinutes = _settingsModel.WorkAlarmConfig.Value.SnoozeMinutes;
        TimeSpan delayTs = TimeSpan.FromMinutes(snoozeMinutes);
        SnoozeTimer.Start(delayTs);
    }

    protected async Task ActAsync()
    {
        if (_mainWindowModel.Visibility.Value == System.Windows.Visibility.Hidden)
        {
            // NOTE: ウィンドウが最小化されるのを防ぐため、Delayを設ける
            await Task.Delay(1000);
            _mainWindowModel.Reopen();
        }
        await DoActionAsync();
    }

    protected abstract Task DoActionAsync();
}
