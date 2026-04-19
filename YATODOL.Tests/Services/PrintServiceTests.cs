using YATODOL.Models;
using YATODOL.Services;
using YATODOL.Utilities;

namespace YATODOL.Tests.Services;

public class PrintServiceTests
{
    public PrintServiceTests()
    {
        Strings.SetLanguage(AppLanguage.English);
    }

    [Fact]
    public void GenerateHtml_EmptyList_ContainsNoTasksMessage()
    {
        var settings = new AppSettings();
        var html = PrintService.GenerateHtml([], [], settings, DateTime.Today);

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("YATODOL", html);
        Assert.Contains("No tasks to display", html);
    }

    [Fact]
    public void GenerateHtml_WithActiveItems_RendersActiveSectionTitle()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Active task", IsDone = false, Date = DateTime.Today }
        };
        var settings = new AppSettings();

        var html = PrintService.GenerateHtml(items, items, settings, DateTime.Today);

        Assert.Contains("Active task", html);
        Assert.Contains("Active", html);
    }

    [Fact]
    public void GenerateHtml_WithCompletedItems_RendersCompletedSection()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Done task", IsDone = true, Date = DateTime.Today }
        };
        var settings = new AppSettings { PrintFilter = PrintFilter.AllItems };

        var html = PrintService.GenerateHtml(items, items, settings, DateTime.Today);

        Assert.Contains("Done task", html);
        Assert.Contains("Completed", html);
    }

    [Fact]
    public void GenerateHtml_RemainingOnly_HidesCompletedItems()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Active task", IsDone = false, Date = DateTime.Today },
            new() { Title = "Done task", IsDone = true, Date = DateTime.Today }
        };
        var settings = new AppSettings { PrintFilter = PrintFilter.RemainingOnly };

        var html = PrintService.GenerateHtml(items, items, settings, DateTime.Today);

        Assert.Contains("Active task", html);
        // The completed items section should not appear
        Assert.DoesNotContain("Done task", html);
        Assert.DoesNotContain("section-title\">Completed", html);
    }

    [Fact]
    public void GenerateHtml_AllDatesScope_ShowsDayHeaders()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var items = new List<TodoItem>
        {
            new() { Title = "Today task", IsDone = false, Date = today },
            new() { Title = "Tomorrow task", IsDone = false, Date = tomorrow }
        };
        var settings = new AppSettings { PrintScope = PrintScope.AllDates };

        var html = PrintService.GenerateHtml(items, items, settings, today);

        Assert.Contains("day-header", html);
        Assert.Contains("All dates", html);
    }

    [Fact]
    public void GenerateHtml_SelectedDateScope_ShowsDateInSubtitle()
    {
        var date = new DateTime(2026, 4, 19);
        var items = new List<TodoItem>
        {
            new() { Title = "Task", IsDone = false, Date = date }
        };
        var settings = new AppSettings { PrintScope = PrintScope.SelectedDate };

        var html = PrintService.GenerateHtml(items, items, settings, date);

        Assert.Contains("April 19, 2026", html);
    }

    [Fact]
    public void GenerateHtml_ContainsSummarySection()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Task 1", IsDone = false, Date = DateTime.Today },
            new() { Title = "Task 2", IsDone = true, Date = DateTime.Today }
        };
        var settings = new AppSettings();

        var html = PrintService.GenerateHtml(items, items, settings, DateTime.Today);

        Assert.Contains("Total", html);
        Assert.Contains("Active", html);
        Assert.Contains("Completed", html);
    }

    [Fact]
    public void GenerateHtml_HtmlEncodesTaskTitles()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "<script>alert('xss')</script>", IsDone = false, Date = DateTime.Today }
        };
        var settings = new AppSettings();

        var html = PrintService.GenerateHtml(items, items, settings, DateTime.Today);

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public void GenerateHtml_RendersNotes()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Task with note", IsDone = false, Date = DateTime.Today, Note = "My note content" }
        };
        var settings = new AppSettings();

        var html = PrintService.GenerateHtml(items, items, settings, DateTime.Today);

        Assert.Contains("My note content", html);
        Assert.Contains("class='note'", html);
    }

    [Fact]
    public void GenerateHtml_HtmlEncodesNotes()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Task", IsDone = false, Date = DateTime.Today, Note = "<b>bold</b>" }
        };
        var settings = new AppSettings();

        var html = PrintService.GenerateHtml(items, items, settings, DateTime.Today);

        Assert.DoesNotContain("<b>bold</b>", html);
        Assert.Contains("&lt;b&gt;bold&lt;/b&gt;", html);
    }

    [Fact]
    public void GenerateHtml_IsValidHtmlDocument()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Task", IsDone = false, Date = DateTime.Today }
        };
        var settings = new AppSettings();

        var html = PrintService.GenerateHtml(items, items, settings, DateTime.Today);

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html>", html);
        Assert.Contains("</html>", html);
        Assert.Contains("<head>", html);
        Assert.Contains("</head>", html);
        Assert.Contains("<body>", html);
        Assert.Contains("</body>", html);
    }

    [Fact]
    public void GenerateHtml_AllDates_GroupsByDate()
    {
        var date1 = new DateTime(2026, 4, 18);
        var date2 = new DateTime(2026, 4, 19);
        var items = new List<TodoItem>
        {
            new() { Title = "Early", IsDone = false, Date = date1 },
            new() { Title = "Later", IsDone = false, Date = date2 }
        };
        var settings = new AppSettings { PrintScope = PrintScope.AllDates };

        var html = PrintService.GenerateHtml(items, items, settings, date1);

        // Both dates should appear as day headers in content
        var headerCount = html.Split("class='day-header'").Length - 1;
        Assert.Equal(2, headerCount);
    }
}
