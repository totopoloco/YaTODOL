using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using YATODOL.Models;

namespace YATODOL.Services;

/// <summary>
/// Handles pointer-based drag-and-drop reordering of to-do item rows within a <see cref="Avalonia.Controls.StackPanel"/>.
/// Uses <see cref="Avalonia.Media.TranslateTransform"/> to provide visual displacement feedback during the drag.
/// </summary>
public class DragReorderService
{
    private readonly Action<TodoItem, int> _onReorder;

    private bool _isDragging;
    private StackPanel? _dragPanel;
    private Grid? _dragRow;
    private int _dragOrigIndex;
    private int _dragTargetIndex;
    private double _pointerStartY;
    private TranslateTransform? _dragTransform;

    /// <summary>
    /// Initializes a new <see cref="DragReorderService"/> with the specified reorder callback.
    /// </summary>
    /// <param name="onReorder">Callback invoked when a drag completes, receiving the moved item and its new index.</param>
    public DragReorderService(Action<TodoItem, int> onReorder)
    {
        _onReorder = onReorder;
    }

    /// <summary>
    /// Attaches drag event handlers to a grip control for reordering its parent row.
    /// </summary>
    /// <param name="grip">The grip <see cref="Border"/> the user drags.</param>
    /// <param name="row">The <see cref="Grid"/> row to be reordered.</param>
    public void AttachGrip(Border grip, Grid row)
    {
        grip.PointerPressed += (s, e) =>
        {
            if (!e.GetCurrentPoint(grip).Properties.IsLeftButtonPressed) return;
            e.Pointer.Capture(grip);
            StartDrag(row, e);
            e.Handled = true;
        };

        grip.PointerMoved += (s, e) =>
        {
            if (_isDragging && _dragRow == row)
            {
                UpdateDrag(e);
                e.Handled = true;
            }
        };

        grip.PointerReleased += (s, e) =>
        {
            if (_isDragging && _dragRow == row)
            {
                EndDrag();
                e.Pointer.Capture(null);
                e.Handled = true;
            }
        };

        grip.PointerCaptureLost += (s, e) =>
        {
            if (_isDragging && _dragRow == row)
                CancelDrag();
        };
    }

    private void StartDrag(Grid row, PointerPressedEventArgs e)
    {
        _dragPanel = row.Parent as StackPanel;
        if (_dragPanel == null) return;

        _isDragging = true;
        _dragRow = row;
        _dragOrigIndex = _dragPanel.Children.IndexOf(row);
        _dragTargetIndex = _dragOrigIndex;
        _pointerStartY = e.GetPosition(_dragPanel).Y;

        _dragTransform = new TranslateTransform();
        row.RenderTransform = _dragTransform;
        row.ZIndex = 100;
        row.Opacity = 0.85;
    }

    private void UpdateDrag(PointerEventArgs e)
    {
        if (_dragPanel == null || _dragRow == null || _dragTransform == null) return;

        var pos = e.GetPosition(_dragPanel);
        _dragTransform.Y = pos.Y - _pointerStartY;

        var dragVisualCenter = _dragRow.Bounds.Y + _dragTransform.Y + _dragRow.Bounds.Height / 2;

        int newTarget = 0;
        for (int i = 0; i < _dragPanel.Children.Count; i++)
        {
            if (i == _dragOrigIndex) continue;
            var child = _dragPanel.Children[i];
            if (child.Bounds.Y + child.Bounds.Height / 2 < dragVisualCenter)
                newTarget++;
        }

        if (newTarget == _dragTargetIndex) return;
        _dragTargetIndex = newTarget;

        double shiftAmount = _dragRow.Bounds.Height + _dragPanel.Spacing;

        for (int i = 0; i < _dragPanel.Children.Count; i++)
        {
            if (i == _dragOrigIndex) continue;

            double shift = 0;
            if (_dragTargetIndex < _dragOrigIndex && i >= _dragTargetIndex && i < _dragOrigIndex)
                shift = shiftAmount;
            else if (_dragTargetIndex > _dragOrigIndex && i > _dragOrigIndex && i <= _dragTargetIndex)
                shift = -shiftAmount;

            var child = _dragPanel.Children[i];
            if (child.RenderTransform is TranslateTransform tt)
                tt.Y = shift;
            else if (shift != 0)
                child.RenderTransform = new TranslateTransform(0, shift);
            else
                child.RenderTransform = null;
        }
    }

    private void EndDrag()
    {
        if (_dragRow == null || _dragPanel == null) return;

        var item = _dragRow.DataContext as TodoItem;
        var finalIndex = _dragTargetIndex;

        ClearAllTransforms();
        ResetState();

        if (item != null)
            _onReorder(item, finalIndex);
    }

    private void CancelDrag()
    {
        ClearAllTransforms();
        ResetState();
    }

    private void ResetState()
    {
        _isDragging = false;
        _dragRow = null;
        _dragPanel = null;
        _dragTransform = null;
    }

    private void ClearAllTransforms()
    {
        if (_dragPanel == null) return;
        foreach (var child in _dragPanel.Children)
        {
            child.RenderTransform = null;
            if (child is Grid g)
            {
                g.ZIndex = 0;
                var todoItem = g.DataContext as TodoItem;
                g.Opacity = todoItem?.IsDone == true ? 0.5 : 1.0;
            }
        }
    }
}
