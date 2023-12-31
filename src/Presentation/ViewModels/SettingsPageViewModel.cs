using System.Reactive.Linq;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class SettingsPageViewModel : ViewModelBase
{
    private readonly SettingsModel _model;
    private readonly StatusFormatModel _statusFormatModel;

    public ReactivePropertySlim<double> StandardWorkHours { get; }

    public ReactivePropertySlim<bool> IsWorkAlarmEnabled { get; }
    public ReactivePropertySlim<int> WorkRemainingMinutes { get; }
    public ReactivePropertySlim<int> WorkSnoozeMinutes { get; }

    public ReactivePropertySlim<bool> IsRestAlarmEnabled { get; }
    public ReactivePropertySlim<int> RestElapsedHours { get; }
    public ReactivePropertySlim<int> RestElapsedMinutes { get; }
    public ReactivePropertySlim<int> RestSnoozeMinutes { get; }

    public ReactivePropertySlim<bool> ResidentNotificationEnabled { get; }
    public ReactiveProperty<string> StatusFormat { get; }
    public ReactiveProperty<string> TimeSpanFormat { get; }
    public ReactivePropertySlim<uint> StatusFormatTextBoxCaret { get; }

    public ReadOnlyReactivePropertySlim<string> AppDataPath { get; }

    public AsyncReactiveCommand<object?> SaveCommand { get; }
    public ReactiveCommandSlim<object?> ResetFormCommand { get; }
    public AsyncReactiveCommand<object?> UnloadedCommand { get; }
    public ReactiveCommandSlim<object?> OpenAppDataDirectoryCommand { get; }

    public SettingsPageViewModel(IDialogHelper dialogHelper, SettingsModel model, StatusFormatModel statusFormatModel)
        : base(dialogHelper)
    {
        _model = model;
        _statusFormatModel = statusFormatModel;

        StandardWorkHours = _model.ConfigForm
            .ToReactivePropertySlimAsSynchronized(
                x => x.Value.StandardWorkMinutes,
                convert: v => (double)v / 60,
                convertBack: v => (int)v * 60
            )
            .AddTo(Disposable);

        IsWorkAlarmEnabled = _model.ConfigForm
            .ToReactivePropertySlimAsSynchronized(x => x.Value.WorkTimeAlarm.IsEnabled)
            .AddTo(Disposable);
        WorkRemainingMinutes = _model.ConfigForm
            .ToReactivePropertySlimAsSynchronized(x => x.Value.WorkTimeAlarm.RemainingMinutes)
            .AddTo(Disposable);
        WorkSnoozeMinutes = _model.ConfigForm
            .ToReactivePropertySlimAsSynchronized(x => x.Value.WorkTimeAlarm.SnoozeMinutes)
            .AddTo(Disposable);

        IsRestAlarmEnabled = _model.ConfigForm
            .ToReactivePropertySlimAsSynchronized(x => x.Value.RestTimeAlarm.IsEnabled)
            .AddTo(Disposable);
        RestElapsedHours = new ReactivePropertySlim<int>().AddTo(Disposable);
        RestElapsedMinutes = new ReactivePropertySlim<int>().AddTo(Disposable);
        _model.ConfigForm
            .ObserveProperty(x => x.Value.RestTimeAlarm.ElapsedMinutes)
            .Subscribe(x =>
            {
                RestElapsedHours.Value = x / 60;
                RestElapsedMinutes.Value = x % 60;
            })
            .AddTo(Disposable);
        RestElapsedHours
            .CombineLatest(RestElapsedMinutes, (h, m) => (h, m))
            .Skip(1)
            .Subscribe(x => _model.ConfigForm.Value.RestTimeAlarm.ElapsedMinutes = x.h * 60 + x.m);
        RestSnoozeMinutes = _model.ConfigForm
            .ToReactivePropertySlimAsSynchronized(x => x.Value.RestTimeAlarm.SnoozeMinutes)
            .AddTo(Disposable);

        TimeSpanFormat = _model.ConfigForm
            .ToReactivePropertyAsSynchronized(x => x.Value.StatusFormat.TimeSpanFormat, ignoreValidationErrorValue: true)
            .SetValidateNotifyError(v =>
                StatusFormatModel.GetFormattedTimeSpan(new(), v) is not { Length: > 0 } ? "不正なフォーマットです" : null
            )
            .AddTo(Disposable);
        StatusFormat = _model.ConfigForm
            .ToReactivePropertyAsSynchronized(x => x.Value.StatusFormat.StatusFormat, ignoreValidationErrorValue: true)
            .SetValidateNotifyError(v =>
                _statusFormatModel.GetFormattedText(v, TimeSpanFormat.Value) is not { Length: > 0 } ? "不正なフォーマットです" : null
            )
            .AddTo(Disposable);
        StatusFormatTextBoxCaret = new ReactivePropertySlim<uint>().AddTo(Disposable);

        ResidentNotificationEnabled = _model.ConfigForm
            .ToReactivePropertySlimAsSynchronized(x => x.Value.ResidentNotificationEnabled)
            .AddTo(Disposable);

        AppDataPath = _model.AppDataPath.ToReadOnlyReactivePropertySlim(string.Empty).AddTo(Disposable);

        SaveCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ => await _model.SaveAsync())
            .AddTo(Disposable);
        ResetFormCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(_ => _model.ResetForm())
            .AddTo(Disposable);
        UnloadedCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ => await _model.LoadAsync())
            .AddTo(Disposable);
        OpenAppDataDirectoryCommand = new ReactiveCommandSlim<object?>()
            .WithSubscribe(_ => _model.OpenAppDataDirectory())
            .AddTo(Disposable);
    }
}
