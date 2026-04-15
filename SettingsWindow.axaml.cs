using Avalonia.Controls;
using Avalonia.Interactivity;

namespace YATODOL;

public partial class SettingsWindow : Window
{
    public AppSettings Result { get; private set; } = new();

    public SettingsWindow()
    {
        InitializeComponent();
    }

    public void LoadFrom(AppSettings settings)
    {
        ThemeLight.IsChecked = settings.Theme == ThemeMode.Light;
        ThemeDark.IsChecked = settings.Theme == ThemeMode.Dark;
        ThemeSystem.IsChecked = settings.Theme == ThemeMode.System;
        ShowPathCheck.IsChecked = settings.ShowPathInTitle;
        HideCompletedDatesCheck.IsChecked = settings.HideCompletedDates;
        CarryForwardCheck.IsChecked = settings.CarryForwardTasks;
        ScopeSelectedDate.IsChecked = settings.PrintScope == PrintScope.SelectedDate;
        ScopeAllDates.IsChecked = settings.PrintScope == PrintScope.AllDates;
        FilterAll.IsChecked = settings.PrintFilter == PrintFilter.AllItems;
        FilterRemaining.IsChecked = settings.PrintFilter == PrintFilter.RemainingOnly;
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        Result = new AppSettings
        {
            Theme = ThemeDark.IsChecked == true ? ThemeMode.Dark
                  : ThemeSystem.IsChecked == true ? ThemeMode.System
                  : ThemeMode.Light,
            ShowPathInTitle = ShowPathCheck.IsChecked == true,
            HideCompletedDates = HideCompletedDatesCheck.IsChecked == true,
            CarryForwardTasks = CarryForwardCheck.IsChecked == true,
            PrintScope = ScopeAllDates.IsChecked == true ? PrintScope.AllDates : PrintScope.SelectedDate,
            PrintFilter = FilterRemaining.IsChecked == true ? PrintFilter.RemainingOnly : PrintFilter.AllItems
        };
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
