using System.Diagnostics;
using System.Reactive.Linq;
using Domain.Config;
using Domain.Interfaces;
using Infrastructure.Shared;
using Presentation.Services;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.Models;

public class SettingsModel : BindableBase, IToastMessageSubscriber
{
    private readonly IAppConfigRepository _configRepository;
    private readonly IAppInfo _appInfo;
    private readonly WorkTimeModel _workTimeModel;
    private readonly ToastMessagePublisher _toastMessagePublisher;

    public ReactivePropertySlim<AppConfig> ConfigForm { get; }
    public ReactivePropertySlim<AppConfig> Config { get; }

    public ReactivePropertySlim<AppConfig.WorkTimeAlarmConfig> WorkAlarmConfig { get; }
    public ReactivePropertySlim<AppConfig.RestTimeAlarmConfig> RestAlarmConfig { get; }
    public ReactivePropertySlim<bool> ResidentNotificationEnabled { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> WorkTimeLimit { get; }
    public ReactivePropertySlim<string> AppDataPath { get; }

    public static readonly string DisableResidentNotificationMessage = "DisableResidentNotification";

    public SettingsModel(
        IAppConfigRepository configRepository,
        IAppInfo appInfo,
        WorkTimeModel workTimeModel,
        ToastMessagePublisher toastMessagePublisher
    )
    {
        _configRepository = configRepository;
        _appInfo = appInfo;
        _workTimeModel = workTimeModel;
        _toastMessagePublisher = toastMessagePublisher;

        ConfigForm = new ReactivePropertySlim<AppConfig>().AddTo(Disposable);
        Config = new ReactivePropertySlim<AppConfig>().AddTo(Disposable);

        WorkAlarmConfig = Config
            .ToReactivePropertySlimAsSynchronized(x => x.Value.WorkTimeAlarm)
            .AddTo(Disposable);
        RestAlarmConfig = Config
            .ToReactivePropertySlimAsSynchronized(x => x.Value.RestTimeAlarm)
            .AddTo(Disposable);
        ResidentNotificationEnabled = Config
            .ToReactivePropertySlimAsSynchronized(x => x.Value.ResidentNotificationEnabled)
            .AddTo(Disposable);

        WorkTimeLimit = Observable
            .CombineLatest(
                Config.ObserveProperty(x => x.Value.StandardWorkMinutes),
                WorkAlarmConfig.ObserveProperty(x => x.Value.RemainingMinutes),
                (standard, remain) => TimeSpan.FromMinutes(standard - remain))
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposable);

        AppDataPath = new ReactivePropertySlim<string>(_appInfo.AppDataPath).AddTo(Disposable);

        _toastMessagePublisher.Subscribe(this);
    }

    // NOTE: 設定ページのUnload時にも読み込む
    public async Task LoadAsync()
    {
        await _configRepository.LoadAsync();

        Config.Value = new();
        Config.Value = _configRepository.Config;

        ConfigForm.Value = new();
        ConfigForm.Value = _configRepository.Config;
    }

    public async Task SaveAsync()
    {
        await _configRepository.SaveAsync(ConfigForm.Value);

        // 設定を適用する
        await LoadAsync();
        await _workTimeModel.LoadDataAsync();
    }

    public void OpenAppDataDirectory() => Process.Start("explorer.exe", AppDataPath.Value);

    public async void HandleToastMessage(string message)
    {
        if (message == DisableResidentNotificationMessage)
        {
            ResidentNotificationEnabled.Value = false;
            await SaveAsync();
        }
    }
}
