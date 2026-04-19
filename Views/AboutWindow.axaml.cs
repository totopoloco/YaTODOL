using System;
using System.IO;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YATODOL.Utilities;

namespace YATODOL.Views;

/// <summary>
/// Modal dialog displaying application version and release notes loaded from <c>RELEASE.md</c>.
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        ApplyLocalization();
        LoadReleaseNotes();
    }

    private void ApplyLocalization()
    {
        Title = Strings.AboutWindowTitle;
        AboutSubtitleText.Text = Strings.AboutSubtitle;
        CloseButton.Content = Strings.ButtonClose;
    }

    private void LoadReleaseNotes()
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "unknown";

        // Strip build metadata (e.g. +commitsha) if present
        var plusIndex = version.IndexOf('+');
        if (plusIndex >= 0)
            version = version[..plusIndex];

        VersionLabel.Text = $"Version {version}";

        // Try to load RELEASE.md from next to the executable
        var exeDir = AppContext.BaseDirectory;
        var releasePath = Path.Combine(exeDir, "RELEASE.md");
        if (File.Exists(releasePath))
        {
            ReleaseNotesText.Text = File.ReadAllText(releasePath);
        }
        else
        {
            ReleaseNotesText.Text = $"Version {version}";
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
