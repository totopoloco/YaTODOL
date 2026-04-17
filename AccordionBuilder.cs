using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace YATODOL;

public class AccordionBuilder
{
    private readonly Action<object?, RoutedEventArgs> _onCheckChanged;
    private readonly Action<object?, RoutedEventArgs> _onDeleteClick;
    private readonly Action<object?, RoutedEventArgs> _onNoteClick;

    public AccordionBuilder(
        Action<object?, RoutedEventArgs> onCheckChanged,
        Action<object?, RoutedEventArgs> onDeleteClick,
        Action<object?, RoutedEventArgs> onNoteClick)
    {
        _onCheckChanged = onCheckChanged;
        _onDeleteClick = onDeleteClick;
        _onNoteClick = onNoteClick;
    }

    public Expander CreateDateExpander(DateTime date, List<TodoItem> items, bool isExpanded)
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

        return expander;
    }

    private static Control CreateDateHeader(DateTime date, int remaining, int total)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        panel.Children.Add(new TextBlock
        {
            Text = date == DateTime.Today ? $"{Strings.TodayPrefix}{date:ddd, MMM d}" : date.ToString("ddd, MMM d, yyyy"),
            FontWeight = FontWeight.SemiBold,
            FontSize = 14
        });
        panel.Children.Add(new TextBlock
        {
            Text = Strings.RemainingOf(remaining, total),
            Foreground = Brushes.Gray,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        });
        return panel;
    }

    private StackPanel CreateItemsPanel(List<TodoItem> items)
    {
        var panel = new StackPanel { Spacing = 2 };
        foreach (var item in items)
            panel.Children.Add(CreateTodoRow(item));
        return panel;
    }

    private Grid CreateTodoRow(TodoItem item)
    {
        var row = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto,Auto"),
            DataContext = item,
            Opacity = item.IsDone ? 0.5 : 1.0
        };

        var cb = new CheckBox
        {
            [!CheckBox.IsCheckedProperty] = new Binding("IsDone") { Mode = BindingMode.TwoWay },
            DataContext = item,
            VerticalAlignment = VerticalAlignment.Center
        };
        cb.Click += (s, e) => _onCheckChanged(s, e);
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

        var noteBtn = new Button
        {
            Content = item.HasNote ? "\U0001f4dd" : "\U0001f4cb",
            DataContext = item,
            VerticalAlignment = VerticalAlignment.Center,
            Background = Brushes.Transparent,
            FontSize = 13,
            Padding = new Thickness(4, 2)
        };
        ToolTip.SetTip(noteBtn, Strings.TooltipNote);
        noteBtn.Click += (s, e) => _onNoteClick(s, e);
        Grid.SetColumn(noteBtn, 2);

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
        del.Click += (s, e) => _onDeleteClick(s, e);
        Grid.SetColumn(del, 3);

        row.Children.Add(cb);
        row.Children.Add(tb);
        row.Children.Add(noteBtn);
        row.Children.Add(del);
        return row;
    }
}
