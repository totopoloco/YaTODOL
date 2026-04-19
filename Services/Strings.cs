using YATODOL.Models;

namespace YATODOL.Utilities;

/// <summary>
/// Static localization provider. Call SetLanguage() once at startup and after language changes.
/// </summary>
public static class Strings
{
    private static AppLanguage _language = AppLanguage.English;

    /// <summary>
    /// Sets the active language for all localized string properties.
    /// </summary>
    /// <param name="lang">The language to use.</param>
    public static void SetLanguage(AppLanguage lang) => _language = lang;

    // Language names shown in their own language (order matches AppLanguage enum)
    public static readonly string[] LanguageNames = ["English", "Español", "Deutsch", "Français"];

    // ─── Main Window ────────────────────────────────────────────────────────

    public static string AppTitle => _language switch
    {
        AppLanguage.Spanish => "Lista de Tareas",
        AppLanguage.German  => "Aufgabenliste",
        AppLanguage.French  => "Liste de tâches",
        _                   => "Yet Another To Do List"
    };

    public static string ButtonToday => _language switch
    {
        AppLanguage.Spanish => "Hoy",
        AppLanguage.German  => "Heute",
        AppLanguage.French  => "Aujourd'hui",
        _                   => "Today"
    };

    public static string TooltipDeleteDate => _language switch
    {
        AppLanguage.Spanish => "Eliminar todas las tareas de la fecha seleccionada",
        AppLanguage.German  => "Alle Aufgaben des gewählten Datums löschen",
        AppLanguage.French  => "Supprimer toutes les tâches de la date sélectionnée",
        _                   => "Delete all tasks for selected date"
    };

    public static string ButtonPrint => _language switch
    {
        AppLanguage.Spanish => "🖨 Imprimir",
        AppLanguage.German  => "🖨 Drucken",
        AppLanguage.French  => "🖨 Imprimer",
        _                   => "🖨 Print"
    };

    public static string ButtonImport => _language switch
    {
        AppLanguage.Spanish => "📥 Importar",
        AppLanguage.German  => "📥 Importieren",
        AppLanguage.French  => "📥 Importer",
        _                   => "📥 Import"
    };

    public static string ButtonExport => _language switch
    {
        AppLanguage.Spanish => "📤 Exportar",
        AppLanguage.German  => "📤 Exportieren",
        AppLanguage.French  => "📤 Exporter",
        _                   => "📤 Export"
    };

    public static string TooltipICal => _language switch
    {
        AppLanguage.Spanish => "Exportar tareas a iCalendar (.ics)",
        AppLanguage.German  => "Aufgaben als iCalendar (.ics) exportieren",
        AppLanguage.French  => "Exporter les tâches vers iCalendar (.ics)",
        _                   => "Export tasks to iCalendar (.ics)"
    };

    public static string ButtonSettings => _language switch
    {
        AppLanguage.Spanish => "⚙ Ajustes",
        AppLanguage.German  => "⚙ Einstellungen",
        AppLanguage.French  => "⚙ Paramètres",
        _                   => "⚙ Settings"
    };

    public static string TooltipAbout => _language switch
    {
        AppLanguage.Spanish => "Acerca de YaTODOL",
        AppLanguage.German  => "Über YaTODOL",
        AppLanguage.French  => "À propos de YaTODOL",
        _                   => "About YaTODOL"
    };

    public static string ButtonAdd => _language switch
    {
        AppLanguage.Spanish => "Añadir",
        AppLanguage.German  => "Hinzufügen",
        AppLanguage.French  => "Ajouter",
        _                   => "Add"
    };

    public static string PlaceholderNewTask => _language switch
    {
        AppLanguage.Spanish => "Añadir una nueva tarea...",
        AppLanguage.German  => "Neue Aufgabe hinzufügen...",
        AppLanguage.French  => "Ajouter une nouvelle tâche...",
        _                   => "Add a new task..."
    };

    public static string NoPastTasks => _language switch
    {
        AppLanguage.Spanish => "¡No se pueden añadir tareas en el pasado!",
        AppLanguage.German  => "Aufgaben in der Vergangenheit können nicht hinzugefügt werden!",
        AppLanguage.French  => "Impossible d'ajouter des tâches dans le passé !",
        _                   => "Cannot add tasks in the past!"
    };

    public static string ConfirmDeleteTitle => _language switch
    {
        AppLanguage.Spanish => "Confirmar eliminación",
        AppLanguage.German  => "Löschen bestätigen",
        AppLanguage.French  => "Confirmer la suppression",
        _                   => "Confirm Delete"
    };

