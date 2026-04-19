using Avalonia;
using System;
using System.Runtime.InteropServices;

namespace YATODOL;

/// <summary>
/// Application entry point. Configures platform workarounds and starts the Avalonia desktop lifetime.
/// </summary>
class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Workaround: IBus on Linux intercepts dead-key compose sequences,
        // preventing accented characters (á, ó, ê, …) from reaching Avalonia.
        // Clearing these variables lets XIM handle composition directly.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Environment.SetEnvironmentVariable("GTK_IM_MODULE", "");
            Environment.SetEnvironmentVariable("QT_IM_MODULE", "");
            Environment.SetEnvironmentVariable("XMODIFIERS", "");
        }

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new X11PlatformOptions { WmClass = "yatodol" })
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
