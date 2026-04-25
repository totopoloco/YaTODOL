using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using YATODOL.Models;
using YATODOL.Services;
using YATODOL.Utilities;

namespace YATODOL.Views;

/// <summary>
/// Primary application window containing the task accordion, date navigation,
/// and all task management logic (add, delete, complete, reorder, import/export, print).
/// </summary>
public partial class MainWindow : Window
{
    private readonly ObservableCollection<TodoItem> _allItems = new();
    private readonly Dictionary<DateTime, Expander> _expanders = new();
    private readonly AccordionBuilder _accordionBuilder;
    private DateTime _selectedDate = DateTime.Today;
    private Timer? _midnightTimer;
    private Timer? _warningTimer;

    private AppSettings _settings = new();

    // ── Filter state ─────────────────────────────────────────────────────
    private readonly HashSet<string> _activeTagFilters = [];
    private string _searchQuery = string.Empty;
    private bool _searchIsRegex;

    public MainWindow()
    {
        InitializeComponent();
        _accordionBuilder = new AccordionBuilder(OnCheckChanged, OnDeleteClick, OnNoteClick, OnReorder, OnRenameComplete);
        DatePicker.SelectedDate = _selectedDate;
        DatePicker.SelectedDateChanged += OnDateChanged;
        _settings = DataService.LoadSettings();
        DataService.ApplyCustomPath(_settings);
        _accordionBuilder.Tags = _settings.GetAllTags();
        Strings.SetLanguage(_settings.Language);
        ApplyTheme();
        ApplyLocalization();
        UpdateTitle();
        foreach (var item in DataService.LoadTodos())
            _allItems.Add(item);
        CarryForwardPastTasks();
        RebuildAccordion();
        RebuildFilterPanel();
        ScheduleMidnightCheck();
    }

    protected override void OnClosed(EventArgs e)
    {
        _midnightTimer?.Dispose();
        base.OnClosed(e);
    }

