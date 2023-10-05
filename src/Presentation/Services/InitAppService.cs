using Domain.Interfaces;
using Presentation.Models;

namespace Presentation.Services;

public class InitAppService
{
    private readonly IAppConfigRepository _configRepository;
    private readonly WorkTimeModel _workTimeModel;

    public InitAppService(IAppConfigRepository configRepository, WorkTimeModel workTimeModel)
    {
        _configRepository = configRepository;
        _workTimeModel = workTimeModel;
    }

    public async Task InitAppAsync()
    {
        await _configRepository.LoadAsync();
        await _workTimeModel.LoadDataAsync();
    }
}
