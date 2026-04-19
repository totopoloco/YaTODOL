using System.Collections.Generic;

namespace YATODOL.Models;

/// <summary>
/// Specifies the application color theme.
/// </summary>
public enum ThemeMode
{
    /// <summary>Light theme.</summary>
    Light,
    /// <summary>Dark theme.</summary>
    Dark,
    /// <summary>Follow the operating system theme.</summary>
    System
}

/// <summary>
/// Specifies the application display language.
/// </summary>
public enum AppLanguage
{
    /// <summary>English.</summary>
    English,
    /// <summary>Spanish (Español).</summary>
    Spanish,
    /// <summary>German (Deutsch).</summary>
    German,
    /// <summary>French (Français).</summary>
    French
}

/// <summary>
/// Specifies which date range to include when printing tasks.
/// </summary>
public enum PrintScope
{
    /// <summary>Print tasks for the currently selected date only.</summary>
    SelectedDate,
    /// <summary>Print tasks across all dates.</summary>
    AllDates
}

/// <summary>
/// Specifies which task completion states to include when printing.
/// </summary>
public enum PrintFilter
{
    /// <summary>Print both completed and remaining tasks.</summary>
    AllItems,
    /// <summary>Print only tasks that are not yet completed.</summary>
    RemainingOnly
}

/// <summary>
/// Stores user-configurable application settings, serialized to <c>settings.json</c>.
/// </summary>
public class AppSettings
{
    /// <summary>Gets or sets the color theme.</summary>
    public ThemeMode Theme { get; set; } = ThemeMode.Light;

    /// <summary>Gets or sets whether the data file path is shown in the window title.</summary>
    public bool ShowPathInTitle { get; set; }

    /// <summary>Gets or sets whether dates with only completed tasks are hidden from the accordion.</summary>
    public bool HideCompletedDates { get; set; }

    /// <summary>Gets or sets whether uncompleted past tasks are automatically carried forward to today at midnight.</summary>
    public bool CarryForwardTasks { get; set; } = true;

    /// <summary>Gets or sets the date scope used when printing.</summary>
    public PrintScope PrintScope { get; set; } = PrintScope.SelectedDate;

    /// <summary>Gets or sets the completion filter used when printing.</summary>
    public PrintFilter PrintFilter { get; set; } = PrintFilter.AllItems;

    /// <summary>Gets or sets the display language.</summary>
    public AppLanguage Language { get; set; } = AppLanguage.English;

    /// <summary>Gets or sets whether a custom data storage path is enabled.</summary>
    public bool UseCustomDataPath { get; set; }

    /// <summary>Gets or sets the custom directory path for data file storage.</summary>
    public string CustomDataPath { get; set; } = string.Empty;
}

/// <summary>
/// Container for import/export of the full application state (tasks and settings).
/// </summary>
public class ExportData
{
    /// <summary>Gets or sets the list of all to-do items.</summary>
    public List<TodoItem> Todos { get; set; } = [];

    /// <summary>Gets or sets the application settings.</summary>
    public AppSettings Settings { get; set; } = new();
}
