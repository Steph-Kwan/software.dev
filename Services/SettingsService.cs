using System.IO;
using System.Text.Json;

namespace TrackMaintenance.Services;

public class AppSettings
{
    public string BackgroundColor { get; set; } = "#EEF2F7";
    public bool   IsChineseLanguage { get; set; } = true;
}

public static class SettingsService
{
    private static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TrackMaintenancePro");

    private static readonly string File = Path.Combine(Folder, "settings.json");

    public static AppSettings Current { get; private set; } = new();

    public static void Load()
    {
        try
        {
            if (!System.IO.File.Exists(File)) return;
            var json = System.IO.File.ReadAllText(File);
            Current = JsonSerializer.Deserialize<AppSettings>(json) ?? new();
        }
        catch { Current = new(); }

        L.IsChinese = Current.IsChineseLanguage;
    }

    public static void Save()
    {
        Directory.CreateDirectory(Folder);
        System.IO.File.WriteAllText(File, JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true }));
    }
}
