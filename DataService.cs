using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace YATODOL;

public static class DataService
{
    private static readonly string DefaultDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YATODOL");

    public static readonly string SettingsPath = Path.Combine(DefaultDataDir, "settings.json");

    private static string _dataDir = DefaultDataDir;

    public static string SavePath => Path.Combine(_dataDir, "todos.json");

    private static readonly JsonSerializerOptions IndentedOptions = new() { WriteIndented = true };

    public static void ApplyCustomPath(AppSettings settings)
    {
        _dataDir = settings.UseCustomDataPath && !string.IsNullOrWhiteSpace(settings.CustomDataPath)
            ? settings.CustomDataPath
            : DefaultDataDir;
    }

    public static void MoveDataFile(string oldPath, AppSettings newSettings)
    {
        var newDir = newSettings.UseCustomDataPath && !string.IsNullOrWhiteSpace(newSettings.CustomDataPath)
            ? newSettings.CustomDataPath
            : DefaultDataDir;
        var newPath = Path.Combine(newDir, "todos.json");

        if (string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
            return;

        Directory.CreateDirectory(newDir);

        if (File.Exists(oldPath) && !File.Exists(newPath))
        {
            File.Copy(oldPath, newPath);
            File.Delete(oldPath);
        }
        else if (File.Exists(oldPath) && File.Exists(newPath))
        {
            // Target already has a file — keep the source as backup
            var backupPath = oldPath + ".bak";
            File.Move(oldPath, backupPath, overwrite: true);
        }

        ApplyCustomPath(newSettings);
    }

    public static List<TodoItem> LoadTodos()
    {
        if (!File.Exists(SavePath)) return [];
        try
        {
            var json = File.ReadAllText(SavePath);
            return JsonSerializer.Deserialize<List<TodoItem>>(json) ?? [];
        }
        catch { return []; }
    }

    public static void SaveTodos(IEnumerable<TodoItem> items)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
        var json = JsonSerializer.Serialize(new List<TodoItem>(items));
        File.WriteAllText(SavePath, json);
    }

    public static AppSettings LoadSettings()
    {
        if (!File.Exists(SettingsPath)) return new AppSettings();
        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch { return new AppSettings(); }
    }

    public static void SaveSettings(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        var json = JsonSerializer.Serialize(settings);
        File.WriteAllText(SettingsPath, json);
    }

    public static string SerializeExport(ExportData data) =>
        JsonSerializer.Serialize(data, IndentedOptions);

    public static ExportData? DeserializeImport(string json) =>
        JsonSerializer.Deserialize<ExportData>(json);
}
