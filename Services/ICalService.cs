using System;
using System.Collections.Generic;
using System.Text;
using YATODOL.Models;

namespace YATODOL.Services;

/// <summary>
/// Generates iCalendar (.ics) formatted strings from to-do items for calendar export.
/// </summary>
public static class ICalService
{
    /// <summary>
    /// Generates a complete iCalendar string containing one all-day VEVENT per to-do item.
    /// Completed items are prefixed with a checkmark, uncompleted with a ballot box.
    /// </summary>
    /// <param name="items">The to-do items to export.</param>
    /// <returns>A valid iCalendar string.</returns>
    public static string GenerateICalString(IEnumerable<TodoItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//YATODOL//Yet Another To Do List//EN");
        sb.AppendLine("CALSCALE:GREGORIAN");
        sb.AppendLine("METHOD:PUBLISH");

        foreach (var item in items)
        {
            var uid = Guid.NewGuid().ToString();
            var dateStr = item.Date.ToString("yyyyMMdd");
            var nextDateStr = item.Date.AddDays(1).ToString("yyyyMMdd");
            var nowStr = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");
            var prefix = item.IsDone ? "\u2713 " : "\u2610 ";

            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{uid}");
            sb.AppendLine($"DTSTAMP:{nowStr}");
            sb.AppendLine($"DTSTART;VALUE=DATE:{dateStr}");
            sb.AppendLine($"DTEND;VALUE=DATE:{nextDateStr}");
            sb.AppendLine($"SUMMARY:{EscapeICalText(prefix + item.Title)}");
            sb.AppendLine("TRANSP:TRANSPARENT");
            sb.AppendLine("END:VEVENT");
        }

        sb.AppendLine("END:VCALENDAR");
        return sb.ToString();
    }

    private static string EscapeICalText(string text) =>
        text.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\n", "\\n");
}
