using YATODOL.Models;

namespace YATODOL.Tests.Models;

public class AppSettingsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var settings = new AppSettings();

        Assert.Equal(ThemeMode.Light, settings.Theme);
        Assert.False(settings.ShowPathInTitle);
        Assert.False(settings.HideCompletedDates);
        Assert.True(settings.CarryForwardTasks);
        Assert.Equal(PrintScope.SelectedDate, settings.PrintScope);
        Assert.Equal(PrintFilter.AllItems, settings.PrintFilter);
        Assert.Equal(AppLanguage.English, settings.Language);
        Assert.False(settings.UseCustomDataPath);
        Assert.Equal(string.Empty, settings.CustomDataPath);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var settings = new AppSettings
        {
            Theme = ThemeMode.Dark,
            ShowPathInTitle = true,
            HideCompletedDates = true,
            CarryForwardTasks = false,
            PrintScope = PrintScope.AllDates,
            PrintFilter = PrintFilter.RemainingOnly,
            Language = AppLanguage.Spanish,
            UseCustomDataPath = true,
            CustomDataPath = "/custom/path"
        };

        Assert.Equal(ThemeMode.Dark, settings.Theme);
        Assert.True(settings.ShowPathInTitle);
        Assert.True(settings.HideCompletedDates);
        Assert.False(settings.CarryForwardTasks);
        Assert.Equal(PrintScope.AllDates, settings.PrintScope);
        Assert.Equal(PrintFilter.RemainingOnly, settings.PrintFilter);
        Assert.Equal(AppLanguage.Spanish, settings.Language);
        Assert.True(settings.UseCustomDataPath);
        Assert.Equal("/custom/path", settings.CustomDataPath);
    }

    [Fact]
    public void ExportData_DefaultValues_AreCorrect()
    {
        var data = new ExportData();

        Assert.NotNull(data.Todos);
        Assert.Empty(data.Todos);
        Assert.NotNull(data.Settings);
    }

    [Fact]
    public void ExportData_CanHoldTodosAndSettings()
    {
        var data = new ExportData
        {
            Todos = [new TodoItem { Title = "Test" }],
            Settings = new AppSettings { Theme = ThemeMode.Dark }
        };

        Assert.Single(data.Todos);
        Assert.Equal("Test", data.Todos[0].Title);
        Assert.Equal(ThemeMode.Dark, data.Settings.Theme);
    }

    [Theory]
    [InlineData(ThemeMode.Light)]
    [InlineData(ThemeMode.Dark)]
    [InlineData(ThemeMode.System)]
    public void ThemeMode_AllValues_AreValid(ThemeMode mode)
    {
        var settings = new AppSettings { Theme = mode };
        Assert.Equal(mode, settings.Theme);
    }

    [Theory]
    [InlineData(AppLanguage.English)]
    [InlineData(AppLanguage.Spanish)]
    [InlineData(AppLanguage.German)]
    [InlineData(AppLanguage.French)]
    public void AppLanguage_AllValues_AreValid(AppLanguage lang)
    {
        var settings = new AppSettings { Language = lang };
        Assert.Equal(lang, settings.Language);
    }

    // ── TagDefinition tests ───────────────────────────────────────────────

    [Fact]
    public void TagDefinition_DefaultValues_AreCorrect()
    {
        var tag = new TagDefinition();
        Assert.Equal(string.Empty, tag.Name);
        Assert.Equal("#888888", tag.Color);
    }

    [Fact]
    public void TagDefinition_CanSetNameAndColor()
    {
        var tag = new TagDefinition { Name = "Work", Color = "#5b82a8" };
        Assert.Equal("Work", tag.Name);
        Assert.Equal("#5b82a8", tag.Color);
    }

    // ── BuiltInTags tests ─────────────────────────────────────────────────

    [Fact]
    public void BuiltInTags_All_ContainsExactlyThreeEntries()
    {
        Assert.Equal(3, BuiltInTags.All.Count);
    }

    [Fact]
    public void BuiltInTags_All_ContainsUrgentImportantLow()
    {
        var names = BuiltInTags.All.Select(t => t.Name).ToList();
        Assert.Contains(BuiltInTags.Urgent, names);
        Assert.Contains(BuiltInTags.Important, names);
        Assert.Contains(BuiltInTags.Low, names);
    }

    [Theory]
    [InlineData(BuiltInTags.Urgent)]
    [InlineData(BuiltInTags.Important)]
    [InlineData(BuiltInTags.Low)]
    public void BuiltInTags_IsBuiltIn_ReturnsTrueForBuiltIns(string name)
    {
        Assert.True(BuiltInTags.IsBuiltIn(name));
    }

    [Theory]
    [InlineData("Work")]
    [InlineData("Personal")]
    [InlineData("urgent")]   // case-sensitive
    [InlineData("")]
    public void BuiltInTags_IsBuiltIn_ReturnsFalseForCustomOrEmpty(string name)
    {
        Assert.False(BuiltInTags.IsBuiltIn(name));
    }

    [Fact]
    public void BuiltInTags_All_EachHasNonEmptyColor()
    {
        foreach (var tag in BuiltInTags.All)
            Assert.False(string.IsNullOrEmpty(tag.Color));
    }

    // ── AppSettings tag integration ───────────────────────────────────────

    [Fact]
    public void AppSettings_CustomTags_DefaultIsEmpty()
    {
        var settings = new AppSettings();
        Assert.NotNull(settings.CustomTags);
        Assert.Empty(settings.CustomTags);
    }

    [Fact]
    public void GetAllTags_NoCustomTags_ReturnsOnlyBuiltIns()
    {
        var settings = new AppSettings();
        var all = settings.GetAllTags().ToList();
        Assert.Equal(3, all.Count);
    }

    [Fact]
    public void GetAllTags_WithCustomTags_ReturnsBuiltInsAndCustom()
    {
        var settings = new AppSettings
        {
            CustomTags = [new TagDefinition { Name = "Work", Color = "#5b82a8" }]
        };
        var all = settings.GetAllTags().ToList();
        Assert.Equal(4, all.Count);
        Assert.Contains(all, t => t.Name == "Work");
        Assert.Contains(all, t => t.Name == BuiltInTags.Urgent);
    }

    [Fact]
    public void GetAllTags_BuiltInsAlwaysFirst()
    {
        var settings = new AppSettings
        {
            CustomTags = [new TagDefinition { Name = "ZZZ", Color = "#aaaaaa" }]
        };
        var all = settings.GetAllTags().ToList();
        Assert.Equal(BuiltInTags.Urgent,    all[0].Name);
        Assert.Equal(BuiltInTags.Important, all[1].Name);
        Assert.Equal(BuiltInTags.Low,       all[2].Name);
        Assert.Equal("ZZZ",                 all[3].Name);
    }
}
