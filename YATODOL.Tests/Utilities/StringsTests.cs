using YATODOL.Models;
using YATODOL.Utilities;

namespace YATODOL.Tests.Utilities;

public class StringsTests : IDisposable
{
    public StringsTests()
    {
        Strings.SetLanguage(AppLanguage.English);
    }

    public void Dispose()
    {
        Strings.SetLanguage(AppLanguage.English);
    }

    [Fact]
    public void SetLanguage_English_ReturnsEnglishStrings()
    {
        Assert.Equal("Yet Another To Do List", Strings.AppTitle);
        Assert.Equal("Today", Strings.ButtonToday);
        Assert.Equal("Add", Strings.ButtonAdd);
        Assert.Equal("Cancel", Strings.ButtonCancel);
        Assert.Equal("Delete", Strings.ButtonDelete);
        Assert.Equal("Settings", Strings.SettingsWindowTitle);
        Assert.Equal("Close", Strings.ButtonClose);
        Assert.Equal("Save", Strings.ButtonSave);
    }

    [Fact]
    public void SetLanguage_Spanish_ReturnsSpanishStrings()
    {
        Strings.SetLanguage(AppLanguage.Spanish);

        Assert.Equal("Lista de Tareas", Strings.AppTitle);
        Assert.Equal("Hoy", Strings.ButtonToday);
        Assert.Equal("Añadir", Strings.ButtonAdd);
        Assert.Equal("Cancelar", Strings.ButtonCancel);
        Assert.Equal("Eliminar", Strings.ButtonDelete);
        Assert.Equal("Ajustes", Strings.SettingsWindowTitle);
        Assert.Equal("Cerrar", Strings.ButtonClose);
        Assert.Equal("Guardar", Strings.ButtonSave);
    }

    [Fact]
    public void SetLanguage_German_ReturnsGermanStrings()
    {
        Strings.SetLanguage(AppLanguage.German);

        Assert.Equal("Aufgabenliste", Strings.AppTitle);
        Assert.Equal("Heute", Strings.ButtonToday);
        Assert.Equal("Hinzufügen", Strings.ButtonAdd);
        Assert.Equal("Abbrechen", Strings.ButtonCancel);
        Assert.Equal("Löschen", Strings.ButtonDelete);
    }

    [Fact]
    public void SetLanguage_French_ReturnsFrenchStrings()
    {
        Strings.SetLanguage(AppLanguage.French);

        Assert.Equal("Liste de tâches", Strings.AppTitle);
        Assert.Equal("Aujourd'hui", Strings.ButtonToday);
        Assert.Equal("Ajouter", Strings.ButtonAdd);
        Assert.Equal("Annuler", Strings.ButtonCancel);
        Assert.Equal("Supprimer", Strings.ButtonDelete);
    }

    [Fact]
    public void RemainingOf_FormatsCorrectly()
    {
        Assert.Equal("(3 remaining of 5)", Strings.RemainingOf(3, 5));
    }

    [Fact]
    public void RemainingOf_SpanishFormat()
    {
        Strings.SetLanguage(AppLanguage.Spanish);
        Assert.Equal("(3 restantes de 5)", Strings.RemainingOf(3, 5));
    }

    [Fact]
    public void CountLabel_FormatsCorrectly()
    {
        Assert.Equal("2 remaining of 5 across 3 date(s)", Strings.CountLabel(2, 5, 3));
    }

    [Fact]
    public void ConfirmDeleteText_IncludesCountAndDate()
    {
        var text = Strings.ConfirmDeleteText(3, "Mon, Apr 19");
        Assert.Contains("3", text);
        Assert.Contains("Mon, Apr 19", text);
    }

    [Fact]
    public void NoteEditorSubtitle_IncludesTaskTitle()
    {
        Assert.Equal("Note for: My Task", Strings.NoteEditorSubtitle("My Task"));
    }

    [Fact]
    public void NoteEditorSubtitle_SpanishFormat()
    {
        Strings.SetLanguage(AppLanguage.Spanish);
        var result = Strings.NoteEditorSubtitle("My Task");
        Assert.Equal("Nota para: My Task", result);
    }