    public static string ConfirmDeleteText(int count, string date) => _language switch
    {
        AppLanguage.Spanish => $"¿Eliminar {count} tarea(s) del {date}?\nEsto incluye las completadas y pendientes.",
        AppLanguage.German  => $"Alle {count} Aufgabe(n) vom {date} löschen?\nDies schließt abgeschlossene und offene Aufgaben ein.",
        AppLanguage.French  => $"Supprimer {count} tâche(s) du {date} ?\nCela inclut les tâches terminées et non terminées.",
        _                   => $"Delete all {count} task(s) for {date}?\nThis includes completed and uncompleted items."
    };

    public static string ButtonCancel => _language switch
    {
        AppLanguage.Spanish => "Cancelar",
        AppLanguage.German  => "Abbrechen",
        AppLanguage.French  => "Annuler",
        _                   => "Cancel"
    };

    public static string ButtonDelete => _language switch
    {
        AppLanguage.Spanish => "Eliminar",
        AppLanguage.German  => "Löschen",
        AppLanguage.French  => "Supprimer",
        _                   => "Delete"
    };

    public static string TodayPrefix => _language switch
    {
        AppLanguage.Spanish => "Hoy \u2014 ",
        AppLanguage.German  => "Heute \u2014 ",
        AppLanguage.French  => "Aujourd'hui \u2014 ",
        _                   => "Today \u2014 "
    };

    public static string RemainingOf(int remaining, int total) => _language switch
    {
        AppLanguage.Spanish => $"({remaining} restantes de {total})",
        AppLanguage.German  => $"({remaining} von {total} offen)",
        AppLanguage.French  => $"({remaining} restantes sur {total})",
        _                   => $"({remaining} remaining of {total})"
    };

    public static string CountLabel(int remaining, int total, int dates) => _language switch
    {
        AppLanguage.Spanish => $"{remaining} restantes de {total} en {dates} fecha(s)",
        AppLanguage.German  => $"{remaining} von {total} offen in {dates} Datum/Daten",
        AppLanguage.French  => $"{remaining} restantes sur {total} sur {dates} date(s)",
        _                   => $"{remaining} remaining of {total} across {dates} date(s)"
    };

    // ─── Settings Window ────────────────────────────────────────────────────

    public static string SettingsWindowTitle => _language switch
    {
        AppLanguage.Spanish => "Ajustes",
        AppLanguage.German  => "Einstellungen",
        AppLanguage.French  => "Paramètres",
        _                   => "Settings"
    };

    public static string SettingsTitleLabel => _language switch
    {
        AppLanguage.Spanish => "⚙ Ajustes",
        AppLanguage.German  => "⚙ Einstellungen",
        AppLanguage.French  => "⚙ Paramètres",
        _                   => "⚙ Settings"
    };

    public static string SettingsSubtitle => _language switch
    {
        AppLanguage.Spanish => "Personaliza cómo se comporta YATODOL",
        AppLanguage.German  => "Passen Sie YATODOL Ihren Wünschen an",
        AppLanguage.French  => "Personnalisez le comportement de YATODOL",
        _                   => "Customize how YATODOL behaves"
    };

    public static string ButtonSave => _language switch
    {
        AppLanguage.Spanish => "Guardar",
        AppLanguage.German  => "Speichern",
        AppLanguage.French  => "Enregistrer",
        _                   => "Save"
    };

    public static string SectionAppearance => _language switch
    {
        AppLanguage.Spanish => "Apariencia",
        AppLanguage.German  => "Erscheinungsbild",
        AppLanguage.French  => "Apparence",
        _                   => "Appearance"
    };

    public static string ThemeLightLabel => _language switch
    {
        AppLanguage.Spanish => "☀ Claro",
        AppLanguage.German  => "☀ Hell",
        AppLanguage.French  => "☀ Clair",
        _                   => "☀ Light"
    };

    public static string ThemeDarkLabel => _language switch
    {
        AppLanguage.Spanish => "🌙 Oscuro",
        AppLanguage.German  => "🌙 Dunkel",
        AppLanguage.French  => "🌙 Sombre",
        _                   => "🌙 Dark"
    };

    public static string ThemeSystemLabel => _language switch
    {
        AppLanguage.Spanish => "💻 Sistema",
        AppLanguage.German  => "💻 System",
        AppLanguage.French  => "💻 Système",
        _                   => "💻 System"
    };

    public static string SectionGeneral => _language switch
    {
        AppLanguage.Spanish => "General",
        AppLanguage.German  => "Allgemein",
        AppLanguage.French  => "Général",
        _                   => "General"
    };

