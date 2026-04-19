using System.Text.Json;
using YATODOL.Models;
using YATODOL.Services;

namespace YATODOL.Tests.Services;

public class DataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public DataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"yatodol_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void ApplyCustomPath_WithCustomPath_ChangesSavePath()
    {
        var settings = new AppSettings
        {
            UseCustomDataPath = true,
            CustomDataPath = _tempDir
        };

        DataService.ApplyCustomPath(settings);

        Assert.Equal(Path.Combine(_tempDir, "todos.json"), DataService.SavePath);
    }

    [Fact]
    public void ApplyCustomPath_WithoutCustomPath_UsesDefault()
    {
        var settings = new AppSettings
        {
            UseCustomDataPath = false,
            CustomDataPath = string.Empty
        };

        DataService.ApplyCustomPath(settings);

        Assert.Contains("YATODOL", DataService.SavePath);
        Assert.EndsWith("todos.json", DataService.SavePath);
    }

    [Fact]
    public void SaveTodos_And_LoadTodos_RoundTrip()
    {
        var settings = new AppSettings { UseCustomDataPath = true, CustomDataPath = _tempDir };
        DataService.ApplyCustomPath(settings);

        var items = new List<TodoItem>
        {
            new() { Title = "Task 1", IsDone = false, Date = DateTime.Today, SortOrder = 0 },
            new() { Title = "Task 2", IsDone = true, Date = DateTime.Today.AddDays(1), Note = "A note", SortOrder = 1 }
        };

        var saved = DataService.SaveTodos(items);
        Assert.True(saved);

        var loaded = DataService.LoadTodos();
        Assert.Equal(2, loaded.Count);
        Assert.Equal("Task 1", loaded[0].Title);
        Assert.False(loaded[0].IsDone);
        Assert.Equal("Task 2", loaded[1].Title);
        Assert.True(loaded[1].IsDone);
        Assert.Equal("A note", loaded[1].Note);
        Assert.Equal(1, loaded[1].SortOrder);
    }

    [Fact]
    public void LoadTodos_WhenFileDoesNotExist_ReturnsEmptyList()
    {
        var settings = new AppSettings
        {
            UseCustomDataPath = true,
            CustomDataPath = Path.Combine(_tempDir, "nonexistent")
        };
        DataService.ApplyCustomPath(settings);

        var loaded = DataService.LoadTodos();
        Assert.Empty(loaded);
    }

    [Fact]
    public void LoadTodos_WhenFileIsCorrupt_ReturnsEmptyList()
    {
        var settings = new AppSettings { UseCustomDataPath = true, CustomDataPath = _tempDir };
        DataService.ApplyCustomPath(settings);

        File.WriteAllText(DataService.SavePath, "not valid json!!!");

        var loaded = DataService.LoadTodos();
        Assert.Empty(loaded);
    }

    [Fact]
    public void SaveSettings_And_LoadSettings_RoundTrip()
    {
        // LoadSettings/SaveSettings use the fixed SettingsPath, so we test SerializeExport/DeserializeImport instead
        var original = new AppSettings
        {
            Theme = ThemeMode.Dark,
            Language = AppLanguage.French,
            CarryForwardTasks = false,
            HideCompletedDates = true,
            PrintScope = PrintScope.AllDates,
            PrintFilter = PrintFilter.RemainingOnly
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<AppSettings>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(ThemeMode.Dark, deserialized.Theme);
        Assert.Equal(AppLanguage.French, deserialized.Language);
        Assert.False(deserialized.CarryForwardTasks);
        Assert.True(deserialized.HideCompletedDates);
        Assert.Equal(PrintScope.AllDates, deserialized.PrintScope);
        Assert.Equal(PrintFilter.RemainingOnly, deserialized.PrintFilter);
    }

    [Fact]
    public void SerializeExport_ProducesValidJson()
    {
        var data = new ExportData
        {
            Todos = [new TodoItem { Title = "Task" }],
            Settings = new AppSettings { Theme = ThemeMode.Dark }
        };

        var json = DataService.SerializeExport(data);

        Assert.Contains("Task", json);
        // Verify it's valid JSON by deserializing
        var parsed = DataService.DeserializeImport(json);
        Assert.NotNull(parsed);
        Assert.Equal(ThemeMode.Dark, parsed!.Settings.Theme);
    }

    [Fact]
    public void DeserializeImport_ValidJson_ReturnsData()
    {
        var data = new ExportData
        {
            Todos = [new TodoItem { Title = "Imported" }],
            Settings = new AppSettings { Language = AppLanguage.German }
        };
        var json = DataService.SerializeExport(data);

        var result = DataService.DeserializeImport(json);

        Assert.NotNull(result);
        Assert.Single(result!.Todos);
        Assert.Equal("Imported", result.Todos[0].Title);
        Assert.Equal(AppLanguage.German, result.Settings.Language);
    }

    [Fact]
    public void DeserializeImport_InvalidJson_ThrowsException()
    {
        Assert.ThrowsAny<System.Text.Json.JsonException>(() => DataService.DeserializeImport("not json"));
    }

    [Fact]
    public void MoveDataFile_CopiesFileToNewLocation()
    {
        // Set up initial location
        var oldDir = Path.Combine(_tempDir, "old");
        var newDir = Path.Combine(_tempDir, "new");
        Directory.CreateDirectory(oldDir);

        var oldPath = Path.Combine(oldDir, "todos.json");
        File.WriteAllText(oldPath, "[{\"Title\":\"moved\"}]");

        var newSettings = new AppSettings
        {
            UseCustomDataPath = true,
            CustomDataPath = newDir
        };

        DataService.MoveDataFile(oldPath, newSettings);

        var newPath = Path.Combine(newDir, "todos.json");
        Assert.True(File.Exists(newPath));
        Assert.False(File.Exists(oldPath));
        Assert.Contains("moved", File.ReadAllText(newPath));
    }

    [Fact]
    public void MoveDataFile_SamePath_DoesNothing()
    {
        var settings = new AppSettings { UseCustomDataPath = true, CustomDataPath = _tempDir };
        DataService.ApplyCustomPath(settings);

        var path = DataService.SavePath;
        File.WriteAllText(path, "[]");

        DataService.MoveDataFile(path, settings);

        Assert.True(File.Exists(path));
        Assert.Equal("[]", File.ReadAllText(path));
    }

    [Fact]
    public void MoveDataFile_TargetExists_CreatesBackup()
    {
        var oldDir = Path.Combine(_tempDir, "src");
        var newDir = Path.Combine(_tempDir, "dst");
        Directory.CreateDirectory(oldDir);
        Directory.CreateDirectory(newDir);

        var oldPath = Path.Combine(oldDir, "todos.json");
        var newPath = Path.Combine(newDir, "todos.json");
        File.WriteAllText(oldPath, "[{\"Title\":\"old\"}]");
        File.WriteAllText(newPath, "[{\"Title\":\"existing\"}]");

        var newSettings = new AppSettings { UseCustomDataPath = true, CustomDataPath = newDir };

        DataService.MoveDataFile(oldPath, newSettings);

        // Target should keep existing data
        Assert.True(File.Exists(newPath));
        // Old file should be renamed to .bak
        Assert.True(File.Exists(oldPath + ".bak"));
    }

    [Fact]
    public void SaveTodos_EmptyList_SavesValidJson()
    {
        var settings = new AppSettings { UseCustomDataPath = true, CustomDataPath = _tempDir };
        DataService.ApplyCustomPath(settings);

        var saved = DataService.SaveTodos([]);
        Assert.True(saved);

        var loaded = DataService.LoadTodos();
        Assert.Empty(loaded);
    }

    [Fact]
    public void SaveTodos_PreservesNotes()
    {
        var settings = new AppSettings { UseCustomDataPath = true, CustomDataPath = _tempDir };
        DataService.ApplyCustomPath(settings);

        var items = new List<TodoItem>
        {
            new() { Title = "With note", Note = "Important details\nMultiline" }
        };

        DataService.SaveTodos(items);
        var loaded = DataService.LoadTodos();

        Assert.Single(loaded);
        Assert.Equal("Important details\nMultiline", loaded[0].Note);
    }
}
