using System.Reactive.Linq;
using Domain.Config;
using Domain.Interfaces;
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

    private ReadOnlyReactivePropertySlim<AppConfig.WorkTimeAlarmConfig> WorkAlarmConfig { get; }

    public ReactiveTimer WorkAlarmTimer { get; }
    public ReactiveTimer WorkSnoozeTimer { get; }

    public AlarmModel(WorkTimeModel workTimeModel, SettingsModel settingsModel, IDialogHelper dialogHelper)
    {
        _workTimeModel = workTimeModel;
        _settingsModel = settingsModel;
        _dialogHelper = dialogHelper;

        WorkAlarmConfig = _settingsModel.Config.ObserveProperty(x => x.Value.WorkTimeAlarm)
            .ToReadOnlyReactivePropertySlim(new()).AddTo(Disposable);

        WorkAlarmTimer = new ReactiveTimer(TimeSpan.FromMinutes(1)).AddTo(Disposable);
        WorkAlarmTimer
            .ObserveOnUIDispatcher()
            .Where(_ => WorkAlarmConfig.Value.IsEnabled == true)
            .Subscribe(async _ => await InvokeWorkTimeAlarm());
        WorkAlarmTimer.Start();

        // NOTE: スヌーズ時間の変更時はWorkAlarmTimer.Intervalを変更する
        // TODO: Configの変更を受信する設計にする。WorkTimeModelも同様。
        WorkSnoozeTimer = new ReactiveTimer(TimeSpan.FromMinutes(WorkAlarmConfig.Value.SnoozeMinutes)).AddTo(Disposable);

    }

    private async Task InvokeWorkTimeAlarm()
    {
        TimeSpan totalWorkTime = _workTimeModel.TotalWorkTime.Value;
        TimeSpan limitWorkTime =
            TimeSpan.FromMinutes(_config.StandardWorkMinutes - _config.WorkTimeAlarm.BeforeMinutes);

        if (totalWorkTime.Ticks < limitWorkTime.Ticks) return;

        WorkAlarmTimer.Stop();

        // 最初の通知
        if (_config.WorkTimeAlarm.IsSnoozeEnabled)
        {
            WorkSnoozeTimer.Start();
        }
        else
        {
            await _dialogHelper.ShowDialogAsync(
                $"退勤予定の{_config.WorkTimeAlarm.BeforeMinutes}分前です。",
                "退勤予定時刻の通知"
            );
        }
    }
}
