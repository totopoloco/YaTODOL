using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using YATODOL.Models;

namespace YATODOL.Services;

/// <summary>
/// Provides persistence operations for to-do items and application settings,
/// including JSON serialization, file I/O with retry logic, and custom data path management.
/// </summary>
public static class DataService
{
    private static readonly string DefaultDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YATODOL");

    /// <summary>
    /// Gets the absolute path to the <c>settings.json</c> file (always in the default data directory).
    /// </summary>
    public static readonly string SettingsPath = Path.Combine(DefaultDataDir, "settings.json");

    private static string _dataDir = DefaultDataDir;

    /// <summary>
    /// Gets the absolute path to the <c>todos.json</c> file in the current data directory.
    /// </summary>
    public static string SavePath => Path.Combine(_dataDir, "todos.json");

    private static readonly JsonSerializerOptions IndentedOptions = new() { WriteIndented = true };

    /// <summary>
    /// Updates the active data directory based on the custom path settings.
    /// </summary>
    /// <param name="settings">The current application settings.</param>
    public static void ApplyCustomPath(AppSettings settings)
    {
        _dataDir = settings.UseCustomDataPath && !string.IsNullOrWhiteSpace(settings.CustomDataPath)
            ? settings.CustomDataPath
            : DefaultDataDir;
    }

    /// <summary>
    /// Moves <c>todos.json</c> from <paramref name="oldPath"/> to the directory specified in <paramref name="newSettings"/>.
    /// If the target already exists, the source is kept as a <c>.bak</c> backup.
    /// </summary>
    /// <param name="oldPath">The current file path before the move.</param>
    /// <param name="newSettings">The settings containing the new data path.</param>
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

    /// <summary>
    /// Loads to-do items from <see cref="SavePath"/>. Returns an empty list if the file is missing or corrupt.
    /// </summary>
    /// <returns>The deserialized list of <see cref="TodoItem"/> objects.</returns>
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

    /// <summary>
    /// Serializes and saves the given to-do items to <see cref="SavePath"/> using atomic write-with-retry.
    /// </summary>
    /// <param name="items">The items to persist.</param>
    /// <returns><c>true</c> if the save succeeded; otherwise <c>false</c>.</returns>
    public static bool SaveTodos(IEnumerable<TodoItem> items)
    {
        var json = JsonSerializer.Serialize(new List<TodoItem>(items));
        return WriteWithRetry(SavePath, json);
    }

    /// <summary>
    /// Loads application settings from <see cref="SettingsPath"/>. Returns defaults if the file is missing or corrupt.
    /// </summary>
    /// <returns>The deserialized <see cref="AppSettings"/> instance.</returns>
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

    /// <summary>
    /// Serializes and saves application settings to <see cref="SettingsPath"/> using atomic write-with-retry.
    /// </summary>
    /// <param name="settings">The settings to persist.</param>
    /// <returns><c>true</c> if the save succeeded; otherwise <c>false</c>.</returns>
    public static bool SaveSettings(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings);
        return WriteWithRetry(SettingsPath, json);
    }

    /// <summary>
    /// Serializes an <see cref="ExportData"/> instance to indented JSON for file export.
    /// </summary>
    /// <param name="data">The export data to serialize.</param>
    /// <returns>A JSON string.</returns>
    public static string SerializeExport(ExportData data) =>
        JsonSerializer.Serialize(data, IndentedOptions);

    /// <summary>
    /// Deserializes a JSON string into an <see cref="ExportData"/> instance for file import.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="ExportData"/>, or <c>null</c> if deserialization fails.</returns>
    public static ExportData? DeserializeImport(string json) =>
        JsonSerializer.Deserialize<ExportData>(json);

    private static bool WriteWithRetry(string path, string content, int maxRetries = 3)
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                var tempPath = path + ".tmp";
                File.WriteAllText(tempPath, content);
                File.Move(tempPath, path, overwrite: true);
                return true;
            }
            catch (IOException) when (attempt < maxRetries)
            {
                Thread.Sleep(100 * (attempt + 1));
            }
            catch (IOException)
            {
                return false;
            }
        }
        return false;
    }
}
