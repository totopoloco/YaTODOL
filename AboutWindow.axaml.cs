using System;
using System.IO;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace YATODOL;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        LoadReleaseNotes();
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
