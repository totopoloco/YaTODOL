using Avalonia.Controls;
using Avalonia.Interactivity;

namespace YATODOL;

public partial class NoteEditorWindow : Window
{
    public string? ResultNote { get; private set; }
    public bool Deleted { get; private set; }

    public NoteEditorWindow()
    {
        InitializeComponent();
        ApplyLocalization();
    }

    public void LoadNote(string taskTitle, string? note)
    {
        SubtitleText.Text = Strings.NoteEditorSubtitle(taskTitle);
        NoteEditor.Text = note ?? string.Empty;
        DeleteNoteButton.IsVisible = !string.IsNullOrWhiteSpace(note);
    }

    private void ApplyLocalization()
    {
        Title = Strings.NoteEditorTitle;
        TitleText.Text = Strings.NoteEditorTitle;
        DeleteNoteButton.Content = Strings.NoteDeleteButton;
        CancelButton.Content = Strings.ButtonCancel;
        SaveButton.Content = Strings.ButtonSave;
        NoteEditor.PlaceholderText = Strings.NotePlaceholder;
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var note = NoteEditor.Text?.Trim();
        ResultNote = string.IsNullOrEmpty(note) ? null : note;
        Close(true);
    }

    private void OnDeleteNoteClick(object? sender, RoutedEventArgs e)
    {
        ResultNote = null;
        Deleted = true;
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
