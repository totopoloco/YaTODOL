using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace YATODOL;

public partial class ICalExportWindow : Window
{
    private readonly List<(CheckBox Check, TodoItem Item)> _rows = [];

    public List<TodoItem> SelectedItems { get; private set; } = [];

    public ICalExportWindow()
    {
        InitializeComponent();
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        Title = Strings.ICalWindowTitle;
        ICalTitleText.Text = Strings.ICalTitle;
        ICalSubtitleText.Text = Strings.ICalSubtitle;
        ICalCancelButton.Content = Strings.ButtonCancel;
        ICalExportButton.Content = Strings.ButtonExport;
        SelectAllButton.Content = Strings.ButtonSelectAll;
        SelectNoneButton.Content = Strings.ButtonSelectNone;
        UncompletedOnlyButton.Content = Strings.ButtonUncompletedOnly;
    }

    public void LoadItems(IEnumerable<TodoItem> items)
    {
        _rows.Clear();
        TaskListPanel.Children.Clear();

        foreach (var group in items.OrderBy(i => i.Date).GroupBy(i => i.Date.Date))
        {
            // Date header
            var header = new TextBlock
            {
                Text = group.Key == DateTime.Today
                    ? $"{Strings.TodayPrefix}{group.Key:ddd, MMM d}"
                    : group.Key.ToString("ddd, MMM d, yyyy"),
                FontWeight = FontWeight.SemiBold,
                FontSize = 13,
                Margin = new Avalonia.Thickness(8, 8, 0, 2),
                Foreground = Brushes.Gray
            };
            TaskListPanel.Children.Add(header);

            foreach (var item in group.OrderBy(i => i.IsDone))
            {
                var cb = new CheckBox
                {
                    IsChecked = !item.IsDone,
                    Margin = new Avalonia.Thickness(4, 1)
                };

                var title = new TextBlock
                {
                    Text = item.Title,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = item.IsDone ? 0.5 : 1.0,
                    TextDecorations = item.IsDone ? TextDecorations.Strikethrough : null
                };

                var status = new TextBlock
                {
                    Text = item.IsDone ? "✓" : "",
                    Foreground = Brushes.Green,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(4, 0)
                };

                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 4
                };
                row.Children.Add(cb);
                row.Children.Add(status);
                row.Children.Add(title);

                TaskListPanel.Children.Add(row);
                _rows.Add((cb, item));
            }
        }
    }

    private void OnSelectAll(object? sender, RoutedEventArgs e)
    {
        foreach (var (cb, _) in _rows) cb.IsChecked = true;
    }

    private void OnSelectNone(object? sender, RoutedEventArgs e)
    {
        foreach (var (cb, _) in _rows) cb.IsChecked = false;
    }

    private void OnSelectUncompleted(object? sender, RoutedEventArgs e)
    {
        foreach (var (cb, item) in _rows) cb.IsChecked = !item.IsDone;
    }

    private void OnExportClick(object? sender, RoutedEventArgs e)
    {
        SelectedItems = _rows
            .Where(r => r.Check.IsChecked == true)
            .Select(r => r.Item)
            .ToList();
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
