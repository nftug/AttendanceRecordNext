namespace Infrastructure.Shared;

public static class AppConfig
{
    public static readonly string AppDataPath =
        Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "data");

    /*
    public static readonly string AppDataPath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EffectMeasurement"
        );
    */

    public static void InitAppDataPath()
    {
        if (!Directory.Exists(AppDataPath))
            Directory.CreateDirectory(AppDataPath);
    }
}
