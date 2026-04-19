using System.Globalization;
using Avalonia.Media;
using YATODOL.Converters;

namespace YATODOL.Tests.Converters;

public class BoolConvertersTests
{
    [Fact]
    public void ToOpacity_True_Returns05()
    {
        var result = BoolConverters.ToOpacity.Convert(true, typeof(double), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0.5, result);
    }

    [Fact]
    public void ToOpacity_False_Returns10()
    {
        var result = BoolConverters.ToOpacity.Convert(false, typeof(double), null!, CultureInfo.InvariantCulture);
        Assert.Equal(1.0, result);
    }

    [Fact]
    public void ToStrikethrough_True_ReturnsStrikethrough()
    {
        var result = BoolConverters.ToStrikethrough.Convert(true, typeof(TextDecorationCollection), null!, CultureInfo.InvariantCulture);
        Assert.NotNull(result);
        Assert.IsType<TextDecorationCollection>(result);
    }

    [Fact]
    public void ToStrikethrough_False_ReturnsNull()
    {
        var result = BoolConverters.ToStrikethrough.Convert(false, typeof(TextDecorationCollection), null!, CultureInfo.InvariantCulture);
        Assert.Null(result);
    }
}
