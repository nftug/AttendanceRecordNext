using System.Text.Json;
using Domain.Config;
using Domain.Interfaces;
using Infrastructure.Shared;

namespace Infrastructure.Repositories;

public class AppConfigRepository : IAppConfigRepository
{
    private readonly IAppInfo _appInfo;
    private string ConfigPath => Path.Combine(_appInfo.AppDataPath, "config.json");

    public AppConfig Config { get; private set; } = null!;

    private readonly static JsonSerializerOptions JsonSerializerOptions =
        new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
            WriteIndented = true
        };

    public AppConfigRepository(IAppInfo appInfo)
    {
        _appInfo = appInfo;
    }

    public async ValueTask LoadAsync()
    {
        if (!Directory.Exists(_appInfo.AppDataPath))
            Directory.CreateDirectory(_appInfo.AppDataPath);

        if (File.Exists(ConfigPath))
        {
            var json = await File.ReadAllTextAsync(ConfigPath);
            Config = JsonSerializer.Deserialize<AppConfig>(json) ?? new();
        }
        else
        {
            await SaveAsync(new());
        }
    }

    public async ValueTask SaveAsync(AppConfig appConfig)
    {
        Config = appConfig;
        var json = JsonSerializer.Serialize(Config, JsonSerializerOptions);
        await File.WriteAllTextAsync(ConfigPath, json);
    }
}
