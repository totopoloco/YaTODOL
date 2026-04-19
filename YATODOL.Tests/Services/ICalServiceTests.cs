using YATODOL.Models;
using YATODOL.Services;

namespace YATODOL.Tests.Services;

public class ICalServiceTests
{
    [Fact]
    public void GenerateICalString_EmptyList_ReturnsValidCalendar()
    {
        var result = ICalService.GenerateICalString([]);

        Assert.Contains("BEGIN:VCALENDAR", result);
        Assert.Contains("END:VCALENDAR", result);
        Assert.Contains("VERSION:2.0", result);
        Assert.Contains("PRODID:-//YATODOL", result);
        Assert.DoesNotContain("BEGIN:VEVENT", result);
    }

    [Fact]
    public void GenerateICalString_SingleItem_ContainsEvent()
    {
        var items = new List<TodoItem>
        {
            new()
            {
                Title = "Buy groceries",
                Date = new DateTime(2026, 4, 19),
                IsDone = false
            }
        };

        var result = ICalService.GenerateICalString(items);

        Assert.Contains("BEGIN:VEVENT", result);
        Assert.Contains("END:VEVENT", result);
        Assert.Contains("DTSTART;VALUE=DATE:20260419", result);
        Assert.Contains("DTEND;VALUE=DATE:20260420", result);
        Assert.Contains("Buy groceries", result);
        Assert.Contains("TRANSP:TRANSPARENT", result);
    }

    [Fact]
    public void GenerateICalString_DoneItem_HasCheckPrefix()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Done task", IsDone = true, Date = DateTime.Today }
        };

        var result = ICalService.GenerateICalString(items);

        Assert.Contains("SUMMARY:\u2713 Done task", result);
    }

    [Fact]
    public void GenerateICalString_UndoneItem_HasBoxPrefix()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Open task", IsDone = false, Date = DateTime.Today }
        };

        var result = ICalService.GenerateICalString(items);

        Assert.Contains("SUMMARY:\u2610 Open task", result);
    }

    [Fact]
    public void GenerateICalString_MultipleItems_CreatesMultipleEvents()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Task 1", Date = DateTime.Today },
            new() { Title = "Task 2", Date = DateTime.Today },
            new() { Title = "Task 3", Date = DateTime.Today.AddDays(1) }
        };

        var result = ICalService.GenerateICalString(items);

        var eventCount = result.Split("BEGIN:VEVENT").Length - 1;
        Assert.Equal(3, eventCount);
    }

    [Fact]
    public void GenerateICalString_EscapesSpecialCharacters()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Task with; comma, and\\backslash", Date = DateTime.Today }
        };

        var result = ICalService.GenerateICalString(items);

        Assert.Contains("\\;", result);
        Assert.Contains("\\,", result);
        Assert.Contains("\\\\", result);
    }

    [Fact]
    public void GenerateICalString_ContainsUniqueUIDs()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Task 1", Date = DateTime.Today },
            new() { Title = "Task 2", Date = DateTime.Today }
        };

        var result = ICalService.GenerateICalString(items);

        var lines = result.Split('\n');
        var uids = lines.Where(l => l.StartsWith("UID:")).Select(l => l.Trim()).ToList();
        Assert.Equal(2, uids.Count);
        Assert.NotEqual(uids[0], uids[1]);
    }

    [Fact]
    public void GenerateICalString_HasCorrectStructure()
    {
        var items = new List<TodoItem>
        {
            new() { Title = "Test", Date = DateTime.Today }
        };

        var result = ICalService.GenerateICalString(items);

        // Calendar must start before any events
        var calStart = result.IndexOf("BEGIN:VCALENDAR");
        var eventStart = result.IndexOf("BEGIN:VEVENT");
        var eventEnd = result.IndexOf("END:VEVENT");
        var calEnd = result.IndexOf("END:VCALENDAR");

        Assert.True(calStart < eventStart);
        Assert.True(eventStart < eventEnd);
        Assert.True(eventEnd < calEnd);
    }
}
