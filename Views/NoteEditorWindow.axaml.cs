using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using YATODOL.Models;
using YATODOL.Utilities;

namespace YATODOL.Views;

/// <summary>
/// Modal dialog for editing/deleting the note and managing tags on a to-do item.
/// </summary>
public partial class NoteEditorWindow : Window
{
    /// <summary>Gets the note text after the dialog closes, or <c>null</c> if deleted/empty.</summary>
    public string? ResultNote { get; private set; }

    /// <summary>Gets whether the user explicitly deleted the note.</summary>
    public bool Deleted { get; private set; }

    /// <summary>Gets the updated tag list after the dialog closes.</summary>
    public List<string> ResultTags { get; private set; } = [];

    private List<string> _selectedTags = [];
    private List<TagDefinition> _allTags = [];

    public NoteEditorWindow()
    {
        InitializeComponent();
        ApplyLocalization();
    }

    /// <summary>
    /// Loads the note content and tag state into the editor.
    /// </summary>
    /// <param name="taskTitle">The title of the task (shown in the subtitle).</param>
    /// <param name="note">The existing note text, or <c>null</c> for a new note.</param>
    /// <param name="currentTags">The tag keys currently assigned to this task.</param>
    /// <param name="allTags">All available tag definitions to show as selectable chips.</param>
    public void LoadNote(string taskTitle, string? note,
                         List<string> currentTags, IEnumerable<TagDefinition> allTags)
    {
        _allTags = allTags.ToList();
        _selectedTags = new List<string>(currentTags);

        SubtitleText.Text = Strings.NoteEditorSubtitle(taskTitle);
        NoteEditor.Text = note ?? string.Empty;
        DeleteNoteButton.IsVisible = !string.IsNullOrWhiteSpace(note);

        BuildTagChips();
    }

    private void BuildTagChips()
    {
        TagsPanel.Children.Clear();
        foreach (var tag in _allTags)
        {
            var isSelected = _selectedTags.Contains(tag.Name);
            TagsPanel.Children.Add(MakeTagChip(tag, isSelected));
        }
    }

    private Border MakeTagChip(TagDefinition tag, bool isSelected)
    {
        var col = Color.Parse(tag.Color);
        var fullBrush = new SolidColorBrush(col);

        Border chip;
        if (isSelected)
        {
            chip = new Border
            {
                CornerRadius = new CornerRadius(12),
                Background = fullBrush,
                Padding = new Thickness(10, 5),
                Margin = new Thickness(0, 0, 0, 4),
                Cursor = new Cursor(StandardCursorType.Hand),
                Child = new TextBlock
                {
                    Text = Strings.GetTagDisplayName(tag.Name),
                    Foreground = Brushes.White,
                    FontSize = 11,
                    FontWeight = FontWeight.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }
        else
        {
            chip = new Border
            {
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(Color.FromArgb(30, col.R, col.G, col.B)),
                BorderBrush = fullBrush,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(9, 4),
                Margin = new Thickness(0, 0, 0, 4),
                Cursor = new Cursor(StandardCursorType.Hand),
                Child = new TextBlock
                {
                    Text = Strings.GetTagDisplayName(tag.Name),
                    Foreground = fullBrush,
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        chip.PointerPressed += (_, _) => ToggleTag(tag.Name);
        return chip;
    }

    private void ToggleTag(string tagName)
    {
        if (_selectedTags.Contains(tagName))
        {
            _selectedTags.Remove(tagName);
        }
        else
        {
            // Built-in tags are mutually exclusive: deselect any other built-in before adding.
            if (BuiltInTags.IsBuiltIn(tagName))
                _selectedTags.RemoveAll(BuiltInTags.IsBuiltIn);

            _selectedTags.Add(tagName);
        }

        BuildTagChips();
    }

    private void ApplyLocalization()
    {
        Title = Strings.NoteEditorTitle;
        TitleText.Text = Strings.NoteEditorTitle;
        TagsSectionLabel.Text = Strings.NoteEditorTagsLabel;
        DeleteNoteButton.Content = Strings.NoteDeleteButton;
        CancelButton.Content = Strings.ButtonCancel;
        SaveButton.Content = Strings.ButtonSave;
        NoteEditor.PlaceholderText = Strings.NotePlaceholder;
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var note = NoteEditor.Text?.Trim();
        ResultNote = string.IsNullOrEmpty(note) ? null : note;
        ResultTags = new List<string>(_selectedTags);
        Close(true);
    }

    private void OnDeleteNoteClick(object? sender, RoutedEventArgs e)
    {
        ResultNote = null;
        ResultTags = new List<string>(_selectedTags);
        Deleted = true;
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
