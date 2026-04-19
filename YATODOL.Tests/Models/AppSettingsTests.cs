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
}
