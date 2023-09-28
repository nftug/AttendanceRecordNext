using System.Reflection;

namespace Infrastructure.Shared;

public interface IAppConfig
{
    string AppDataPath { get; }
    string AppName { get; }
    string AppVersion { get; }
}

public class AppConfig : IAppConfig
{
    public string AppDataPath { get; }
    public string AppName { get; }
    public string AppVersion { get; }

    public AppConfig()
    {
        var assembly = Assembly.GetEntryAssembly()!;
        AppName = assembly.GetCustomAttribute<AssemblyTitleAttribute>()!.Title;
        AppVersion = assembly.GetName().Version!.ToString();

        AppDataPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "data");
        /*
        AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            assembly.GetName().Name!
        );
        */

        // データ保存用のパスが存在しないとサービス注入時に例外が発生するので、事前に作っておく
        if (!Directory.Exists(AppDataPath))
            Directory.CreateDirectory(AppDataPath);
    }
}
