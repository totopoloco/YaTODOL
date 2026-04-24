using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using YATODOL.Models;
using YATODOL.Services;
using YATODOL.Utilities;

namespace YATODOL.Views;

/// <summary>
/// Modal settings dialog for configuring theme, language, print options, task behavior, and custom data path.
/// </summary>
public partial class SettingsWindow : Window
{
    /// <summary>
    /// Gets the <see cref="AppSettings"/> built from the user's selections when the dialog is saved.
    /// </summary>
    public AppSettings Result { get; private set; } = new();

    private AppLanguage _previousLanguage = AppLanguage.English;
    private bool _closedByButton;

    // ── Tag management state ──────────────────────────────────────────────
    private List<TagDefinition> _customTags = [];
    private string _selectedTagColor = "#5b82a8";
    private HashSet<string> _usedTagNames = [];

    private static readonly string[] TagColorPalette =
    [
        "#b06060", "#b07840", "#a09040", "#4a8060",
        "#4a9080", "#5b82a8", "#7e6ea8", "#7a8490"
    ];

    public SettingsWindow()
    {
        InitializeComponent();
        Closing += OnWindowClosing;

        // Populate language ComboBox with native language names
        foreach (var name in Strings.LanguageNames)
            LanguageCombo.Items.Add(name);

        LanguageCombo.SelectionChanged += OnLanguageChanged;

        ApplyLocalization();
        BuildColorPicker();
        RefreshBuiltInTags();
    }

    /// <summary>
    /// Populates the dialog controls from the given settings.
    /// </summary>
    /// <param name="settings">The current application settings to display.</param>
    /// <param name="usedTagNames">Tag names currently assigned to at least one task (cannot be deleted).</param>
    public void LoadFrom(AppSettings settings, IEnumerable<string>? usedTagNames = null)
    {
        _usedTagNames = usedTagNames is null ? [] : new HashSet<string>(usedTagNames);
        _previousLanguage = settings.Language;

        ThemeLight.IsChecked = settings.Theme == ThemeMode.Light;
        ThemeDark.IsChecked = settings.Theme == ThemeMode.Dark;
        ThemeSystem.IsChecked = settings.Theme == ThemeMode.System;
        ShowPathCheck.IsChecked = settings.ShowPathInTitle;
        HideCompletedDatesCheck.IsChecked = settings.HideCompletedDates;
        CarryForwardCheck.IsChecked = settings.CarryForwardTasks;
        ScopeSelectedDate.IsChecked = settings.PrintScope == PrintScope.SelectedDate;
        ScopeAllDates.IsChecked = settings.PrintScope == PrintScope.AllDates;
        FilterAll.IsChecked = settings.PrintFilter == PrintFilter.AllItems;
        FilterRemaining.IsChecked = settings.PrintFilter == PrintFilter.RemainingOnly;
        LanguageCombo.SelectedIndex = (int)settings.Language;
        CustomPathCheck.IsChecked = settings.UseCustomDataPath;
        CustomPathBox.Text = settings.CustomDataPath;
        CustomPathPanel.IsVisible = settings.UseCustomDataPath;

        _customTags = settings.CustomTags
            .Select(t => new TagDefinition { Name = t.Name, Color = t.Color })
            .ToList();
        RefreshCustomTagsList();
    }

