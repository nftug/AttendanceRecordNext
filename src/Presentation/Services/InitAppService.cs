using Presentation.Models;

namespace Presentation.Services;

public class InitAppService
{
    private readonly SettingsModel _settings;
    private readonly WorkTimeModel _workTimeModel;
    private readonly AlarmToastInitializer _toastInitializer;

    public InitAppService(SettingsModel settings, WorkTimeModel workTimeModel, AlarmToastInitializer toastInitializer)
    {
        _settings = settings;
        _workTimeModel = workTimeModel;
        _toastInitializer = toastInitializer;
    }

    public async Task InitAppAsync()
    {
        _toastInitializer.Init();
        await _settings.LoadAsync();
        await _workTimeModel.LoadDataAsync();
    }
}
