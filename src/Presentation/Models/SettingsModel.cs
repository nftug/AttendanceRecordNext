using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using Domain.Config;
using Domain.Interfaces;
using Infrastructure.Shared;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.Models;

public class SettingsModel : BindableBase
{
    private readonly IAppConfigRepository _configRepository;
    private readonly IAppInfo _appInfo;
    private readonly WorkTimeModel _workTimeModel;

    public ReactivePropertySlim<AppConfig> Config { get; }

    public ReactivePropertySlim<int> StandardWorkMinutes { get; }
    public ReactivePropertySlim<AppConfig.WorkTimeAlarmConfig> WorkAlarmConfig { get; }
    public ReactivePropertySlim<AppConfig.RestTimeAlarmConfig> RestAlarmConfig { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> WorkTimeLimit { get; }
    public ReactivePropertySlim<string> AppDataPath { get; }

    public SettingsModel(IAppConfigRepository configRepository, IAppInfo appInfo, WorkTimeModel workTimeModel)
    {
        _configRepository = configRepository;
        _appInfo = appInfo;
        _workTimeModel = workTimeModel;

        Config = new ReactivePropertySlim<AppConfig>().AddTo(Disposable);

        StandardWorkMinutes = Config
            .ToReactivePropertySlimAsSynchronized(x => x.Value.StandardWorkMinutes)
            .AddTo(Disposable);
        WorkAlarmConfig = Config
            .ToReactivePropertySlimAsSynchronized(x => x.Value.WorkTimeAlarm)
            .AddTo(Disposable);
        RestAlarmConfig = Config
            .ToReactivePropertySlimAsSynchronized(x => x.Value.RestTimeAlarm)
            .AddTo(Disposable);

        WorkTimeLimit = Observable
            .CombineLatest(
                StandardWorkMinutes,
                WorkAlarmConfig.ObserveProperty(x => x.Value.RemainingMinutes),
                (standard, remain) => TimeSpan.FromMinutes(standard - remain))
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);

        AppDataPath = new ReactivePropertySlim<string>(_appInfo.AppDataPath).AddTo(Disposable);
    }

    // NOTE: 設定ページのUnload時にも読み込む
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

    public void OpenAppDataDirectory() => Process.Start("explorer.exe", AppDataPath.Value);
}
