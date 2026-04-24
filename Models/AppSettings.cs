using System.Collections.Generic;
using System.Linq;

namespace YATODOL.Models;

/// <summary>
/// Represents a user-defined tag with a name and a hex color string.
/// </summary>
public class TagDefinition
{
    /// <summary>Gets or sets the canonical tag name (storage key).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the tag color as a hex string, e.g. "#3498db".</summary>
    public string Color { get; set; } = "#888888";
}

/// <summary>
/// Provides the three built-in, non-deletable tags.
/// </summary>
public static class BuiltInTags
{
    public const string Urgent    = "Urgent";
    public const string Important = "Important";
    public const string Low       = "Low";

    /// <summary>All built-in tag definitions (fixed color, fixed name key).</summary>
    public static readonly IReadOnlyList<TagDefinition> All =
    [
        new TagDefinition { Name = Urgent,    Color = "#b06060" },
        new TagDefinition { Name = Important, Color = "#b07840" },
        new TagDefinition { Name = Low,       Color = "#4a8060" }
    ];

    /// <summary>Returns true when <paramref name="name"/> matches a built-in tag key.</summary>
    public static bool IsBuiltIn(string name) => name is Urgent or Important or Low;
}



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

    /// <summary>Gets or sets user-defined custom tags (built-in tags are not stored here).</summary>
    public List<TagDefinition> CustomTags { get; set; } = [];

    /// <summary>Returns all tags: built-in first, then custom.</summary>
    public IEnumerable<TagDefinition> GetAllTags() =>
        BuiltInTags.All.Concat(CustomTags);
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
