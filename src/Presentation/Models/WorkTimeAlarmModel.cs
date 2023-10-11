using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Presentation.Helpers;

namespace Presentation.Models;

public class WorkTimeAlarmModel : AlarmModelBase<WorkTimeAlarmModel>
{
    public WorkTimeAlarmModel(
        WorkTimeModel workTimeModel,
        SettingsModel settingsModel,
        IDialogHelper dialogHelper,
        IToastHelper toastHelper
    )
        : base(workTimeModel, settingsModel, dialogHelper, toastHelper)
    {
        IsAlarmEnabled = Observable
            .CombineLatest(
                _settingsModel.WorkAlarmConfig.ObserveProperty(v => v.Value.IsEnabled),
                _workTimeModel.IsOngoing,
                _workTimeModel.TotalWorkTime,
                _settingsModel.WorkTimeLimit,
                (enabled, ongoing, total, limit) => enabled && ongoing && total >= limit)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);

        IsSnoozeEnabled = _settingsModel.WorkAlarmConfig
            .ObserveProperty(v => v.Value.SnoozeMinutes)
            .Select(x => x > 0)
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);

        OvertimeMessage = _workTimeModel.Overtime
            .Select(v => v <= TimeSpan.Zero
                ? $"退勤予定時刻の{Math.Abs(v.TotalMinutes):0}分前です。"
                : $"退勤予定時刻から{v.TotalMinutes:0}分が経過しました。"
            )
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
    }

    protected override string MessageTitle => "退勤のアラーム";

    protected override string ActionName => "退勤";

    public override async Task DoActionAsync()
    {
        var ans = await _dialogHelper.ShowDialogAsync(
            "本日の勤務を終了しますか？",
            "退勤",
            DialogButton.YesNo, DialogImage.Question
        );
        if (ans != Helpers.DialogResult.Yes) return;

        await _workTimeModel.ToggleWorkAsync();
    }
}