    public static string ShowPathLabel => _language switch
    {
        AppLanguage.Spanish => "Mostrar ruta del archivo en la barra de título",
        AppLanguage.German  => "Dateipfad in der Titelleiste anzeigen",
        AppLanguage.French  => "Afficher le chemin dans la barre de titre",
        _                   => "Show file path in title bar"
    };

    public static string ShowPathDesc => _language switch
    {
        AppLanguage.Spanish => "Muestra la ubicación del archivo de datos en el título",
        AppLanguage.German  => "Zeigt den Speicherort der Datendatei im Fenstertitel an",
        AppLanguage.French  => "Affiche l'emplacement du fichier de données dans le titre",
        _                   => "Displays the data file location in the window title"
    };

    public static string HideCompletedLabel => _language switch
    {
        AppLanguage.Spanish => "Ocultar fechas completadas",
        AppLanguage.German  => "Abgeschlossene Daten ausblenden",
        AppLanguage.French  => "Masquer les dates entièrement terminées",
        _                   => "Hide fully completed dates"
    };

    public static string HideCompletedDesc => _language switch
    {
        AppLanguage.Spanish => "Las fechas con todas las tareas completadas se ocultarán",
        AppLanguage.German  => "Daten, bei denen alle Aufgaben erledigt sind, werden ausgeblendet",
        AppLanguage.French  => "Les dates où toutes les tâches sont terminées seront masquées",
        _                   => "Dates where all tasks are done will be hidden from the list"
    };

    public static string CarryForwardLabel => _language switch
    {
        AppLanguage.Spanish => "Trasladar tareas incompletas",
        AppLanguage.German  => "Unerledigte Aufgaben übertragen",
        AppLanguage.French  => "Reporter les tâches non terminées",
        _                   => "Carry forward uncompleted tasks"
    };

    public static string CarryForwardDesc => _language switch
    {
        AppLanguage.Spanish => "Mueve las tareas pasadas incompletas a hoy automáticamente",
        AppLanguage.German  => "Verschiebt vergangene unerledigte Aufgaben automatisch auf heute",
        AppLanguage.French  => "Déplace automatiquement les tâches non terminées à aujourd'hui",
        _                   => "Moves past uncompleted tasks to today automatically at midnight"
    };

    public static string SectionPrinting => _language switch
    {
        AppLanguage.Spanish => "🖨 Impresión",
        AppLanguage.German  => "🖨 Drucken",
        AppLanguage.French  => "🖨 Impression",
        _                   => "🖨 Printing"
    };

    public static string PrintScopeLabel => _language switch
    {
        AppLanguage.Spanish => "Alcance",
        AppLanguage.German  => "Bereich",
        AppLanguage.French  => "Portée",
        _                   => "Scope"
    };

    public static string PrintScopeSelected => _language switch
    {
        AppLanguage.Spanish => "Solo la fecha seleccionada",
        AppLanguage.German  => "Nur ausgewähltes Datum",
        AppLanguage.French  => "Date sélectionnée uniquement",
        _                   => "Selected date only"
    };

    public static string PrintScopeAll => _language switch
    {
        AppLanguage.Spanish => "Todas las fechas",
        AppLanguage.German  => "Alle Daten",
        AppLanguage.French  => "Toutes les dates",
        _                   => "All dates"
    };

    public static string PrintFilterLabel => _language switch
    {
        AppLanguage.Spanish => "Filtro",
        AppLanguage.German  => "Filter",
        AppLanguage.French  => "Filtre",
        _                   => "Filter"
    };

    public static string PrintFilterAll => _language switch
    {
        AppLanguage.Spanish => "Todos los elementos (activos + completados)",
        AppLanguage.German  => "Alle Einträge (aktiv + abgeschlossen)",
        AppLanguage.French  => "Tous les éléments (actifs + terminés)",
        _                   => "All items (active + completed)"
    };

    public static string PrintFilterRemaining => _language switch
    {
        AppLanguage.Spanish => "Solo pendientes (ocultar completados)",
        AppLanguage.German  => "Nur verbleibende (abgeschlossene ausblenden)",
        AppLanguage.French  => "Restants uniquement (masquer terminés)",
        _                   => "Remaining only (hide completed)"
    };

    public static string SectionLanguage => _language switch
    {
        AppLanguage.Spanish => "Idioma",
        AppLanguage.German  => "Sprache",
        AppLanguage.French  => "Langue",
        _                   => "Language"
    };

    public static string LanguagePickerLabel => _language switch
    {
        AppLanguage.Spanish => "Idioma de la interfaz",
        AppLanguage.German  => "Sprache der Benutzeroberfläche",
        AppLanguage.French  => "Langue de l'interface",
        _                   => "Interface language"
    };

