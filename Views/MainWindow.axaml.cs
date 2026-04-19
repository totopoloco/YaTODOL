using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

    public MainWindow()
    {
        InitializeComponent();
        _accordionBuilder = new AccordionBuilder(OnCheckChanged, OnDeleteClick, OnNoteClick, OnReorder, OnRenameComplete);
        DatePicker.SelectedDate = _selectedDate;
        DatePicker.SelectedDateChanged += OnDateChanged;
        _settings = DataService.LoadSettings();
        DataService.ApplyCustomPath(_settings);
        Strings.SetLanguage(_settings.Language);
        ApplyTheme();
        ApplyLocalization();
        UpdateTitle();
        foreach (var item in DataService.LoadTodos())
            _allItems.Add(item);
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

        var groups = _allItems.OrderByDescending(i => i.Date).GroupBy(i => i.Date.Date);

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

    private async void OnNoteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is TodoItem item)
        {
            var win = new NoteEditorWindow();
            win.LoadNote(item.Title, item.Note);
            var result = await win.ShowDialog<bool?>(this);
            if (result == true)
            {
                item.Note = win.ResultNote;
                var expanded = _expanders.Where(kv => kv.Value.IsExpanded).Select(kv => kv.Key).ToHashSet();
                RebuildAccordion(expanded);
                Save();
            }
        }
    }

    private async void OnSettingsClick(object? sender, RoutedEventArgs e)
    {
        var win = new SettingsWindow();
        win.LoadFrom(_settings);
        var result = await win.ShowDialog<bool?>(this);
        if (result == true)
        {
            var oldSavePath = DataService.SavePath;
            _settings = win.Result;
            Strings.SetLanguage(_settings.Language);

            // Move todos.json if the data path changed
            DataService.MoveDataFile(oldSavePath, _settings);

            ApplyTheme();
            ApplyLocalization();
            UpdateTitle();
            DataService.SaveSettings(_settings);
            CarryForwardPastTasks();
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
        TodayButton.Content = Strings.ButtonToday;
        ToolTip.SetTip(DeleteDateButton, Strings.TooltipDeleteDate);
        PrintButton.Content = Strings.ButtonPrint;
        ImportButton.Content = Strings.ButtonImport;
        ExportButton.Content = Strings.ButtonExport;
        ToolTip.SetTip(ICalButton, Strings.TooltipICal);
        SettingsButton.Content = Strings.ButtonSettings;
        ToolTip.SetTip(AboutButton, Strings.TooltipAbout);
        AddButton.Content = Strings.ButtonAdd;
        NewItemBox.PlaceholderText = Strings.PlaceholderNewTask;
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

        var html = PrintService.GenerateHtml(items, _allItems, _settings, _selectedDate);
        var printPath = Path.Combine(Path.GetTempPath(), "yatodol-print.html");
        File.WriteAllText(printPath, html);
        Process.Start(new ProcessStartInfo(printPath) { UseShellExecute = true });
    }
}