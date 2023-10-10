using System.Reactive.Linq;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Presentation.Helpers;

namespace Presentation.Models;

public interface IAlarmModel
{
    void InvokeSnooze();
}

public abstract class AlarmModelBase<TSelf> : BindableBase, IAlarmModel
    where TSelf : AlarmModelBase<TSelf>
{
    protected readonly WorkTimeModel _workTimeModel;
    protected readonly IDialogHelper _dialogHelper;
    protected readonly SettingsModel _settingsModel;
    protected readonly IToastHelper _toastHelper;

    protected ReactiveTimer AlarmTimer { get; }
    protected ReactiveTimer SnoozeTimer { get; }
    protected ReadOnlyReactivePropertySlim<bool> IsAlarmEnabled { get; set; } = null!;
    protected ReadOnlyReactivePropertySlim<bool> IsSnoozeEnabled { get; set; } = null!;
    protected ReadOnlyReactivePropertySlim<string?> OvertimeMessage { get; set; } = null!;

    protected abstract string MessageTitle { get; }

    protected AlarmModelBase(
        WorkTimeModel workTimeModel,
        SettingsModel settingsModel,
        IDialogHelper dialogHelper,
        IToastHelper toastHelper
    )
    {
        _workTimeModel = workTimeModel;
        _settingsModel = settingsModel;
        _dialogHelper = dialogHelper;
        _toastHelper = toastHelper;

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
            _toastHelper.ShowAlarmToast(MessageTitle, OvertimeMessage.Value!);
        }
    }

    protected void InvokeWorkTimeSnooze()
    {
        SnoozeTimer.Stop();
        _toastHelper.ShowAlarmToastWithSnooze<TSelf>(MessageTitle, OvertimeMessage.Value!);
    }

    public void InvokeSnooze()
    {
        int snoozeMinutes = _settingsModel.WorkAlarmConfig.Value.SnoozeMinutes;
        TimeSpan delayTs = TimeSpan.FromMinutes(snoozeMinutes);
        SnoozeTimer.Start(delayTs);
    }
}