    private void ScheduleMidnightCheck()
    {
        var now = DateTime.Now;
        var nextMidnight = DateTime.Today.AddDays(1);
        var delay = nextMidnight - now + TimeSpan.FromSeconds(1); // 1s past midnight

        _midnightTimer?.Dispose();
        _midnightTimer = new Timer(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                CarryForwardPastTasks();
                _selectedDate = DateTime.Today;
                DatePicker.SelectedDate = _selectedDate;
                RebuildAccordion();
                ScheduleMidnightCheck(); // reschedule for next midnight
            });
        }, null, delay, Timeout.InfiniteTimeSpan);
    }

    private void CarryForwardPastTasks()
    {
        if (!_settings.CarryForwardTasks) return;

        var today = DateTime.Today;
        var moved = false;
        var maxOrder = _allItems
            .Where(i => i.Date.Date == today)
            .Select(i => i.SortOrder)
            .DefaultIfEmpty(-1)
            .Max();

        foreach (var item in _allItems.Where(i => i.Date.Date < today && !i.IsDone))
        {
            item.Date = today;
            item.SortOrder = ++maxOrder;
            moved = true;
        }

        if (moved) Save();
    }

    private void Save()
    {
        if (!DataService.SaveTodos(_allItems))
            ShowWarning(Strings.SaveFailed);
    }

    private void OnAddClick(object? sender, RoutedEventArgs e) => AddItem();

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            AddItem();
    }

    private void AddItem()
    {
        var text = NewItemBox.Text?.Trim();
        if (string.IsNullOrEmpty(text)) return;

        if (_selectedDate.Date < DateTime.Today)
        {
            ShowWarning(Strings.NoPastTasks);
            return;
        }

        HideWarning();
        var maxSortOrder = _allItems
            .Where(i => i.Date.Date == _selectedDate.Date)
            .Select(i => i.SortOrder)
            .DefaultIfEmpty(-1)
            .Max();
        var item = new TodoItem { Title = text, Date = _selectedDate, SortOrder = maxSortOrder + 1 };
        _allItems.Add(item);
        NewItemBox.Text = string.Empty;
        NewItemBox.PlaceholderText = Strings.PlaceholderNewTask;
        RebuildAccordion();
        Save();
        NewItemBox.Focus();
    }

    private void ShowWarning(string message)
    {
        WarningLabel.Text = message;
        WarningLabel.IsVisible = true;
        _warningTimer?.Dispose();
        _warningTimer = new Timer(_ =>
            Dispatcher.UIThread.Post(HideWarning),
            null, TimeSpan.FromSeconds(4), Timeout.InfiniteTimeSpan);
    }

    private void HideWarning()
    {
        WarningLabel.IsVisible = false;
        _warningTimer?.Dispose();
        _warningTimer = null;
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is TodoItem item)
        {
            _allItems.Remove(item);
            RebuildAccordion();
            Save();
        }
    }

    private void OnCheckChanged(object? sender, RoutedEventArgs e)
    {
        // Remember which dates are expanded
        var expanded = _expanders.Where(kv => kv.Value.IsExpanded).Select(kv => kv.Key).ToHashSet();
        RebuildAccordion(expanded);
        Save();
    }

    private void OnReorder(TodoItem draggedItem, int newIndex)
    {
        var date = draggedItem.Date.Date;
        var dateItems = _allItems
            .Where(i => i.Date.Date == date)
            .OrderBy(i => i.SortOrder)
            .ToList();

        var oldIndex = dateItems.IndexOf(draggedItem);
        if (oldIndex < 0 || oldIndex == newIndex) return;

        dateItems.Remove(draggedItem);
        newIndex = Math.Clamp(newIndex, 0, dateItems.Count);
        dateItems.Insert(newIndex, draggedItem);

        for (int i = 0; i < dateItems.Count; i++)
            dateItems[i].SortOrder = i;

        var expanded = _expanders.Where(kv => kv.Value.IsExpanded).Select(kv => kv.Key).ToHashSet();
        RebuildAccordion(expanded);
        Save();
    }

    private void OnRenameComplete(TodoItem item, string newTitle)
    {
        item.Title = newTitle;
        Save();
    }

    private void OnDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DatePicker.SelectedDate is { } date)
        {
            _selectedDate = date.Date;
            NavigateToDate(_selectedDate);
        }
    }

    private void OnPrevDay(object? sender, RoutedEventArgs e)
    {
        _selectedDate = _selectedDate.AddDays(-1);
        DatePicker.SelectedDate = _selectedDate;
    }

    private void OnNextDay(object? sender, RoutedEventArgs e)
    {
        _selectedDate = _selectedDate.AddDays(1);
        DatePicker.SelectedDate = _selectedDate;
    }

    private void OnTodayClick(object? sender, RoutedEventArgs e)
    {
        _selectedDate = DateTime.Today;
        DatePicker.SelectedDate = _selectedDate;
    }

    private async void OnDeleteDateClick(object? sender, RoutedEventArgs e)
    {
        var items = _allItems.Where(i => i.Date.Date == _selectedDate.Date).ToList();
        if (items.Count == 0) return;

        var message = Strings.ConfirmDeleteText(items.Count, _selectedDate.ToString("ddd, MMM d, yyyy"));
        if (!await DialogService.ShowConfirmDialog(this,
            Strings.ConfirmDeleteTitle, message, Strings.ButtonDelete, Strings.ButtonCancel))
            return;

        foreach (var item in items)
            _allItems.Remove(item);
        RebuildAccordion();
        Save();
    }

    private void NavigateToDate(DateTime date)
    {
        // Collapse all, expand selected
        foreach (var (d, exp) in _expanders)
            exp.IsExpanded = d == date.Date;

        // Scroll into view
        if (_expanders.TryGetValue(date.Date, out var target))
            target.BringIntoView();

        UpdateCount();
    }

    private void RebuildAccordion(HashSet<DateTime>? expandedDates = null)
    {
        AccordionPanel.Children.Clear();
        _expanders.Clear();

        var filtered = ApplyFilters(_allItems);
        var groups = filtered.OrderByDescending(i => i.Date).GroupBy(i => i.Date.Date);

        foreach (var group in groups.Where(g =>
            !_settings.HideCompletedDates || g.Any(i => !i.IsDone)))
        {
            var date = group.Key;
            var isExpanded = expandedDates?.Contains(date) ?? date == _selectedDate.Date;
            var expander = _accordionBuilder.CreateDateExpander(date, group.OrderBy(i => i.SortOrder).ToList(), isExpanded);

            expander.PropertyChanged += (s, e) =>
            {
                if (e.Property == Expander.IsExpandedProperty && expander.IsExpanded && expander.Tag is DateTime d)
                {
                    _selectedDate = d;
                    DatePicker.SelectedDate = d;
                }
            };

            AccordionPanel.Children.Add(expander);
            _expanders[date] = expander;
        }

        UpdateCount();
    }

    // ── Filter & search ──────────────────────────────────────────────────

    private IEnumerable<TodoItem> ApplyFilters(IEnumerable<TodoItem> items)
    {
        if (_activeTagFilters.Count > 0)
            items = items.Where(i => i.Tags.Any(t => _activeTagFilters.Contains(t)));

        var q = _searchQuery;
        if (!string.IsNullOrWhiteSpace(q))
        {
            if (_searchIsRegex)
            {
                try
                {
                    var rx = new Regex(q, RegexOptions.IgnoreCase);
                    items = items.Where(i => rx.IsMatch(i.Title) || (i.Note != null && rx.IsMatch(i.Note)));
                }
                catch (RegexParseException) { /* invalid pattern — show all */ }
            }
            else
            {
                // Wildcard mode: * = any chars, ? = one char; plain text = substring match
                bool hasWildcard = q.Contains('*') || q.Contains('?');
                if (hasWildcard)
                {
                    var pattern = "^" + Regex.Escape(q)
                        .Replace(@"\*", ".*")
                        .Replace(@"\?", ".") + "$";
                    try
                    {
                        var rx = new Regex(pattern, RegexOptions.IgnoreCase);
                        items = items.Where(i => rx.IsMatch(i.Title) || (i.Note != null && rx.IsMatch(i.Note)));
                    }
                    catch (RegexParseException) { }
                }
                else
                {
                    items = items.Where(i =>
                        i.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        (i.Note != null && i.Note.Contains(q, StringComparison.OrdinalIgnoreCase)));
                }
            }
        }

        return items;
    }

    private void RebuildFilterPanel()
    {
        FilterTagsPanel.Children.Clear();
        foreach (var tag in _settings.GetAllTags())
        {
            var isActive = _activeTagFilters.Contains(tag.Name);
            var col = Color.Parse(tag.Color);
            var fullBrush = new SolidColorBrush(col);

            var chip = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = isActive
                    ? fullBrush
                    : new SolidColorBrush(Color.FromArgb(30, col.R, col.G, col.B)),
                BorderBrush = fullBrush,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8, 4),
                Margin = new Thickness(0, 0, 4, 4),
                Cursor = new Cursor(StandardCursorType.Hand),
                Child = new TextBlock
                {
                    Text = Strings.GetTagDisplayName(tag.Name),
                    Foreground = isActive ? Brushes.White : fullBrush,
                    FontSize = 11,
                    FontWeight = FontWeight.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            var tagName = tag.Name;
            chip.PointerPressed += (_, _) =>
            {
                if (_activeTagFilters.Contains(tagName))
                    _activeTagFilters.Remove(tagName);
                else
                    _activeTagFilters.Add(tagName);

                FilterClearButton.IsVisible = _activeTagFilters.Count > 0;
                RebuildFilterPanel();
                RebuildAccordion(_expanders.Where(kv => kv.Value.IsExpanded).Select(kv => kv.Key).ToHashSet());
            };

            FilterTagsPanel.Children.Add(chip);
        }
    }

    private void OnFilterClearClick(object? sender, RoutedEventArgs e)
    {
        _activeTagFilters.Clear();
        FilterClearButton.IsVisible = false;
        RebuildFilterPanel();
        RebuildAccordion();
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchQuery = SearchBox.Text ?? string.Empty;
        RebuildAccordion(_expanders.Where(kv => kv.Value.IsExpanded).Select(kv => kv.Key).ToHashSet());
    }

    private void OnRegexToggleChanged(object? sender, RoutedEventArgs e)
    {
        _searchIsRegex = RegexToggleButton.IsChecked == true;
        if (!string.IsNullOrWhiteSpace(_searchQuery))
            RebuildAccordion(_expanders.Where(kv => kv.Value.IsExpanded).Select(kv => kv.Key).ToHashSet());
    }

    private async void OnNoteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is TodoItem item)
        {
            var win = new NoteEditorWindow();
            win.LoadNote(item.Title, item.Note, item.Tags, _settings.GetAllTags());
            var result = await win.ShowDialog<bool?>(this);
            if (result == true)
            {
                item.Note = win.ResultNote;
                item.Tags = win.ResultTags;
                var expanded = _expanders.Where(kv => kv.Value.IsExpanded).Select(kv => kv.Key).ToHashSet();
                RebuildAccordion(expanded);
                Save();
            }
        }
    }

    private async void OnSettingsClick(object? sender, RoutedEventArgs e)
    {
        var win = new SettingsWindow();
        var usedTags = _allItems.SelectMany(i => i.Tags).Distinct();
        win.LoadFrom(_settings, usedTags);
        var result = await win.ShowDialog<bool?>(this);
        if (result == true)
        {
            var oldSavePath = DataService.SavePath;
            _settings = win.Result;
            Strings.SetLanguage(_settings.Language);
            _accordionBuilder.Tags = _settings.GetAllTags();

            // Move todos.json if the data path changed
            DataService.MoveDataFile(oldSavePath, _settings);

            ApplyTheme();
            ApplyLocalization();
            UpdateTitle();
            DataService.SaveSettings(_settings);
            CarryForwardPastTasks();
            // Remove any active tag filters that no longer exist after settings change
            _activeTagFilters.IntersectWith(_settings.GetAllTags().Select(t => t.Name));
            FilterClearButton.IsVisible = _activeTagFilters.Count > 0;
            RebuildFilterPanel();
            RebuildAccordion();
        }
        else
        {
            // Restore language in case user previewed a different one
            Strings.SetLanguage(_settings.Language);
        }
    }

    private async void OnAboutClick(object? sender, RoutedEventArgs e)
    {
        var win = new AboutWindow();
        await win.ShowDialog(this);
    }

    private void UpdateTitle()
    {
        Title = _settings.ShowPathInTitle
            ? $"YATODOL \u2014 {DataService.SavePath}"
            : "YATODOL";
    }

    private void ApplyLocalization()
    {
        HeaderLabel.Text = Strings.AppTitle;

        // Sidebar section labels
        SidebarNavLabel.Text   = Strings.SidebarNavSection;
        SidebarTasksLabel.Text = Strings.SidebarTasksSection;
        SidebarAppLabel.Text   = Strings.SidebarAppSection;

        // Navigation group
        TodayButton.Content      = BuildActionButtonContent("📅", Strings.ButtonToday, "#7e6ea8");
        DeleteDateButton.Content = BuildActionButtonContent("🗑", Strings.ButtonDeleteDate, "#b06060");
        ToolTip.SetTip(DeleteDateButton, Strings.TooltipDeleteDate);

        // Tasks group
        PrintButton.Content  = BuildActionButtonContent("🖨", Strings.ButtonPrint, "#4a9080");
        ImportButton.Content = BuildActionButtonContent("📥", Strings.ButtonImport, "#5b82a8");
        ExportButton.Content = BuildActionButtonContent("📤", Strings.ButtonExport, "#b07840");
        ICalButton.Content   = BuildActionButtonContent("🗓", Strings.ButtonICal, "#5a7080");
        ToolTip.SetTip(ICalButton, Strings.TooltipICal);

        // App group
        SettingsButton.Content = BuildActionButtonContent("⚙", Strings.ButtonSettings, "#5a6472");
        AboutButton.Content    = BuildActionButtonContent("ℹ", Strings.ButtonAbout, "#7e6ea8");
        ToolTip.SetTip(AboutButton, Strings.TooltipAbout);

        AddButton.Content          = Strings.ButtonAdd;
        NewItemBox.PlaceholderText = Strings.PlaceholderNewTask;
        SearchBox.PlaceholderText  = Strings.SearchPlaceholder;
        ToolTip.SetTip(RegexToggleButton, Strings.TooltipRegexToggle);
        SidebarFilterLabel.Text    = Strings.SidebarFilterSection;
        FilterClearButton.Content  = Strings.FilterClearButton;

        ApplyToolbarButtonSizing();
    }

    private static Control BuildActionButtonContent(string icon, string label, string colorHex)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            VerticalAlignment = VerticalAlignment.Center
        };

        panel.Children.Add(BuildIconOnlyBadge(icon, colorHex));
        panel.Children.Add(new TextBlock
        {
            Text = label,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
            FontWeight = FontWeight.SemiBold
        });

        return panel;
    }

    private static Border BuildIconOnlyBadge(string icon, string colorHex)
    {
        return new Border
        {
            Width = 20,
            Height = 20,
            CornerRadius = new CornerRadius(10),
            Background = new SolidColorBrush(Color.Parse(colorHex)),
            Child = new TextBlock
            {
                Text = icon,
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            }
        };
    }

    private void ApplyToolbarButtonSizing()
    {
        var sidebarButtons = new[]
        {
            TodayButton, DeleteDateButton,
            PrintButton, ImportButton, ExportButton, ICalButton,
            SettingsButton, AboutButton
        };
        foreach (var btn in sidebarButtons)
        {
            btn.Padding = new Thickness(10, 8);
            btn.MinHeight = 36;
            btn.HorizontalContentAlignment = HorizontalAlignment.Left;
        }
    }

    private void ApplyTheme()
    {
        if (Application.Current is not { } app) return;
        app.RequestedThemeVariant = _settings.Theme switch
        {
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }

    private static readonly FilePickerFileType JsonFileType = new("JSON files")
    {
        Patterns = ["*.json"]
    };

    private static readonly FilePickerFileType IcsFileType = new("iCalendar files")
    {
        Patterns = ["*.ics"]
    };

    private async void OnICalExportClick(object? sender, RoutedEventArgs e)
    {
        var win = new ICalExportWindow();
        win.LoadItems(_allItems);
        var result = await win.ShowDialog<bool?>(this);
        if (result != true || win.SelectedItems.Count == 0) return;

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export to iCal",
            SuggestedFileName = "todos.ics",
            FileTypeChoices = [IcsFileType]
        });
        if (file is null) return;

        var icalContent = ICalService.GenerateICalString(win.SelectedItems);
        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(icalContent);
    }

    private async void OnExportClick(object? sender, RoutedEventArgs e)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export YATODOL",
            SuggestedFileName = "todos.json",
            FileTypeChoices = [JsonFileType]
        });
        if (file is null) return;

        var json = DataService.SerializeExport(
            new ExportData { Todos = _allItems.ToList(), Settings = _settings });
        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
    }

    private async void OnImportClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import YATODOL",
            AllowMultiple = false,
            FileTypeFilter = [JsonFileType]
        });
        if (files.Count == 0) return;

        try
        {
            await using var stream = await files[0].OpenReadAsync();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var data = DataService.DeserializeImport(json);
            if (data is null) return;

            _allItems.Clear();
            foreach (var item in data.Todos)
                _allItems.Add(item);

            _settings = data.Settings;
            Strings.SetLanguage(_settings.Language);
            ApplyTheme();
            ApplyLocalization();
            UpdateTitle();
            DataService.SaveSettings(_settings);
            CarryForwardPastTasks();
            RebuildAccordion();
            Save();
        }
        catch { /* ignore invalid file */ }
    }

    private void UpdateCount()
    {
        var total = _allItems.Count;
        var remaining = _allItems.Count(i => !i.IsDone);
        var dates = _allItems.Select(i => i.Date.Date).Distinct().Count();
        CountLabel.Text = Strings.CountLabel(remaining, total, dates);
    }

    private void OnPrintClick(object? sender, RoutedEventArgs e)
    {
        var allDates = _settings.PrintScope == PrintScope.AllDates;
        var remainingOnly = _settings.PrintFilter == PrintFilter.RemainingOnly;

        var items = allDates
            ? _allItems.ToList()
            : _allItems.Where(i => i.Date.Date == _selectedDate.Date).ToList();

        if (remainingOnly)
            items = items.Where(i => !i.IsDone).ToList();

        var html = PrintService.GenerateHtml(items, _allItems, _settings, _selectedDate, _settings.GetAllTags());
        var printPath = Path.Combine(Path.GetTempPath(), "yatodol-print.html");
        File.WriteAllText(printPath, html);
        Process.Start(new ProcessStartInfo(printPath) { UseShellExecute = true });
    }
}