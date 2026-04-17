using System.Collections.Generic;

namespace YATODOL;

public enum ThemeMode
{
    Light,
    Dark,
    System
}

public enum AppLanguage
{
    English,
    Spanish,
    German,
    French
}

public enum PrintScope
{
    SelectedDate,
    AllDates
}

public enum PrintFilter
{
    AllItems,
    RemainingOnly
}

public class AppSettings
{
    public ThemeMode Theme { get; set; } = ThemeMode.Light;
    public bool ShowPathInTitle { get; set; }
    public bool HideCompletedDates { get; set; }
    public bool CarryForwardTasks { get; set; } = true;
    public PrintScope PrintScope { get; set; } = PrintScope.SelectedDate;
    public PrintFilter PrintFilter { get; set; } = PrintFilter.AllItems;
    public AppLanguage Language { get; set; } = AppLanguage.English;
}

public class ExportData
{
    public List<TodoItem> Todos { get; set; } = [];
    public AppSettings Settings { get; set; } = new();
}
