using System.Reactive.Linq;
using Domain.Config;
using Domain.Interfaces;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.Models;

public class SettingsModel : BindableBase
{
    private readonly IAppConfigRepository _configRepository;
    private readonly WorkTimeModel _workTimeModel;

    public ReactivePropertySlim<AppConfig> Config { get; }

    public ReactivePropertySlim<int> StandardWorkMinutes { get; }

    public ReactivePropertySlim<AppConfig.WorkTimeAlarmConfig> WorkAlarmConfig { get; }
    public ReactivePropertySlim<bool> IsWorkAlarmEnabled { get; }
    public ReactivePropertySlim<double> BeforeWorkMinutes { get; }
    public ReactivePropertySlim<bool> IsWorkSnoozeEnabled { get; }
    public ReactivePropertySlim<double> SnoozeWorkMinutes { get; }

    public ReadOnlyReactivePropertySlim<TimeSpan> WorkTimeLimit { get; }

    public SettingsModel(IAppConfigRepository configRepository, WorkTimeModel workTimeModel)
    {
        _configRepository = configRepository;
        _workTimeModel = workTimeModel;

        Config = new ReactivePropertySlim<AppConfig>().AddTo(Disposable);

        StandardWorkMinutes = Config
            .ToReactivePropertySlimAsSynchronized(x => x.Value.StandardWorkMinutes)
            .AddTo(Disposable);

        WorkAlarmConfig = Config
            .ToReactivePropertySlimAsSynchronized(x => x.Value.WorkTimeAlarm)
            .AddTo(Disposable);

        IsWorkAlarmEnabled = WorkAlarmConfig
            .ToReactivePropertySlimAsSynchronized(x => x.Value.IsEnabled)
            .AddTo(Disposable);
        BeforeWorkMinutes = WorkAlarmConfig
            .ToReactivePropertySlimAsSynchronized(x => x.Value.BeforeMinutes)
            .AddTo(Disposable);
        IsWorkSnoozeEnabled = WorkAlarmConfig
            .ToReactivePropertySlimAsSynchronized(x => x.Value.IsSnoozeEnabled)
            .AddTo(Disposable);
        SnoozeWorkMinutes = WorkAlarmConfig
            .ToReactivePropertySlimAsSynchronized(x => x.Value.SnoozeMinutes)
            .AddTo(Disposable);

        WorkTimeLimit = Observable
            .CombineLatest(
                StandardWorkMinutes,
                BeforeWorkMinutes,
                (standard, cfg) => TimeSpan.FromMinutes(standard - cfg))
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);
    }

    public async Task LoadAsync()
    {
        await _configRepository.LoadAsync();
        Config.Value = _configRepository.Config;
    }

    public async Task SaveAsync()
    {
        await _configRepository.SaveAsync(Config.Value);

        // 設定を適用する
        await LoadAsync();
        await _workTimeModel.LoadDataAsync();
    }
}
