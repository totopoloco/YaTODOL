using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace YATODOL;

public static class DataService
{
    public static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YATODOL", "todos.json");

    public static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YATODOL", "settings.json");

    private static readonly JsonSerializerOptions IndentedOptions = new() { WriteIndented = true };

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
