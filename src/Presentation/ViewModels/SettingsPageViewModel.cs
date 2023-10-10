using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class SettingsPageViewModel : ViewModelBase
{
    private readonly SettingsModel _model;
    public ReactivePropertySlim<double> StandardWorkHours { get; }
    public ReactivePropertySlim<bool> IsWorkAlarmEnabled { get; }
    public ReactivePropertySlim<int> LimitMinutes { get; }
    public ReactivePropertySlim<int> SnoozeMinutes { get; }
    public AsyncReactiveCommand<object?> SaveCommand { get; }

    public SettingsPageViewModel(IDialogHelper dialogHelper, SettingsModel model) : base(dialogHelper)
    {
        _model = model;

        StandardWorkHours = _model.StandardWorkMinutes
            .ToReactivePropertySlimAsSynchronized(
                x => x.Value,
                convert: x => (double)x / 60,
                convertBack: x => (int)x * 60
            )
            .AddTo(Disposable);

        IsWorkAlarmEnabled = _model.WorkAlarmConfig
            .ToReactivePropertySlimAsSynchronized(x => x.Value.IsEnabled)
            .AddTo(Disposable);
        LimitMinutes = _model.WorkAlarmConfig
            .ToReactivePropertySlimAsSynchronized(x => x.Value.BeforeMinutes)
            .AddTo(Disposable);
        SnoozeMinutes = _model.WorkAlarmConfig
            .ToReactivePropertySlimAsSynchronized(x => x.Value.SnoozeMinutes)
            .AddTo(Disposable);

        SaveCommand = new AsyncReactiveCommand()
            .WithSubscribe(async _ => await _model.SaveAsync())
            .AddTo(Disposable);
    }
}
