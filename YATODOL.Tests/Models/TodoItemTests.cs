using System.ComponentModel;
using YATODOL.Models;

namespace YATODOL.Tests.Models;

public class TodoItemTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var item = new TodoItem();

        Assert.Equal(string.Empty, item.Title);
        Assert.False(item.IsDone);
        Assert.Equal(DateTime.Today, item.Date);
        Assert.Null(item.Note);
        Assert.Equal(0, item.SortOrder);
        Assert.False(item.HasNote);
    }

    [Fact]
    public void Title_SetValue_RaisesPropertyChanged()
    {
        var item = new TodoItem();
        var raised = AssertPropertyChanged(item, nameof(TodoItem.Title), () => item.Title = "Test");

        Assert.True(raised);
        Assert.Equal("Test", item.Title);
    }

    [Fact]
    public void IsDone_SetValue_RaisesPropertyChanged()
    {
        var item = new TodoItem();
        var raised = AssertPropertyChanged(item, nameof(TodoItem.IsDone), () => item.IsDone = true);

        Assert.True(raised);
        Assert.True(item.IsDone);
    }

    [Fact]
    public void Date_SetValue_RaisesPropertyChanged()
    {
        var item = new TodoItem();
        var tomorrow = DateTime.Today.AddDays(1);
        var raised = AssertPropertyChanged(item, nameof(TodoItem.Date), () => item.Date = tomorrow);

        Assert.True(raised);
        Assert.Equal(tomorrow, item.Date);
    }

    [Fact]
    public void Note_SetValue_RaisesPropertyChangedForNoteAndHasNote()
    {
        var item = new TodoItem();
        var changedProperties = new List<string>();
        item.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        item.Note = "A note";

        Assert.Contains(nameof(TodoItem.Note), changedProperties);
        Assert.Contains(nameof(TodoItem.HasNote), changedProperties);
        Assert.Equal("A note", item.Note);
    }

    [Fact]
    public void HasNote_ReturnsFalse_WhenNoteIsNull()
    {
        var item = new TodoItem { Note = null };
        Assert.False(item.HasNote);
    }

    [Fact]
    public void HasNote_ReturnsFalse_WhenNoteIsWhitespace()
    {
        var item = new TodoItem { Note = "   " };
        Assert.False(item.HasNote);
    }

    [Fact]
    public void HasNote_ReturnsTrue_WhenNoteHasContent()
    {
        var item = new TodoItem { Note = "Some note" };
        Assert.True(item.HasNote);
    }

    [Fact]
    public void SortOrder_SetValue_RaisesPropertyChanged()
    {
        var item = new TodoItem();
        var raised = AssertPropertyChanged(item, nameof(TodoItem.SortOrder), () => item.SortOrder = 5);

        Assert.True(raised);
        Assert.Equal(5, item.SortOrder);
    }

    [Fact]
    public void Title_Rename_UpdatesTitleAndRaisesPropertyChanged()
    {
        var item = new TodoItem { Title = "Original task" };
        var raised = AssertPropertyChanged(item, nameof(TodoItem.Title), () => item.Title = "Renamed task");

        Assert.True(raised);
        Assert.Equal("Renamed task", item.Title);
    }

    [Fact]
    public void Title_Rename_ToSameValue_StillRaisesPropertyChanged()
    {
        var item = new TodoItem { Title = "Same" };
        var raised = AssertPropertyChanged(item, nameof(TodoItem.Title), () => item.Title = "Same");

        Assert.True(raised);
        Assert.Equal("Same", item.Title);
    }

    [Fact]
    public void Title_Rename_PreservesOtherProperties()
    {
        var item = new TodoItem
        {
            Title = "Buy groceries",
            IsDone = true,
            Date = new DateTime(2026, 4, 19),
            Note = "Don't forget milk",
            SortOrder = 3
        };

        item.Title = "Buy organic groceries";

        Assert.Equal("Buy organic groceries", item.Title);
        Assert.True(item.IsDone);
        Assert.Equal(new DateTime(2026, 4, 19), item.Date);
        Assert.Equal("Don't forget milk", item.Note);
        Assert.Equal(3, item.SortOrder);
    }

    private static bool AssertPropertyChanged(TodoItem item, string expectedProperty, Action action)
    {
        bool raised = false;
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == expectedProperty)
                raised = true;
        };
        action();
        return raised;
    }
}
