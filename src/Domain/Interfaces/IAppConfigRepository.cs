using Domain.Config;

namespace Domain.Interfaces;

public interface IAppConfigRepository
{
    AppConfig Config { get; }
    ValueTask LoadAsync();
    ValueTask SaveAsync(AppConfig appConfig);
}