    // ─── Data Storage Settings ──────────────────────────────────────────────

    public static string SectionStorage => _language switch
    {
        AppLanguage.Spanish => "Almacenamiento de datos",
        AppLanguage.German  => "Datenspeicherung",
        AppLanguage.French  => "Stockage des données",
        _                   => "Data Storage"
    };

    public static string CustomPathLabel => _language switch
    {
        AppLanguage.Spanish => "Usar carpeta de datos personalizada",
        AppLanguage.German  => "Benutzerdefinierten Datenordner verwenden",
        AppLanguage.French  => "Utiliser un dossier de données personnalisé",
        _                   => "Use custom data folder"
    };

    public static string CustomPathDesc => _language switch
    {
        AppLanguage.Spanish => "Guarda todos.json en una ubicación personalizada (p. ej. una carpeta de red compartida)",
        AppLanguage.German  => "Speichert todos.json an einem benutzerdefinierten Ort (z. B. ein gemeinsamer Netzwerkordner)",
        AppLanguage.French  => "Stocke todos.json dans un emplacement personnalisé (ex. un dossier réseau partagé)",
        _                   => "Store todos.json in a custom location (e.g. a shared network folder)"
    };

    public static string ButtonBrowse => _language switch
    {
        AppLanguage.Spanish => "📁 Explorar",
        AppLanguage.German  => "📁 Durchsuchen",
        AppLanguage.French  => "📁 Parcourir",
        _                   => "📁 Browse"
    };

    public static string CustomPathPlaceholder => _language switch
    {
        AppLanguage.Spanish => "Selecciona una carpeta...",
        AppLanguage.German  => "Ordner auswählen...",
        AppLanguage.French  => "Sélectionner un dossier...",
        _                   => "Select a folder..."
    };

    public static string BrowseFolderTitle => _language switch
    {
        AppLanguage.Spanish => "Seleccionar carpeta de datos",
        AppLanguage.German  => "Datenordner auswählen",
        AppLanguage.French  => "Sélectionner le dossier de données",
        _                   => "Select data folder"
    };

    // ─── About Window ───────────────────────────────────────────────────────

    public static string AboutWindowTitle => _language switch
    {
        AppLanguage.Spanish => "Acerca de YaTODOL",
        AppLanguage.German  => "Über YaTODOL",
        AppLanguage.French  => "À propos de YaTODOL",
        _                   => "About YaTODOL"
    };

    public static string AboutSubtitle => _language switch
    {
        AppLanguage.Spanish => "Otra lista de tareas pendientes",
        AppLanguage.German  => "Noch eine Aufgabenliste",
        AppLanguage.French  => "Encore une liste de tâches",
        _                   => "Yet another todo list"
    };

    public static string ButtonClose => _language switch
    {
        AppLanguage.Spanish => "Cerrar",
        AppLanguage.German  => "Schließen",
        AppLanguage.French  => "Fermer",
        _                   => "Close"
    };

    // ─── iCal Export Window ─────────────────────────────────────────────────

    public static string ICalWindowTitle => _language switch
    {
        AppLanguage.Spanish => "Exportar a iCal",
        AppLanguage.German  => "Als iCal exportieren",
        AppLanguage.French  => "Exporter en iCal",
        _                   => "Export to iCal"
    };

    public static string ICalTitle => _language switch
    {
        AppLanguage.Spanish => "📅 Exportar a iCal",
        AppLanguage.German  => "📅 Als iCal exportieren",
        AppLanguage.French  => "📅 Exporter en iCal",
        _                   => "📅 Export to iCal"
    };

    public static string ICalSubtitle => _language switch
    {
        AppLanguage.Spanish => "Selecciona las tareas a exportar como eventos .ics",
        AppLanguage.German  => "Aufgaben als .ics-Kalendereinträge exportieren",
        AppLanguage.French  => "Sélectionner les tâches à exporter en événements .ics",
        _                   => "Select tasks to export as .ics calendar events"
    };

    public static string ButtonSelectAll => _language switch
    {
        AppLanguage.Spanish => "Seleccionar todo",
        AppLanguage.German  => "Alle auswählen",
        AppLanguage.French  => "Tout sélectionner",
        _                   => "Select All"
    };

    public static string ButtonSelectNone => _language switch
    {
        AppLanguage.Spanish => "Deseleccionar todo",
        AppLanguage.German  => "Keine auswählen",
        AppLanguage.French  => "Ne rien sélectionner",
        _                   => "Select None"
    };

