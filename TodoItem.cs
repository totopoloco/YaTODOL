using System;
using System.ComponentModel;

namespace YATODOL;

public class TodoItem : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private bool _isDone;
    private DateTime _date = DateTime.Today;

    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(nameof(Title)); }
    }

    public bool IsDone
    {
        get => _isDone;
        set { _isDone = value; OnPropertyChanged(nameof(IsDone)); }
    }

    public DateTime Date
    {
        get => _date;
        set { _date = value; OnPropertyChanged(nameof(Date)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
