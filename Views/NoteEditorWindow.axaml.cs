using Avalonia.Controls;
using Avalonia.Interactivity;
using YATODOL.Utilities;

namespace YATODOL.Views;

/// <summary>
/// Modal dialog for editing or deleting a note attached to a to-do item.
/// </summary>
public partial class NoteEditorWindow : Window
{
    /// <summary>
    /// Gets the note text after the dialog closes, or <c>null</c> if the note was deleted or empty.
    /// </summary>
    public string? ResultNote { get; private set; }

    /// <summary>
    /// Gets whether the user explicitly deleted the note.
    /// </summary>
    public bool Deleted { get; private set; }

    public NoteEditorWindow()
    {
        InitializeComponent();
        ApplyLocalization();
    }

    /// <summary>
    /// Loads the note content into the editor.
    /// </summary>
    /// <param name="taskTitle">The title of the task (shown in the subtitle).</param>
    /// <param name="note">The existing note text, or <c>null</c> for a new note.</param>
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
