using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using YATODOL.Models;
using YATODOL.Utilities;

namespace YATODOL.Services;

/// <summary>
/// Builds the accordion UI of date-grouped expander panels containing to-do item rows
/// with checkboxes, note buttons, delete buttons, and drag-reorder grips.
/// </summary>
public class AccordionBuilder
{
    private readonly Action<object?, RoutedEventArgs> _onCheckChanged;
    private readonly Action<object?, RoutedEventArgs> _onDeleteClick;
    private readonly Action<object?, RoutedEventArgs> _onNoteClick;
    private readonly Action<TodoItem, string> _onRenameComplete;
    private readonly DragReorderService _dragService;

    /// <summary>
    /// Initializes a new <see cref="AccordionBuilder"/> with the specified event callbacks.
    /// </summary>
    /// <param name="onCheckChanged">Handler invoked when a task's checkbox is toggled.</param>
    /// <param name="onDeleteClick">Handler invoked when a task's delete button is clicked.</param>
    /// <param name="onNoteClick">Handler invoked when a task's note button is clicked.</param>
    /// <param name="onReorder">Callback receiving the reordered item and its new index.</param>
    /// <param name="onRenameComplete">Callback invoked when a task is renamed, receiving the item and new title.</param>
    public AccordionBuilder(
        Action<object?, RoutedEventArgs> onCheckChanged,
        Action<object?, RoutedEventArgs> onDeleteClick,
        Action<object?, RoutedEventArgs> onNoteClick,
        Action<TodoItem, int> onReorder,
        Action<TodoItem, string> onRenameComplete)
    {
        _onCheckChanged = onCheckChanged;
        _onDeleteClick = onDeleteClick;
        _onNoteClick = onNoteClick;
        _onRenameComplete = onRenameComplete;
        _dragService = new DragReorderService(onReorder);
    }

    /// <summary>
    /// Creates an <see cref="Expander"/> control for a single date group containing all its to-do items.
    /// </summary>
    /// <param name="date">The date for this group.</param>
    /// <param name="items">The to-do items assigned to this date.</param>
    /// <param name="isExpanded">Whether the expander should be initially expanded.</param>
    /// <returns>A configured <see cref="Expander"/> control.</returns>
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
            ColumnDefinitions = ColumnDefinitions.Parse("Auto,Auto,*,Auto,Auto"),
            DataContext = item,
            Opacity = item.IsDone ? 0.5 : 1.0
        };

        var grip = new Border
        {
            Child = new TextBlock
            {
                Text = "\u2807",
                FontSize = 16,
                Foreground = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center
            },
            Cursor = new Cursor(StandardCursorType.Hand),
            Padding = new Thickness(4, 0),
            Background = Brushes.Transparent
        };
        ToolTip.SetTip(grip, Strings.TooltipDragReorder);
        _dragService.AttachGrip(grip, row);
        Grid.SetColumn(grip, 0);

        var cb = new CheckBox
        {
            [!CheckBox.IsCheckedProperty] = new Binding("IsDone") { Mode = BindingMode.TwoWay },
            DataContext = item,
            VerticalAlignment = VerticalAlignment.Center
        };
        cb.Click += (s, e) => _onCheckChanged(s, e);
        cb.Click += (_, _) => row.Opacity = item.IsDone ? 0.5 : 1.0;
        Grid.SetColumn(cb, 1);

        var tb = new TextBlock
        {
            Text = item.Title,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0),
            TextWrapping = TextWrapping.Wrap,
            TextDecorations = item.IsDone ? TextDecorations.Strikethrough : null,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        ToolTip.SetTip(tb, Strings.TooltipRename);
        tb.DoubleTapped += (_, _) => BeginInlineEdit(tb, item, row);
        cb.Click += (_, _) => tb.TextDecorations = item.IsDone ? TextDecorations.Strikethrough : null;
        Grid.SetColumn(tb, 2);

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
        Grid.SetColumn(noteBtn, 3);

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
        Grid.SetColumn(del, 4);

        row.Children.Add(grip);
        row.Children.Add(cb);
        row.Children.Add(tb);
        row.Children.Add(noteBtn);
        row.Children.Add(del);
        return row;
    }

    private void BeginInlineEdit(TextBlock tb, TodoItem item, Grid row)
    {
        var editBox = new TextBox
        {
            Text = item.Title,
            FontSize = tb.FontSize,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = tb.Margin,
            Padding = new Thickness(4, 2)
        };
        Grid.SetColumn(editBox, Grid.GetColumn(tb));

        row.Children.Remove(tb);
        row.Children.Add(editBox);
        editBox.Focus();
        editBox.SelectAll();

        var committed = false;

        void Commit()
        {
            if (committed) return;
            committed = true;

            var newTitle = editBox.Text?.Trim();
            row.Children.Remove(editBox);
            row.Children.Add(tb);

            if (!string.IsNullOrEmpty(newTitle) && newTitle != item.Title)
            {
                tb.Text = newTitle;
                _onRenameComplete(item, newTitle);
            }
        }

        editBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                Commit();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                committed = true;
                row.Children.Remove(editBox);
                row.Children.Add(tb);
            }
        };

        editBox.LostFocus += (_, _) => Commit();
    }
}
