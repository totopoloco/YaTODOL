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
    /// All tag definitions (built-in + custom) used to look up colors for chip display.
    /// Set this before calling <see cref="CreateDateExpander"/>.
    /// </summary>
    public IEnumerable<TagDefinition> Tags { get; set; } = [];

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

        // Content panel: title text + optional tag chips below
        var contentPanel = new StackPanel
        {
            Spacing = 3,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 2, 8, 2)
        };

        var tb = new TextBlock
        {
            Text = item.Title,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextDecorations = item.IsDone ? TextDecorations.Strikethrough : null,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        ToolTip.SetTip(tb, Strings.TooltipRename);
        tb.DoubleTapped += (_, _) => BeginInlineEdit(tb, item, row, contentPanel);
        cb.Click += (_, _) => tb.TextDecorations = item.IsDone ? TextDecorations.Strikethrough : null;

        contentPanel.Children.Add(tb);

        if (item.Tags.Count > 0)
            contentPanel.Children.Add(BuildTagChips(item.Tags));

        Grid.SetColumn(contentPanel, 2);

        var noteBtn = new Button
        {
            Content = new Border
            {
                Width = 24,
                Height = 24,
                CornerRadius = new CornerRadius(12),
                Background = item.HasNote
                    ? new SolidColorBrush(Color.Parse("#4a9080"))
                    : new SolidColorBrush(Color.Parse("#b07840")),
                Child = new TextBlock
                {
                    Text = item.HasNote ? "✎" : "+",
                    Foreground = Brushes.White,
                    FontSize = 14,
                    FontWeight = FontWeight.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                }
            },
            DataContext = item,
            VerticalAlignment = VerticalAlignment.Center,
            Background = Brushes.Transparent,
            FontSize = 14,
            Padding = new Thickness(4, 2)
        };
        ToolTip.SetTip(noteBtn, item.HasNote ? Strings.TooltipEditNote : Strings.TooltipAddNote);
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
        row.Children.Add(contentPanel);
        row.Children.Add(noteBtn);
        row.Children.Add(del);
        return row;
    }

    /// <summary>Builds a WrapPanel of colored tag chip badges for the given tag keys.</summary>
    private WrapPanel BuildTagChips(List<string> tags)
    {
        var wrap = new WrapPanel { Margin = new Thickness(0, 1, 0, 1) };
        foreach (var tagName in tags)
        {
            var brush = GetTagBrush(tagName);
            wrap.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = brush,
                Padding = new Thickness(6, 2),
                Margin = new Thickness(0, 0, 4, 2),
                Child = new TextBlock
                {
                    Text = Strings.GetTagDisplayName(tagName),
                    Foreground = Brushes.White,
                    FontSize = 10,
                    FontWeight = FontWeight.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
        }
        return wrap;
    }

    private IBrush GetTagBrush(string tagName)
    {
        var def = Tags.FirstOrDefault(t => t.Name == tagName)
               ?? BuiltInTags.All.FirstOrDefault(t => t.Name == tagName);
        return new SolidColorBrush(Color.Parse(def?.Color ?? "#888888"));
    }

    private void BeginInlineEdit(TextBlock tb, TodoItem item, Grid row, StackPanel contentPanel)
    {
        var editBox = new TextBox
        {
            Text = item.Title,
            FontSize = tb.FontSize,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = contentPanel.Margin,
            Padding = new Thickness(4, 2)
        };
        Grid.SetColumn(editBox, 2);

        row.Children.Remove(contentPanel);
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
            row.Children.Add(contentPanel);

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
                row.Children.Add(contentPanel);
            }
        };

        editBox.LostFocus += (_, _) => Commit();
    }
}
