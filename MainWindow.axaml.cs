using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;

namespace YATODOL;

public partial class MainWindow : Window
{
    private static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YATODOL", "todos.json");

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "YATODOL", "settings.json");

    private readonly ObservableCollection<TodoItem> _allItems = new();
    private readonly Dictionary<DateTime, Expander> _expanders = new();
    private DateTime _selectedDate = DateTime.Today;
    private Timer? _midnightTimer;

    private AppSettings _settings = new();

    public MainWindow()
    {
        InitializeComponent();
        DatePicker.SelectedDate = _selectedDate;
        DatePicker.SelectedDateChanged += OnDateChanged;
        LoadSettings();
        ApplyTheme();
        UpdateTitle();
        Load();
        CarryForwardPastTasks();
        RebuildAccordion();
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
        foreach (var item in _allItems.Where(i => i.Date.Date < today && !i.IsDone))
        {
            item.Date = today;
            moved = true;
        }

        if (moved) Save();
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
            NewItemBox.PlaceholderText = "Cannot add tasks in the past!";
            NewItemBox.Text = string.Empty;
            return;
        }

        var item = new TodoItem { Title = text, Date = _selectedDate };
        _allItems.Add(item);
        NewItemBox.Text = string.Empty;
        NewItemBox.PlaceholderText = "Add a new task...";
        RebuildAccordion();
        Save();
        NewItemBox.Focus();
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

        var box = new Avalonia.Controls.Window
        {
            Title = "Confirm Delete",
            Width = 400, Height = 160,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = $"Delete all {items.Count} task(s) for {_selectedDate:ddd, MMM d, yyyy}?\nThis includes completed and uncompleted items.",
                        TextWrapping = TextWrapping.Wrap
                    },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Spacing = 8,
                        Children =
                        {
                            new Button { Content = "Cancel", Width = 80, Tag = false },
                            new Button { Content = "Delete", Width = 80, Tag = true, Classes = { "accent" } }
                        }
                    }
                }
            }
        };

        var buttons = ((StackPanel)((StackPanel)box.Content).Children[1]).Children;
        foreach (Button btn in buttons)
            btn.Click += (_, _) => box.Close(btn.Tag);

        var result = await box.ShowDialog<object?>(this);
        if (result is true)
        {
            foreach (var item in items)
                _allItems.Remove(item);
            RebuildAccordion();
            Save();
        }
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

        var groups = _allItems.OrderByDescending(i => i.Date).GroupBy(i => i.Date.Date);

        foreach (var group in groups.Where(g =>
            !_settings.HideCompletedDates || g.Any(i => !i.IsDone)))
        {
            var date = group.Key;
            var isExpanded = expandedDates?.Contains(date) ?? date == _selectedDate.Date;
            var expander = CreateDateExpander(date, group.ToList(), isExpanded);
            AccordionPanel.Children.Add(expander);
            _expanders[date] = expander;
        }

        UpdateCount();
    }

    private Expander CreateDateExpander(DateTime date, List<TodoItem> items, bool isExpanded)
    {
        var remaining = items.Count(i => !i.IsDone);

        var expander = new Expander
        {
            IsExpanded = isExpanded,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 2),
            Header = CreateDateHeader(date, remaining, items.Count),
            Content = CreateItemsPanel(items),
            Tag = date
        };

        expander.PropertyChanged += (s, e) =>
        {
            if (e.Property == Expander.IsExpandedProperty && expander.IsExpanded && expander.Tag is DateTime d)
            {
                _selectedDate = d;
                DatePicker.SelectedDate = d;
            }
        };

        return expander;
    }

    private static Control CreateDateHeader(DateTime date, int remaining, int total)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        panel.Children.Add(new TextBlock
        {
            Text = date == DateTime.Today ? $"Today \u2014 {date:ddd, MMM d}" : date.ToString("ddd, MMM d, yyyy"),
            FontWeight = FontWeight.SemiBold,
            FontSize = 14
        });
        panel.Children.Add(new TextBlock
        {
            Text = $"({remaining} remaining of {total})",
            Foreground = Brushes.Gray,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        });
        return panel;
    }

    private StackPanel CreateItemsPanel(List<TodoItem> items)
    {
        var panel = new StackPanel { Spacing = 2 };
        foreach (var item in items.OrderBy(i => i.IsDone))
            panel.Children.Add(CreateTodoRow(item));
        return panel;
    }

    private Grid CreateTodoRow(TodoItem item)
    {
        var row = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto"),
            DataContext = item,
            Opacity = item.IsDone ? 0.5 : 1.0
        };

        var cb = new CheckBox
        {
            [!CheckBox.IsCheckedProperty] = new Binding("IsDone") { Mode = BindingMode.TwoWay },
            DataContext = item,
            VerticalAlignment = VerticalAlignment.Center
        };
        cb.Click += OnCheckChanged;
        cb.Click += (_, _) => row.Opacity = item.IsDone ? 0.5 : 1.0;
        Grid.SetColumn(cb, 0);

        var tb = new TextBlock
        {
            Text = item.Title,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0),
            TextWrapping = TextWrapping.Wrap,
            TextDecorations = item.IsDone ? TextDecorations.Strikethrough : null
        };
        cb.Click += (_, _) => tb.TextDecorations = item.IsDone ? TextDecorations.Strikethrough : null;
        Grid.SetColumn(tb, 1);

        var del = new Button
        {
            Content = "\u2715",
            DataContext = item,
            VerticalAlignment = VerticalAlignment.Center,
            Background = Brushes.Transparent,
            Foreground = Brushes.Gray,
            FontSize = 14,
            Padding = new Thickness(6, 2)
        };
        del.Click += OnDeleteClick;
        Grid.SetColumn(del, 2);

        row.Children.Add(cb);
        row.Children.Add(tb);
        row.Children.Add(del);
        return row;
    }

    private async void OnSettingsClick(object? sender, RoutedEventArgs e)
    {
        var win = new SettingsWindow();
        win.LoadFrom(_settings);
        var result = await win.ShowDialog<bool?>(this);
        if (result == true)
        {
            _settings = win.Result;
            ApplyTheme();
            UpdateTitle();
            SaveSettings();
            CarryForwardPastTasks();
            RebuildAccordion();
        }
    }

    private void UpdateTitle()
    {
        Title = _settings.ShowPathInTitle
            ? $"YATODOL \u2014 {SavePath}"
            : "YATODOL";
    }

    private void LoadSettings()
    {
        if (!File.Exists(SettingsPath)) return;
        try
        {
            var json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            if (settings is not null)
                _settings = settings;
        }
        catch { /* ignore corrupt file */ }
    }

    private void SaveSettings()
    {
        var dir = Path.GetDirectoryName(SettingsPath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(_settings);
        File.WriteAllText(SettingsPath, json);
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

        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//YATODOL//Yet Another To Do List//EN");
        sb.AppendLine("CALSCALE:GREGORIAN");
        sb.AppendLine("METHOD:PUBLISH");

        foreach (var item in win.SelectedItems)
        {
            var uid = Guid.NewGuid().ToString();
            var dateStr = item.Date.ToString("yyyyMMdd");
            var nextDateStr = item.Date.AddDays(1).ToString("yyyyMMdd");
            var nowStr = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");
            var prefix = item.IsDone ? "✓ " : "☐ ";

            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{uid}");
            sb.AppendLine($"DTSTAMP:{nowStr}");
            sb.AppendLine($"DTSTART;VALUE=DATE:{dateStr}");
            sb.AppendLine($"DTEND;VALUE=DATE:{nextDateStr}");
            sb.AppendLine($"SUMMARY:{EscapeICalText(prefix + item.Title)}");
            sb.AppendLine("TRANSP:TRANSPARENT");
            sb.AppendLine("END:VEVENT");
        }

        sb.AppendLine("END:VCALENDAR");

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        await writer.WriteAsync(sb.ToString());
    }

    private static string EscapeICalText(string text) =>
        text.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\n", "\\n");

    private async void OnExportClick(object? sender, RoutedEventArgs e)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export YATODOL",
            SuggestedFileName = "todos.json",
            FileTypeChoices = [JsonFileType]
        });
        if (file is null) return;

        var json = JsonSerializer.Serialize(
            new ExportData { Todos = _allItems.ToList(), Settings = _settings },
            new JsonSerializerOptions { WriteIndented = true });
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
            var data = JsonSerializer.Deserialize<ExportData>(json);
            if (data is null) return;

            _allItems.Clear();
            foreach (var item in data.Todos)
                _allItems.Add(item);

            _settings = data.Settings;
            ApplyTheme();
            UpdateTitle();
            SaveSettings();
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
        CountLabel.Text = $"{remaining} remaining of {total} across {dates} date{(dates != 1 ? "s" : "")}";
    }

    private void OnPrintClick(object? sender, RoutedEventArgs e)
    {
        var allDates = _settings.PrintScope == PrintScope.AllDates;
        var remainingOnly = _settings.PrintFilter == PrintFilter.RemainingOnly;

        // Gather items based on scope
        var items = allDates
            ? _allItems.ToList()
            : _allItems.Where(i => i.Date.Date == _selectedDate.Date).ToList();

        // Apply filter
        if (remainingOnly)
            items = items.Where(i => !i.IsDone).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset='utf-8'>");
        sb.AppendLine("<title>YATODOL</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("""
            @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap');
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body { font-family: 'Inter', sans-serif; max-width: 640px; margin: 40px auto;
                   padding: 0 24px; color: #1a1a2e; }
            h1 { font-size: 28px; font-weight: 700; margin-bottom: 4px; }
            .subtitle { color: #888; font-size: 14px; margin-bottom: 32px; }
            .day-header { font-size: 16px; font-weight: 700; color: #1a1a2e; margin: 28px 0 8px;
                          padding-bottom: 6px; border-bottom: 2px solid #4a6cf7; }
            .day-header:first-of-type { margin-top: 0; }
            .section { margin-bottom: 16px; }
            .section-title { font-size: 12px; font-weight: 600; text-transform: uppercase;
                             letter-spacing: 1.2px; color: #555; margin: 12px 0 8px;
                             padding-bottom: 4px; border-bottom: 1px solid #e0e0e0; }
            ul { list-style: none; }
            li { padding: 8px 0; border-bottom: 1px solid #f0f0f0; display: flex;
                 align-items: center; gap: 12px; font-size: 15px; }
            li:last-child { border-bottom: none; }
            .checkbox { width: 18px; height: 18px; border: 2px solid #ccc; border-radius: 4px;
                        flex-shrink: 0; display: flex; align-items: center; justify-content: center; }
            .done .checkbox { border-color: #4caf50; background: #4caf50; }
            .done .checkbox::after { content: '\2713'; color: white; font-size: 12px; font-weight: 700; }
            .done .text { color: #999; text-decoration: line-through; }
            .summary { margin-top: 32px; padding: 16px; border-top: 2px solid #e0e0e0;
                       font-size: 14px; color: #666; display: flex; gap: 24px; }
            .summary span { font-weight: 600; color: #333; }
            .empty { color: #aaa; font-style: italic; padding: 12px 0; }
            @media print { body { margin: 20px auto; } }
            """);
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h1>\u2611 YATODOL</h1>");

        var subtitle = allDates ? "All dates" : _selectedDate.ToString("MMMM d, yyyy");
        if (remainingOnly) subtitle += " \u2014 remaining only";
        sb.AppendLine($"<p class='subtitle'>{WebUtility.HtmlEncode(subtitle)}</p>");

        if (items.Count == 0)
        {
            sb.AppendLine("<p class='empty'>No tasks to display.</p>");
        }
        else if (allDates)
        {
            // Group by date
            var groups = items.OrderBy(i => i.Date).ThenBy(i => i.IsDone).GroupBy(i => i.Date.Date);
            foreach (var group in groups)
            {
                sb.AppendLine($"<div class='day-header'>{WebUtility.HtmlEncode(group.Key.ToString("dddd, MMMM d, yyyy"))}</div>");
                RenderItemList(sb, group.ToList(), remainingOnly);
            }
        }
        else
        {
            RenderItemList(sb, items, remainingOnly);
        }

        // Summary
        var totalAll = allDates ? _allItems.Count : _allItems.Count(i => i.Date.Date == _selectedDate.Date);
        var activeAll = allDates ? _allItems.Count(i => !i.IsDone) : _allItems.Count(i => i.Date.Date == _selectedDate.Date && !i.IsDone);
        var doneAll = totalAll - activeAll;
        sb.AppendLine($"<div class='summary'>" +
            $"<div>Total: <span>{totalAll}</span></div>" +
            $"<div>Active: <span>{activeAll}</span></div>" +
            $"<div>Completed: <span>{doneAll}</span></div></div>");
        sb.AppendLine("</body></html>");

        var printPath = Path.Combine(Path.GetTempPath(), "yatodol-print.html");
        File.WriteAllText(printPath, sb.ToString());
        Process.Start(new ProcessStartInfo(printPath) { UseShellExecute = true });
    }

    private static void RenderItemList(StringBuilder sb, List<TodoItem> items, bool remainingOnly)
    {
        var active = items.Where(i => !i.IsDone).ToList();
        var done = items.Where(i => i.IsDone).ToList();

        if (active.Count > 0)
        {
            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<div class='section-title'>Active</div><ul>");
            foreach (var item in active)
                sb.AppendLine($"<li><div class='checkbox'></div><span class='text'>{WebUtility.HtmlEncode(item.Title)}</span></li>");
            sb.AppendLine("</ul></div>");
        }

        if (!remainingOnly && done.Count > 0)
        {
            sb.AppendLine("<div class='section'>");
            sb.AppendLine("<div class='section-title'>Completed</div><ul>");
            foreach (var item in done)
                sb.AppendLine($"<li class='done'><div class='checkbox'></div><span class='text'>{WebUtility.HtmlEncode(item.Title)}</span></li>");
            sb.AppendLine("</ul></div>");
        }
    }

    private void Save()
    {
        var dir = Path.GetDirectoryName(SavePath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(_allItems.ToList());
        File.WriteAllText(SavePath, json);
    }

    private void Load()
    {
        if (!File.Exists(SavePath)) return;
        try
        {
            var json = File.ReadAllText(SavePath);
            var items = JsonSerializer.Deserialize<List<TodoItem>>(json);
            if (items is null) return;
            foreach (var item in items)
                _allItems.Add(item);
        }
        catch { /* ignore corrupt file */ }
    }
}