    private void OnLanguageChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (LanguageCombo.SelectedIndex >= 0)
        {
            Strings.SetLanguage((AppLanguage)LanguageCombo.SelectedIndex);
            ApplyLocalization();
        }
    }

    private void ApplyLocalization()
    {
        Title = Strings.SettingsWindowTitle;
        SettingsTitleText.Text = Strings.SettingsTitleLabel;
        SettingsSubtitleText.Text = Strings.SettingsSubtitle;
        CancelButton.Content = Strings.ButtonCancel;
        SaveButton.Content = Strings.ButtonSave;
        AppearanceSectionText.Text = Strings.SectionAppearance;
        ThemeLight.Content = Strings.ThemeLightLabel;
        ThemeDark.Content = Strings.ThemeDarkLabel;
        ThemeSystem.Content = Strings.ThemeSystemLabel;
        GeneralSectionText.Text = Strings.SectionGeneral;
        ShowPathLabelText.Text = Strings.ShowPathLabel;
        ShowPathDescText.Text = Strings.ShowPathDesc;
        HideCompletedLabelText.Text = Strings.HideCompletedLabel;
        HideCompletedDescText.Text = Strings.HideCompletedDesc;
        CarryForwardLabelText.Text = Strings.CarryForwardLabel;
        CarryForwardDescText.Text = Strings.CarryForwardDesc;
        PrintSectionText.Text = Strings.SectionPrinting;
        PrintScopeLabelText.Text = Strings.PrintScopeLabel;
        ScopeSelectedDate.Content = Strings.PrintScopeSelected;
        ScopeAllDates.Content = Strings.PrintScopeAll;
        PrintFilterLabelText.Text = Strings.PrintFilterLabel;
        FilterAll.Content = Strings.PrintFilterAll;
        FilterRemaining.Content = Strings.PrintFilterRemaining;
        LanguageSectionText.Text = Strings.SectionLanguage;
        LanguagePickerLabelText.Text = Strings.LanguagePickerLabel;
        StorageSectionText.Text = Strings.SectionStorage;
        CustomPathLabelText.Text = Strings.CustomPathLabel;
        CustomPathDescText.Text = Strings.CustomPathDesc;
        BrowseButton.Content = Strings.ButtonBrowse;
        CustomPathBox.PlaceholderText = Strings.CustomPathPlaceholder;
        TagsSectionText.Text = Strings.SectionTags;
        TagsBuiltInText.Text = Strings.TagsBuiltInHeader;
        TagsCustomText.Text = Strings.TagsCustomHeader;
        NewTagNameBox.PlaceholderText = Strings.TagsAddPlaceholder;
        AddTagButton.Content = Strings.TagsAddButton;
        TagsColorText.Text = Strings.TagsColorLabel;
        TagDuplicateWarning.Text = Strings.TagsDuplicateWarning;
        RefreshBuiltInTags();
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        Result = new AppSettings
        {
            Theme = ThemeDark.IsChecked == true ? ThemeMode.Dark
                  : ThemeSystem.IsChecked == true ? ThemeMode.System
                  : ThemeMode.Light,
            ShowPathInTitle = ShowPathCheck.IsChecked == true,
            HideCompletedDates = HideCompletedDatesCheck.IsChecked == true,
            CarryForwardTasks = CarryForwardCheck.IsChecked == true,
            PrintScope = ScopeAllDates.IsChecked == true ? PrintScope.AllDates : PrintScope.SelectedDate,
            PrintFilter = FilterRemaining.IsChecked == true ? PrintFilter.RemainingOnly : PrintFilter.AllItems,
            Language = LanguageCombo.SelectedIndex >= 0 ? (AppLanguage)LanguageCombo.SelectedIndex : AppLanguage.English,
            UseCustomDataPath = CustomPathCheck.IsChecked == true,
            CustomDataPath = CustomPathBox.Text ?? string.Empty,
            CustomTags = _customTags.Select(t => new TagDefinition { Name = t.Name, Color = t.Color }).ToList()
        };
        _closedByButton = true;
        Close(true);
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        if (!_closedByButton)
        {
            // Treat OS close as Cancel
            e.Cancel = true;
        }
    }

    private void OnCustomPathCheckChanged(object? sender, RoutedEventArgs e)
    {
        CustomPathPanel.IsVisible = CustomPathCheck.IsChecked == true;
    }

    private async void OnBrowseClick(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Strings.BrowseFolderTitle,
            AllowMultiple = false
        });
        if (folders.Count > 0)
        {
            CustomPathBox.Text = folders[0].Path.LocalPath;
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        // Restore language to what it was before this dialog was opened
        Strings.SetLanguage(_previousLanguage);
        _closedByButton = true;
        Close(false);
    }

    // ── Tag management ────────────────────────────────────────────────────

    private void RefreshBuiltInTags()
    {
        BuiltInTagsPanel.Children.Clear();
        foreach (var tag in BuiltInTags.All)
            BuiltInTagsPanel.Children.Add(MakeReadOnlyChip(tag));
    }

    private void RefreshCustomTagsList()
    {
        CustomTagsPanel.Children.Clear();
        foreach (var tag in _customTags)
        {
            var row = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto") };

            var dot = new Border
            {
                Width = 16, Height = 16,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.Parse(tag.Color)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            Grid.SetColumn(dot, 0);

            var label = new TextBlock
            {
                Text = tag.Name,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(label, 1);

            var inUse = _usedTagNames.Contains(tag.Name);
            var del = new Button
            {
                Content = inUse ? "🔒" : "✕",
                Background = Brushes.Transparent,
                Foreground = inUse ? Brushes.Gray : Brushes.Gray,
                Padding = new Thickness(6, 2),
                VerticalAlignment = VerticalAlignment.Center,
                IsEnabled = !inUse,
                Tag = tag.Name
            };
            ToolTip.SetTip(del, inUse ? Strings.TagsInUseTooltip : Strings.TagsDeleteTooltip);
            del.Click += OnDeleteCustomTagClick;
            Grid.SetColumn(del, 2);

            row.Children.Add(dot);
            row.Children.Add(label);
            row.Children.Add(del);
            CustomTagsPanel.Children.Add(row);
        }
    }

    private Border MakeReadOnlyChip(TagDefinition tag)
    {
        var col = Color.Parse(tag.Color);
        return new Border
        {
            CornerRadius = new CornerRadius(12),
            Background = new SolidColorBrush(col),
            Padding = new Thickness(10, 5),
            Margin = new Thickness(0, 0, 6, 4),
            Child = new TextBlock
            {
                Text = Strings.GetTagDisplayName(tag.Name),
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
    }

    private void BuildColorPicker()
    {
        TagColorPicker.Children.Clear();
        foreach (var hex in TagColorPalette)
        {
            var swatch = MakeColorSwatch(hex, hex == _selectedTagColor);
            TagColorPicker.Children.Add(swatch);
        }
    }

    private Grid MakeColorSwatch(string hex, bool selected)
    {
        var grid = new Grid { Width = 26, Height = 26, Cursor = new Cursor(StandardCursorType.Hand), Margin = new Thickness(0, 0, 6, 0) };
        grid.Children.Add(new Border
        {
            CornerRadius = new CornerRadius(13),
            Background = new SolidColorBrush(Color.Parse(hex))
        });
        if (selected)
        {
            grid.Children.Add(new TextBlock
            {
                Text = "✓",
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12,
                FontWeight = FontWeight.Bold
            });
        }
        grid.Tag = hex;
        grid.PointerPressed += OnColorSwatchPressed;
        return grid;
    }

    private void OnColorSwatchPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Grid { Tag: string hex })
        {
            _selectedTagColor = hex;
            BuildColorPicker();
        }
    }

    private void OnAddTagClick(object? sender, RoutedEventArgs e)
    {
        var name = NewTagNameBox.Text?.Trim();
        if (string.IsNullOrEmpty(name)) return;

        var allExisting = BuiltInTags.All.Select(t => t.Name)
            .Concat(_customTags.Select(t => t.Name));

        if (allExisting.Any(n => string.Equals(n, name, System.StringComparison.OrdinalIgnoreCase)))
        {
            TagDuplicateWarning.IsVisible = true;
            return;
        }

        TagDuplicateWarning.IsVisible = false;
        _customTags.Add(new TagDefinition { Name = name, Color = _selectedTagColor });
        NewTagNameBox.Text = string.Empty;
        RefreshCustomTagsList();
    }

    private void OnDeleteCustomTagClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string tagName })
        {
            _customTags.RemoveAll(t => t.Name == tagName);
            RefreshCustomTagsList();
        }
    }
}
