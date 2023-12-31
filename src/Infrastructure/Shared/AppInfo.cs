using System.Reflection;

namespace Infrastructure.Shared;

public interface IAppInfo
{
    string AppDataPath { get; }
    string AppName { get; }
    string AppVersion { get; }
}

public class AppInfo : IAppInfo
{
    public string AppDataPath { get; }
    public string AppName { get; }
    public string AppVersion { get; }

    public AppInfo()
    {
        var assembly = Assembly.GetEntryAssembly()!;
        AppName = assembly.GetCustomAttribute<AssemblyTitleAttribute>()!.Title;
        AppVersion = assembly.GetName().Version!.ToString();

#if DEBUG
        AppDataPath = Path.GetDirectoryName(Environment.ProcessPath)!;
#else
        AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            assembly.GetName().Name!
        );
#endif

        // データ保存用のパスが存在しないとサービス注入時に例外が発生するので、事前に作っておく
        if (!Directory.Exists(AppDataPath))
            Directory.CreateDirectory(AppDataPath);
    }
}
