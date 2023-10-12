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

    public ReadOnlyReactivePropertySlim<AppConfig.WorkTimeAlarmConfig> WorkAlarmConfig { get; }
    public ReadOnlyReactivePropertySlim<AppConfig.RestTimeAlarmConfig> RestAlarmConfig { get; }
    public ReadOnlyReactivePropertySlim<bool> ResidentNotificationEnabled { get; }
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
            .ObserveProperty(x => x.Value.WorkTimeAlarm)
            .ToReadOnlyReactivePropertySlim(new())
            .AddTo(Disposable);
        RestAlarmConfig = Config
            .ObserveProperty(x => x.Value.RestTimeAlarm)
            .ToReadOnlyReactivePropertySlim(new())
            .AddTo(Disposable);
        ResidentNotificationEnabled = Config
            .ObserveProperty(x => x.Value.ResidentNotificationEnabled)
            .ToReadOnlyReactivePropertySlim()
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

        // NOTE: 設定を正しく反映させるため、二度更新する
        Config.Value = null!;
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

    public void ResetForm() => ConfigForm.Value = new();

    public void OpenAppDataDirectory() => Process.Start("explorer.exe", AppDataPath.Value);

    public async void HandleToastMessage(string message)
    {
        if (message == DisableResidentNotificationMessage)
        {
            ConfigForm.Value.ResidentNotificationEnabled = false;
            await SaveAsync();
        }
    }
}
