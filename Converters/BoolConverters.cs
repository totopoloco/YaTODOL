using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace YATODOL.Converters;

/// <summary>
/// Provides Avalonia value converters that map boolean values to visual properties for task display.
/// </summary>
public static class BoolConverters
{
    /// <summary>
    /// Converts a boolean to opacity: <c>true</c> (done) yields 0.5, <c>false</c> yields 1.0.
    /// </summary>
    public static readonly IValueConverter ToOpacity =
        new FuncValueConverter<bool, double>(b => b ? 0.5 : 1.0);

    /// <summary>
    /// Converts a boolean to text decorations: <c>true</c> (done) yields strikethrough, <c>false</c> yields none.
    /// </summary>
    public static readonly IValueConverter ToStrikethrough =
        new FuncValueConverter<bool, TextDecorationCollection?>(b =>
            b ? TextDecorations.Strikethrough : null);
}
