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

    public ReactivePropertySlim<double> StandardWorkHours { get; }

    public ReactivePropertySlim<bool> IsWorkAlarmEnabled { get; }
    public ReactivePropertySlim<int> WorkRemainingMinutes { get; }
    public ReactivePropertySlim<int> WorkSnoozeMinutes { get; }

    public ReactivePropertySlim<bool> IsRestAlarmEnabled { get; }
    public ReactivePropertySlim<int> RestElapsedHours { get; }
    public ReactivePropertySlim<int> RestElapsedMinutes { get; }
    public ReactivePropertySlim<int> RestSnoozeMinutes { get; }

    public AsyncReactiveCommand<object?> SaveCommand { get; }

    public SettingsPageViewModel(IDialogHelper dialogHelper, SettingsModel model) : base(dialogHelper)
    {
        _model = model;

        StandardWorkHours = _model.StandardWorkMinutes
            .ToReactivePropertySlimAsSynchronized(
                x => x.Value,
                convert: v => (double)v / 60,
                convertBack: v => (int)v * 60
            )
            .AddTo(Disposable);

        IsWorkAlarmEnabled = _model.WorkAlarmConfig
            .ToReactivePropertySlimAsSynchronized(x => x.Value.IsEnabled)
            .AddTo(Disposable);
        WorkRemainingMinutes = _model.WorkAlarmConfig
            .ToReactivePropertySlimAsSynchronized(x => x.Value.RemainingMinutes)
            .AddTo(Disposable);
        WorkSnoozeMinutes = _model.WorkAlarmConfig
            .ToReactivePropertySlimAsSynchronized(x => x.Value.SnoozeMinutes)
            .AddTo(Disposable);

        IsRestAlarmEnabled = _model.RestAlarmConfig
            .ToReactivePropertySlimAsSynchronized(x => x.Value.IsEnabled)
            .AddTo(Disposable);
        RestElapsedHours = new ReactivePropertySlim<int>().AddTo(Disposable);
        RestElapsedMinutes = new ReactivePropertySlim<int>().AddTo(Disposable);
        _model.RestAlarmConfig
            .ObserveProperty(x => x.Value.ElapsedMinutes)
            .Subscribe(x =>
            {
                RestElapsedHours.Value = x / 60;
                RestElapsedMinutes.Value = x % 60;
            })
            .AddTo(Disposable);
        RestElapsedHours
            .CombineLatest(RestElapsedMinutes, (h, m) => (h, m))
            .Skip(1)
            .Subscribe(x => _model.RestAlarmConfig.Value.ElapsedMinutes = x.h * 60 + x.m);
        RestSnoozeMinutes = _model.RestAlarmConfig
            .ToReactivePropertySlimAsSynchronized(x => x.Value.SnoozeMinutes)
            .AddTo(Disposable);

        SaveCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ => await _model.SaveAsync())
            .AddTo(Disposable);
    }
}
