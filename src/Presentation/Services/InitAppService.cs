using Presentation.Models;

namespace Presentation.Services;

public class InitAppService
{
    private readonly SettingsModel _settings;
    private readonly WorkTimeModel _workTimeModel;

    public InitAppService(SettingsModel settings, WorkTimeModel workTimeModel)
    {
        _settings = settings;
        _workTimeModel = workTimeModel;
    }

    public async Task InitAppAsync()
    {
        await _settings.LoadAsync();
        await _workTimeModel.LoadDataAsync();
    }
}
