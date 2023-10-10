using System.Reactive.Linq;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Presentation.Helpers;

namespace Presentation.Models;

public class AlarmModel : BindableBase
{
    private readonly WorkTimeModel _workTimeModel;
    private readonly IDialogHelper _dialogHelper;
    private readonly SettingsModel _settingsModel;

    public ReactiveTimer WorkAlarmTimer { get; }
    public ReactiveTimer WorkSnoozeTimer { get; }
    public ReadOnlyReactivePropertySlim<string?> FormattedWorkOverTime { get; }

    public AlarmModel(WorkTimeModel workTimeModel, SettingsModel settingsModel, IDialogHelper dialogHelper)
    {
        _workTimeModel = workTimeModel;
        _settingsModel = settingsModel;
        _dialogHelper = dialogHelper;

        // Work time alarm
        WorkAlarmTimer = new ReactiveTimer(TimeSpan.FromSeconds(1)).AddTo(Disposable);
        WorkAlarmTimer
            .ObserveOnUIDispatcher()
            .Where(_ =>
                _settingsModel.WorkAlarmConfig.Value?.IsEnabled == true &&
                _workTimeModel.IsOngoing.Value &&
                _workTimeModel.TotalWorkTime.Value >= _settingsModel.WorkTimeLimit.Value
            )
            .Subscribe(async _ => await InvokeWorkTimeAlarm())
            .AddTo(Disposable);
        _workTimeModel.IsOngoing.Where(v => v).Subscribe(_ => WorkAlarmTimer.Start()).AddTo(Disposable);

        WorkSnoozeTimer = new ReactiveTimer(TimeSpan.FromSeconds(1)).AddTo(Disposable);
        WorkSnoozeTimer
            .ObserveOnUIDispatcher()
            .Where(_ =>
                _settingsModel.WorkAlarmConfig.Value?.IsEnabled == true &&
                _settingsModel.WorkAlarmConfig.Value.IsSnoozeEnabled &&
                _workTimeModel.IsOngoing.Value &&
                _workTimeModel.TotalWorkTime.Value >= _settingsModel.WorkTimeLimit.Value
            )
            .Subscribe(async _ => await InvokeWorkTimeSnooze())
            .AddTo(Disposable);
        _settingsModel.WorkAlarmConfig
            .ObserveProperty(v => v.Value.SnoozeMinutes)
            .Subscribe(v => WorkSnoozeTimer.Interval = TimeSpan.FromMinutes(v))
            .AddTo(Disposable);

        FormattedWorkOverTime = _workTimeModel.Overtime
            .Select(v => $"{v.Minutes}{(v <= TimeSpan.Zero ? "分前です。" : "分後")}")
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
    }

    private async Task InvokeWorkTimeAlarm()
    {
        WorkAlarmTimer.Stop();

        // 最初の通知
        if (_settingsModel.Config.Value.WorkTimeAlarm.IsSnoozeEnabled)
        {
            WorkSnoozeTimer.Start();
        }
        else
        {
            await _dialogHelper.ShowDialogAsync(
                $"退勤予定時刻の{FormattedWorkOverTime.Value}分前です。",
                "退勤予定時刻の通知"
            );
        }
    }

    private async Task InvokeWorkTimeSnooze()
    {
        WorkSnoozeTimer.Stop();

        var result = await _dialogHelper.ShowDialogAsync(
            $"退勤予定時刻の{FormattedWorkOverTime.Value}です。\n" +
                $"{_settingsModel.WorkAlarmConfig.Value.SnoozeMinutes}分後に再度アラームを鳴らしますか？",
            "退勤予定時刻の通知",
            button: DialogButton.YesNo
        );

        if (result == Helpers.DialogResult.Yes)
        {
            TimeSpan delayTs = TimeSpan.FromMinutes(_settingsModel.WorkAlarmConfig.Value.SnoozeMinutes);
            WorkSnoozeTimer.Interval = delayTs;
            WorkSnoozeTimer.Start(delayTs);
        }
    }
}
