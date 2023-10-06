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

    public SettingsModel(IAppConfigRepository configRepository, WorkTimeModel workTimeModel)
    {
        _configRepository = configRepository;
        _workTimeModel = workTimeModel;

        Config = new ReactivePropertySlim<AppConfig>(_configRepository.Config).AddTo(Disposable);
        StandardWorkMinutes = Config
            .ToReactivePropertySlimAsSynchronized(x => x.Value.StandardWorkMinutes)
            .AddTo(Disposable);
    }

    public async Task SaveAsync()
    {
        await _configRepository.SaveAsync(Config.Value);

        // 設定を適用する
        await _workTimeModel.LoadDataAsync();
    }
}
