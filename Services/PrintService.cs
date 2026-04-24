using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using YATODOL.Models;
using YATODOL.Utilities;

namespace YATODOL.Services;

/// <summary>
/// Generates a self-contained HTML document for printing to-do items,
/// with date grouping, active/completed sections, notes, and a summary footer.
/// </summary>
public static class PrintService
{
    /// <summary>
    /// Generates a styled HTML document for printing tasks.
    /// </summary>
    /// <param name="items">The filtered list of items to render.</param>
    /// <param name="allItems">All items (used for the summary counts).</param>
    /// <param name="settings">Application settings controlling print scope and filter.</param>
    /// <param name="selectedDate">The currently selected date in the UI.</param>
    /// <returns>A complete HTML document string.</returns>
    public static string GenerateHtml(
        List<TodoItem> items,
        IEnumerable<TodoItem> allItems,
        AppSettings settings,
        DateTime selectedDate,
        IEnumerable<TagDefinition>? allTags = null)
    {
        var allDates = settings.PrintScope == PrintScope.AllDates;
        var remainingOnly = settings.PrintFilter == PrintFilter.RemainingOnly;

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset='utf-8'>");
        sb.AppendLine("<title>YATODOL</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("""
            @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap');
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body { font-family: 'Inter', sans-serif; max-width: 640px; margin: 40px auto;
                   padding: 0 24px; color: #1a1a2e; }
            h1 { font-size: 28px; font-weight: 700; margin-bottom: 4px; }
            .subtitle { color: #888; font-size: 14px; margin-bottom: 32px; }
            .day-header { font-size: 16px; font-weight: 700; color: #1a1a2e; margin: 28px 0 8px;
                          padding-bottom: 6px; border-bottom: 2px solid #4a6cf7; }
            .day-header:first-of-type { margin-top: 0; }
            .section { margin-bottom: 16px; }
            .section-title { font-size: 12px; font-weight: 600; text-transform: uppercase;
                             letter-spacing: 1.2px; color: #555; margin: 12px 0 8px;
                             padding-bottom: 4px; border-bottom: 1px solid #e0e0e0; }
            ul { list-style: none; }
            li { padding: 8px 0; border-bottom: 1px solid #f0f0f0; display: flex;
                 align-items: center; gap: 12px; font-size: 15px; }
            li:last-child { border-bottom: none; }
            .checkbox { width: 18px; height: 18px; border: 2px solid #ccc; border-radius: 4px;
                        flex-shrink: 0; display: flex; align-items: center; justify-content: center; }
            .done .checkbox { border-color: #4caf50; background: #4caf50; }
            .done .checkbox::after { content: '\2713'; color: white; font-size: 12px; font-weight: 700; }
            .done .text { color: #999; text-decoration: line-through; }
            .note { margin: 4px 0 4px 30px; padding: 8px 12px; background: #f8f8fa;
                    border-left: 3px solid #4a6cf7; border-radius: 0 6px 6px 0;
                    font-size: 13px; color: #555; white-space: pre-wrap; line-height: 1.5; }
            .done .note { border-left-color: #ccc; }
            .tags { display: flex; flex-wrap: wrap; gap: 4px; margin-left: auto; }
            .tag { display: inline-block; padding: 2px 8px; border-radius: 10px;
                   font-size: 11px; font-weight: 600; color: white; white-space: nowrap; }
            .summary { margin-top: 32px; padding: 16px; border-top: 2px solid #e0e0e0;
                       font-size: 14px; color: #666; display: flex; gap: 24px; }
            .summary span { font-weight: 600; color: #333; }
            .empty { color: #aaa; font-style: italic; padding: 12px 0; }
            @media print { body { margin: 20px auto; } }
            """);
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h1>\u2611 YATODOL</h1>");

        var subtitle = allDates ? Strings.PrintAllDates : selectedDate.ToString("MMMM d, yyyy");
        if (remainingOnly) subtitle += Strings.PrintRemainingOnly;
        sb.AppendLine($"<p class='subtitle'>{WebUtility.HtmlEncode(subtitle)}</p>");

        var tagLookup = (allTags ?? Enumerable.Empty<TagDefinition>())
            .ToDictionary(t => t.Name, t => t.Color);

        if (items.Count == 0)
        {
            sb.AppendLine($"<p class='empty'>{WebUtility.HtmlEncode(Strings.PrintNoTasks)}</p>");
        }
        else if (allDates)
        {
            var groups = items.OrderBy(i => i.Date).ThenBy(i => i.IsDone).GroupBy(i => i.Date.Date);
            foreach (var group in groups)
            {
                sb.AppendLine($"<div class='day-header'>{WebUtility.HtmlEncode(group.Key.ToString("dddd, MMMM d, yyyy"))}</div>");
                RenderItemList(sb, group.ToList(), remainingOnly, tagLookup);
            }
        }
        else
        {
            RenderItemList(sb, items, remainingOnly, tagLookup);
        }

        var allItemsList = allItems.ToList();
        var totalAll = allDates ? allItemsList.Count : allItemsList.Count(i => i.Date.Date == selectedDate.Date);
        var activeAll = allDates ? allItemsList.Count(i => !i.IsDone) : allItemsList.Count(i => i.Date.Date == selectedDate.Date && !i.IsDone);
        var doneAll = totalAll - activeAll;
        sb.AppendLine($"<div class='summary'>" +
            $"<div>{Strings.PrintTotal}: <span>{totalAll}</span></div>" +
            $"<div>{Strings.PrintActive}: <span>{activeAll}</span></div>" +
            $"<div>{Strings.PrintCompleted}: <span>{doneAll}</span></div></div>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private static void RenderItemList(StringBuilder sb, List<TodoItem> items, bool remainingOnly,
        Dictionary<string, string>? tagLookup = null)
    {
        var active = items.Where(i => !i.IsDone).ToList();
        var done = items.Where(i => i.IsDone).ToList();

        if (active.Count > 0)
        {
            sb.AppendLine("<div class='section'>");
            sb.AppendLine($"<div class='section-title'>{Strings.PrintSectionActive}</div><ul>");
            foreach (var item in active)
            {
                sb.AppendLine($"<li><div class='checkbox'></div><span class='text'>{WebUtility.HtmlEncode(item.Title)}</span>{RenderTagChips(item, tagLookup)}</li>");
                if (item.HasNote)
                    sb.AppendLine($"<div class='note'>{WebUtility.HtmlEncode(item.Note!)}</div>");
            }
            sb.AppendLine("</ul></div>");
        }

        if (!remainingOnly && done.Count > 0)
        {
            sb.AppendLine("<div class='section'>");
            sb.AppendLine($"<div class='section-title'>{Strings.PrintSectionCompleted}</div><ul>");
            foreach (var item in done)
            {
                sb.AppendLine($"<li class='done'><div class='checkbox'></div><span class='text'>{WebUtility.HtmlEncode(item.Title)}</span>{RenderTagChips(item, tagLookup)}</li>");
                if (item.HasNote)
                    sb.AppendLine($"<div class='note'>{WebUtility.HtmlEncode(item.Note!)}</div>");
            }
            sb.AppendLine("</ul></div>");
        }
    }

    private static string RenderTagChips(TodoItem item, Dictionary<string, string>? tagLookup)
    {
        if (tagLookup == null || item.Tags.Count == 0) return string.Empty;
        var chips = new StringBuilder();
        chips.Append("<div class='tags'>");
        foreach (var tag in item.Tags)
        {
            var color = tagLookup.TryGetValue(tag, out var c) ? c : "#888888";
            var label = WebUtility.HtmlEncode(Strings.GetTagDisplayName(tag));
            chips.Append($"<span class='tag' style='background:{color}'>{label}</span>");
        }
        chips.Append("</div>");
        return chips.ToString();
    }
}