    public static string ButtonUncompletedOnly => _language switch
    {
        AppLanguage.Spanish => "Solo pendientes",
        AppLanguage.German  => "Nur offene",
        AppLanguage.French  => "Non terminées",
        _                   => "Uncompleted Only"
    };

    // ─── Print HTML ─────────────────────────────────────────────────────────

    public static string PrintAllDates => _language switch
    {
        AppLanguage.Spanish => "Todas las fechas",
        AppLanguage.German  => "Alle Daten",
        AppLanguage.French  => "Toutes les dates",
        _                   => "All dates"
    };

    public static string PrintRemainingOnly => _language switch
    {
        AppLanguage.Spanish => " \u2014 solo pendientes",
        AppLanguage.German  => " \u2014 nur verbleibende",
        AppLanguage.French  => " \u2014 restants uniquement",
        _                   => " \u2014 remaining only"
    };

    public static string PrintNoTasks => _language switch
    {
        AppLanguage.Spanish => "No hay tareas para mostrar.",
        AppLanguage.German  => "Keine Aufgaben anzuzeigen.",
        AppLanguage.French  => "Aucune tâche à afficher.",
        _                   => "No tasks to display."
    };

    public static string PrintSectionActive => _language switch
    {
        AppLanguage.Spanish => "Activas",
        AppLanguage.German  => "Aktiv",
        AppLanguage.French  => "Actives",
        _                   => "Active"
    };

    public static string PrintSectionCompleted => _language switch
    {
        AppLanguage.Spanish => "Completadas",
        AppLanguage.German  => "Abgeschlossen",
        AppLanguage.French  => "Terminées",
        _                   => "Completed"
    };

    public static string PrintTotal => _language switch
    {
        AppLanguage.Spanish => "Total",
        AppLanguage.German  => "Gesamt",
        AppLanguage.French  => "Total",
        _                   => "Total"
    };

    public static string PrintActive => _language switch
    {
        AppLanguage.Spanish => "Activas",
        AppLanguage.German  => "Aktiv",
        AppLanguage.French  => "Actives",
        _                   => "Active"
    };

    public static string PrintCompleted => _language switch
    {
        AppLanguage.Spanish => "Completadas",
        AppLanguage.German  => "Abgeschlossen",
        AppLanguage.French  => "Terminées",
        _                   => "Completed"
    };

    // ─── Note Editor Window ─────────────────────────────────────────────────

    public static string NoteEditorTitle => _language switch
    {
        AppLanguage.Spanish => "Nota de la tarea",
        AppLanguage.German  => "Aufgabennotiz",
        AppLanguage.French  => "Note de la tâche",
        _                   => "Task Note"
    };

    public static string NoteEditorSubtitle(string task) => _language switch
    {
        AppLanguage.Spanish => $"Nota para: {task}",
        AppLanguage.German  => $"Notiz für: {task}",
        AppLanguage.French  => $"Note pour : {task}",
        _                   => $"Note for: {task}"
    };

    public static string NoteDeleteButton => _language switch
    {
        AppLanguage.Spanish => "Eliminar nota",
        AppLanguage.German  => "Notiz löschen",
        AppLanguage.French  => "Supprimer la note",
        _                   => "Delete Note"
    };

    public static string TooltipNote => _language switch
    {
        AppLanguage.Spanish => "Nota",
        AppLanguage.German  => "Notiz",
        AppLanguage.French  => "Note",
        _                   => "Note"
    };

    public static string TooltipDragReorder => _language switch
    {
        AppLanguage.Spanish => "Arrastrar para reordenar",
        AppLanguage.German  => "Ziehen zum Neuordnen",
        AppLanguage.French  => "Glisser pour réorganiser",
        _                   => "Drag to reorder"
    };

    public static string TooltipRename => _language switch
    {
        AppLanguage.Spanish => "Doble clic para renombrar",
        AppLanguage.German  => "Doppelklicken zum Umbenennen",
        AppLanguage.French  => "Double-cliquer pour renommer",
        _                   => "Double-click to rename"
    };

    public static string SaveFailed => _language switch
    {
        AppLanguage.Spanish => "No se pudo guardar. El archivo no es accesible.",
        AppLanguage.German  => "Speichern fehlgeschlagen. Die Datei ist nicht erreichbar.",
        AppLanguage.French  => "Échec de la sauvegarde. Le fichier est inaccessible.",
        _                   => "Could not save. The file is not accessible."
    };

    public static string NotePlaceholder => _language switch
    {
        AppLanguage.Spanish => "Escribe tu nota aquí...",
        AppLanguage.German  => "Schreibe hier deine Notiz...",
        AppLanguage.French  => "Écrivez votre note ici...",
        _                   => "Write your note here..."
    };
}
