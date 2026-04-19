using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace YATODOL.Services;

/// <summary>
/// Provides reusable modal confirmation dialogs.
/// </summary>
public static class DialogService
{
    /// <summary>
    /// Shows a modal confirmation dialog with a message and two buttons.
    /// </summary>
    /// <param name="owner">The parent window for centering.</param>
    /// <param name="title">The dialog window title.</param>
    /// <param name="message">The message to display.</param>
    /// <param name="confirmText">Text for the confirm button.</param>
    /// <param name="cancelText">Text for the cancel button.</param>
    /// <returns><c>true</c> if the user clicked confirm; otherwise <c>false</c>.</returns>
    public static async Task<bool> ShowConfirmDialog(
        Window owner, string title, string message,
        string confirmText, string cancelText)
    {
        var box = new Window
        {
            Title = title,
            Width = 400, Height = 160,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Icon = owner.Icon,
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap
                    },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Spacing = 8,
                        Children =
                        {
                            new Button { Content = cancelText, Width = 80, Tag = false },
                            new Button { Content = confirmText, Width = 80, Tag = true, Classes = { "accent" } }
                        }
                    }
                }
            }
        };

        var buttons = ((StackPanel)((StackPanel)box.Content).Children[1]).Children;
        foreach (Button btn in buttons)
            btn.Click += (_, _) => box.Close(btn.Tag);

        var result = await box.ShowDialog<object?>(owner);
        return result is true;
    }
}
