using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using YATODOL.Models;
using YATODOL.Services;
using YATODOL.Utilities;

namespace YATODOL.Views;

/// <summary>
/// Modal settings dialog for configuring theme, language, print options, task behavior, and custom data path.
/// </summary>
public partial class SettingsWindow : Window
{
    /// <summary>
    /// Gets the <see cref="AppSettings"/> built from the user's selections when the dialog is saved.
    /// </summary>
    public AppSettings Result { get; private set; } = new();

    private AppLanguage _previousLanguage = AppLanguage.English;
    private bool _closedByButton;

    public SettingsWindow()
    {
        InitializeComponent();
        Closing += OnWindowClosing;

        // Populate language ComboBox with native language names
        foreach (var name in Strings.LanguageNames)
            LanguageCombo.Items.Add(name);

        LanguageCombo.SelectionChanged += OnLanguageChanged;

        ApplyLocalization();
    }

    /// <summary>
    /// Populates the dialog controls from the given settings.
    /// </summary>
    /// <param name="settings">The current application settings to display.</param>
    public void LoadFrom(AppSettings settings)
    {
        _previousLanguage = settings.Language;

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
        LanguageCombo.SelectedIndex = (int)settings.Language;
        CustomPathCheck.IsChecked = settings.UseCustomDataPath;
        CustomPathBox.Text = settings.CustomDataPath;
        CustomPathPanel.IsVisible = settings.UseCustomDataPath;
    }

    private void OnLanguageChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (LanguageCombo.SelectedIndex >= 0)
        {
            Strings.SetLanguage((AppLanguage)LanguageCombo.SelectedIndex);
            ApplyLocalization();
        }
    }

    private void ApplyLocalization()
    {
        Title = Strings.SettingsWindowTitle;
        SettingsTitleText.Text = Strings.SettingsTitleLabel;
        SettingsSubtitleText.Text = Strings.SettingsSubtitle;
        CancelButton.Content = Strings.ButtonCancel;
        SaveButton.Content = Strings.ButtonSave;
        AppearanceSectionText.Text = Strings.SectionAppearance;
        ThemeLight.Content = Strings.ThemeLightLabel;
        ThemeDark.Content = Strings.ThemeDarkLabel;
        ThemeSystem.Content = Strings.ThemeSystemLabel;
        GeneralSectionText.Text = Strings.SectionGeneral;
        ShowPathLabelText.Text = Strings.ShowPathLabel;
        ShowPathDescText.Text = Strings.ShowPathDesc;
        HideCompletedLabelText.Text = Strings.HideCompletedLabel;
        HideCompletedDescText.Text = Strings.HideCompletedDesc;
        CarryForwardLabelText.Text = Strings.CarryForwardLabel;
        CarryForwardDescText.Text = Strings.CarryForwardDesc;
        PrintSectionText.Text = Strings.SectionPrinting;
        PrintScopeLabelText.Text = Strings.PrintScopeLabel;
        ScopeSelectedDate.Content = Strings.PrintScopeSelected;
        ScopeAllDates.Content = Strings.PrintScopeAll;
        PrintFilterLabelText.Text = Strings.PrintFilterLabel;
        FilterAll.Content = Strings.PrintFilterAll;
        FilterRemaining.Content = Strings.PrintFilterRemaining;
        LanguageSectionText.Text = Strings.SectionLanguage;
        LanguagePickerLabelText.Text = Strings.LanguagePickerLabel;
        StorageSectionText.Text = Strings.SectionStorage;
        CustomPathLabelText.Text = Strings.CustomPathLabel;
        CustomPathDescText.Text = Strings.CustomPathDesc;
        BrowseButton.Content = Strings.ButtonBrowse;
        CustomPathBox.PlaceholderText = Strings.CustomPathPlaceholder;
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
            PrintFilter = FilterRemaining.IsChecked == true ? PrintFilter.RemainingOnly : PrintFilter.AllItems,
            Language = LanguageCombo.SelectedIndex >= 0 ? (AppLanguage)LanguageCombo.SelectedIndex : AppLanguage.English,
            UseCustomDataPath = CustomPathCheck.IsChecked == true,
            CustomDataPath = CustomPathBox.Text ?? string.Empty
        };
        _closedByButton = true;
        Close(true);
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        if (!_closedByButton)
        {
            // Treat OS close as Cancel
            e.Cancel = true;
        }
    }

    private void OnCustomPathCheckChanged(object? sender, RoutedEventArgs e)
    {
        CustomPathPanel.IsVisible = CustomPathCheck.IsChecked == true;
    }

    private async void OnBrowseClick(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Strings.BrowseFolderTitle,
            AllowMultiple = false
        });
        if (folders.Count > 0)
        {
            CustomPathBox.Text = folders[0].Path.LocalPath;
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        // Restore language to what it was before this dialog was opened
        Strings.SetLanguage(_previousLanguage);
        _closedByButton = true;
        Close(false);
    }
}
