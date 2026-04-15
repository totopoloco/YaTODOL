using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace YATODOL;

public static class BoolConverters
{
    public static readonly IValueConverter ToOpacity =
        new FuncValueConverter<bool, double>(b => b ? 0.5 : 1.0);

    public static readonly IValueConverter ToStrikethrough =
        new FuncValueConverter<bool, TextDecorationCollection?>(b =>
            b ? TextDecorations.Strikethrough : null);
}
