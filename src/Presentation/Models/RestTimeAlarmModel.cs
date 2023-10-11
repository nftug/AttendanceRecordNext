using System.Reactive.Linq;
using Presentation.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.Models;

public class RestTimeAlarmModel : AlarmModelBase<RestTimeAlarmModel>
{
    public RestTimeAlarmModel(
        WorkTimeModel workTimeModel,
        SettingsModel settingsModel,
        IDialogHelper dialogHelper,
        IToastHelper toastHelper
    )
        : base(workTimeModel, settingsModel, dialogHelper, toastHelper)
    {
        IsAlarmEnabled = Observable
            .CombineLatest(
                _settingsModel.RestAlarmConfig.ObserveProperty(v => v.Value.IsEnabled),
                _workTimeModel.IsOngoing,
                _workTimeModel.TotalWorkTime,
                _settingsModel.RestAlarmConfig.ObserveProperty(v => v.Value.ElapsedMinutes),
                _workTimeModel.TotalRestTime,
                (restEnabled, ongoing, total, limitMin, totalRest)
                    => restEnabled && ongoing &&
                        total.TotalMinutes >= limitMin && totalRest == default
            )
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);

        IsSnoozeEnabled = _settingsModel.RestAlarmConfig
            .ObserveProperty(v => v.Value.SnoozeMinutes)
            .Select(x => x > 0)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);

        OvertimeMessage = _workTimeModel.TotalWorkTime
            .CombineLatest(
                _settingsModel.RestAlarmConfig.ObserveProperty(v => v.Value.ElapsedMinutes),
                (total, limitMin) => total.TotalMinutes - limitMin
            )
            .Select(v => v == 0
                ? $"休憩予定時刻になりました。"
                : $"休憩予定時刻から{v:0}分が経過しました。"
            )
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
    }

    protected override string MessageTitle => "休憩のアラーム";

    protected override string ActionName => "休憩";

    public override async Task DoActionAsync() => await _workTimeModel.ToggleRestAsync();
}
