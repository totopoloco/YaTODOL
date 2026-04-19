using System;
using System.ComponentModel;

namespace YATODOL.Models;

/// <summary>
/// Represents a single to-do task with a title, completion status, date, optional note, and sort order.
/// Implements <see cref="INotifyPropertyChanged"/> for Avalonia data binding.
/// </summary>
public class TodoItem : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private bool _isDone;
    private DateTime _date = DateTime.Today;
    private string? _note;
    private int _sortOrder;

    /// <summary>
    /// Gets or sets the task title text.
    /// </summary>
    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(nameof(Title)); }
    }

    /// <summary>
    /// Gets or sets whether the task is completed.
    /// </summary>
    public bool IsDone
    {
        get => _isDone;
        set { _isDone = value; OnPropertyChanged(nameof(IsDone)); }
    }

    /// <summary>
    /// Gets or sets the date this task is assigned to.
    /// </summary>
    public DateTime Date
    {
        get => _date;
        set { _date = value; OnPropertyChanged(nameof(Date)); }
    }

    /// <summary>
    /// Gets or sets an optional note attached to the task.
    /// Setting this also raises <see cref="PropertyChanged"/> for <see cref="HasNote"/>.
    /// </summary>
    public string? Note
    {
        get => _note;
        set { _note = value; OnPropertyChanged(nameof(Note)); OnPropertyChanged(nameof(HasNote)); }
    }

    /// <summary>
    /// Gets or sets the sort order for manual drag-and-drop reordering within a date group.
    /// </summary>
    public int SortOrder
    {
        get => _sortOrder;
        set { _sortOrder = value; OnPropertyChanged(nameof(SortOrder)); }
    }

    /// <summary>
    /// Gets a value indicating whether the task has a non-empty note.
    /// </summary>
    public bool HasNote => !string.IsNullOrWhiteSpace(_note);

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
