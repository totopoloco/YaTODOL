using Avalonia.Controls;
using Avalonia.Interactivity;

namespace YATODOL;

public partial class SettingsWindow : Window
{
    public AppSettings Result { get; private set; } = new();

    private AppLanguage _previousLanguage = AppLanguage.English;

    public SettingsWindow()
    {
        InitializeComponent();

        // Populate language ComboBox with native language names
        foreach (var name in Strings.LanguageNames)
            LanguageCombo.Items.Add(name);

        LanguageCombo.SelectionChanged += OnLanguageChanged;

        ApplyLocalization();
    }

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
            Language = LanguageCombo.SelectedIndex >= 0 ? (AppLanguage)LanguageCombo.SelectedIndex : AppLanguage.English
        };
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        // Restore language to what it was before this dialog was opened
        Strings.SetLanguage(_previousLanguage);
        Close(false);
    }
}