    [Fact]
    public void LanguageNames_HasCorrectCount()
    {
        Assert.Equal(4, Strings.LanguageNames.Length);
        Assert.Equal("English", Strings.LanguageNames[0]);
        Assert.Equal("Español", Strings.LanguageNames[1]);
        Assert.Equal("Deutsch", Strings.LanguageNames[2]);
        Assert.Equal("Français", Strings.LanguageNames[3]);
    }

    [Theory]
    [InlineData(AppLanguage.English)]
    [InlineData(AppLanguage.Spanish)]
    [InlineData(AppLanguage.German)]
    [InlineData(AppLanguage.French)]
    public void AllMainWindowStrings_AreNonEmpty(AppLanguage lang)
    {
        Strings.SetLanguage(lang);
        Assert.False(string.IsNullOrEmpty(Strings.AppTitle));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonToday));
        Assert.False(string.IsNullOrEmpty(Strings.TooltipDeleteDate));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonPrint));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonImport));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonExport));
        Assert.False(string.IsNullOrEmpty(Strings.TooltipICal));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonSettings));
        Assert.False(string.IsNullOrEmpty(Strings.TooltipAbout));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonAdd));
        Assert.False(string.IsNullOrEmpty(Strings.PlaceholderNewTask));
        Assert.False(string.IsNullOrEmpty(Strings.NoPastTasks));
        Assert.False(string.IsNullOrEmpty(Strings.ConfirmDeleteTitle));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonCancel));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonDelete));
        Assert.Contains("\u2014", Strings.TodayPrefix);
        Assert.False(string.IsNullOrEmpty(Strings.RemainingOf(1, 2)));
        Assert.False(string.IsNullOrEmpty(Strings.CountLabel(1, 2, 1)));
        Assert.False(string.IsNullOrEmpty(Strings.ConfirmDeleteText(1, "date")));
    }

    [Theory]
    [InlineData(AppLanguage.English)]
    [InlineData(AppLanguage.Spanish)]
    [InlineData(AppLanguage.German)]
    [InlineData(AppLanguage.French)]
    public void AllSettingsStrings_AreNonEmpty(AppLanguage lang)
    {
        Strings.SetLanguage(lang);
        Assert.False(string.IsNullOrEmpty(Strings.SettingsWindowTitle));
        Assert.False(string.IsNullOrEmpty(Strings.SettingsTitleLabel));
        Assert.False(string.IsNullOrEmpty(Strings.SettingsSubtitle));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonSave));
        Assert.False(string.IsNullOrEmpty(Strings.SectionAppearance));
        Assert.False(string.IsNullOrEmpty(Strings.ThemeLightLabel));
        Assert.False(string.IsNullOrEmpty(Strings.ThemeDarkLabel));
        Assert.False(string.IsNullOrEmpty(Strings.ThemeSystemLabel));
        Assert.False(string.IsNullOrEmpty(Strings.SectionGeneral));
        Assert.False(string.IsNullOrEmpty(Strings.ShowPathLabel));
        Assert.False(string.IsNullOrEmpty(Strings.ShowPathDesc));
        Assert.False(string.IsNullOrEmpty(Strings.HideCompletedLabel));
        Assert.False(string.IsNullOrEmpty(Strings.HideCompletedDesc));
        Assert.False(string.IsNullOrEmpty(Strings.CarryForwardLabel));
        Assert.False(string.IsNullOrEmpty(Strings.CarryForwardDesc));
        Assert.False(string.IsNullOrEmpty(Strings.SectionPrinting));
        Assert.False(string.IsNullOrEmpty(Strings.PrintScopeLabel));
        Assert.False(string.IsNullOrEmpty(Strings.PrintScopeSelected));
        Assert.False(string.IsNullOrEmpty(Strings.PrintScopeAll));
        Assert.False(string.IsNullOrEmpty(Strings.PrintFilterLabel));
        Assert.False(string.IsNullOrEmpty(Strings.PrintFilterAll));
        Assert.False(string.IsNullOrEmpty(Strings.PrintFilterRemaining));
        Assert.False(string.IsNullOrEmpty(Strings.SectionLanguage));
        Assert.False(string.IsNullOrEmpty(Strings.LanguagePickerLabel));
    }

    [Theory]
    [InlineData(AppLanguage.English)]
    [InlineData(AppLanguage.Spanish)]
    [InlineData(AppLanguage.German)]
    [InlineData(AppLanguage.French)]
    public void AllStorageStrings_AreNonEmpty(AppLanguage lang)
    {
        Strings.SetLanguage(lang);
        Assert.False(string.IsNullOrEmpty(Strings.SectionStorage));
        Assert.False(string.IsNullOrEmpty(Strings.CustomPathLabel));
        Assert.False(string.IsNullOrEmpty(Strings.CustomPathDesc));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonBrowse));
        Assert.False(string.IsNullOrEmpty(Strings.CustomPathPlaceholder));
        Assert.False(string.IsNullOrEmpty(Strings.BrowseFolderTitle));
    }

    [Theory]
    [InlineData(AppLanguage.English)]
    [InlineData(AppLanguage.Spanish)]
    [InlineData(AppLanguage.German)]
    [InlineData(AppLanguage.French)]
    public void AllAboutStrings_AreNonEmpty(AppLanguage lang)
    {
        Strings.SetLanguage(lang);
        Assert.False(string.IsNullOrEmpty(Strings.AboutWindowTitle));
        Assert.False(string.IsNullOrEmpty(Strings.AboutSubtitle));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonClose));
    }

    [Theory]
    [InlineData(AppLanguage.English)]
    [InlineData(AppLanguage.Spanish)]
    [InlineData(AppLanguage.German)]
    [InlineData(AppLanguage.French)]
    public void AllICalStrings_AreNonEmpty(AppLanguage lang)
    {
        Strings.SetLanguage(lang);
        Assert.False(string.IsNullOrEmpty(Strings.ICalWindowTitle));
        Assert.False(string.IsNullOrEmpty(Strings.ICalTitle));
        Assert.False(string.IsNullOrEmpty(Strings.ICalSubtitle));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonSelectAll));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonSelectNone));
        Assert.False(string.IsNullOrEmpty(Strings.ButtonUncompletedOnly));
    }

    [Theory]
    [InlineData(AppLanguage.English)]
    [InlineData(AppLanguage.Spanish)]
    [InlineData(AppLanguage.German)]
    [InlineData(AppLanguage.French)]
    public void AllPrintStrings_AreNonEmpty(AppLanguage lang)
    {
        Strings.SetLanguage(lang);
        Assert.False(string.IsNullOrEmpty(Strings.PrintAllDates));
        Assert.False(string.IsNullOrEmpty(Strings.PrintRemainingOnly));
        Assert.False(string.IsNullOrEmpty(Strings.PrintNoTasks));
        Assert.False(string.IsNullOrEmpty(Strings.PrintSectionActive));
        Assert.False(string.IsNullOrEmpty(Strings.PrintSectionCompleted));
        Assert.False(string.IsNullOrEmpty(Strings.PrintTotal));
        Assert.False(string.IsNullOrEmpty(Strings.PrintActive));
        Assert.False(string.IsNullOrEmpty(Strings.PrintCompleted));
    }

    [Theory]
    [InlineData(AppLanguage.English)]
    [InlineData(AppLanguage.Spanish)]
    [InlineData(AppLanguage.German)]
    [InlineData(AppLanguage.French)]
    public void AllNoteStrings_AreNonEmpty(AppLanguage lang)
    {
        Strings.SetLanguage(lang);
        Assert.False(string.IsNullOrEmpty(Strings.NoteEditorTitle));
        Assert.False(string.IsNullOrEmpty(Strings.NoteEditorSubtitle("task")));
        Assert.False(string.IsNullOrEmpty(Strings.NoteDeleteButton));
        Assert.False(string.IsNullOrEmpty(Strings.TooltipNote));
        Assert.False(string.IsNullOrEmpty(Strings.NotePlaceholder));
    }

    [Theory]
    [InlineData(AppLanguage.English)]
    [InlineData(AppLanguage.Spanish)]
    [InlineData(AppLanguage.German)]
    [InlineData(AppLanguage.French)]
    public void AllMiscStrings_AreNonEmpty(AppLanguage lang)
    {
        Strings.SetLanguage(lang);
        Assert.False(string.IsNullOrEmpty(Strings.TooltipDragReorder));
        Assert.False(string.IsNullOrEmpty(Strings.TooltipRename));
        Assert.False(string.IsNullOrEmpty(Strings.SaveFailed));
    }
}
