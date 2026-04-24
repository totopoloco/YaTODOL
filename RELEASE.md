## 1.4.0

- Added tag support — assign color-coded tags to tasks to categorize and highlight them
- Three built-in tags (Urgent, Important, Low) are always available and cannot be deleted
- Built-in tags are mutually exclusive per task; only one may be assigned at a time
- Custom tags can be created in Settings with a name and a color from the picker palette
- Custom tags can be deleted unless they are currently assigned to at least one task
- Tags are shown as colored chips on each task row in the main list
- Tags are selectable in the note/tag editor dialog alongside the note text
- Tags are included in print output as colored pills next to each task title
- Tags are localized in all supported languages (English, Spanish, German, French)
- Improved toolbar and note button icons: color-coded circular badge design for all action buttons
- Softened overall color palette — all UI colors are now muted and easier on the eye

## 1.3.0

- Added drag-and-drop task reordering — rearrange tasks within a date by dragging the grip handle
- Custom task order is persisted across sessions
- Visual drop indicator shows insertion point during drag
- Localized drag tooltip in all supported languages (English, Spanish, German, French)

## 1.2.0

- Added custom data folder option — store todos.json in any location (e.g. a shared network folder)
- Data file is automatically moved when changing the storage path
- New "Data Storage" section in Settings with folder picker
- Localized in all supported languages (English, Spanish, German, French)

## 1.1.0

- Added multi-language support (English, Spanish, German, French) with language picker in Settings
- Added task notes — attach plain-text notes to any task via the note editor dialog
- Notes are included in print output with styled formatting
- Tasks now stay in place when completed (no longer move to the bottom)
- Cannot add tasks to past dates — shows a warning that auto-dismisses after 4 seconds
- Application icon now appears in all dialog windows
- Refactored codebase: extracted DataService, PrintService, ICalService, DialogService, and AccordionBuilder from MainWindow

## 1.0.1

- Added About dialog showing version and release notes
- Fixed .deb package: app now appears in Ubuntu Apps and shows icon in the dock
- Improved .desktop file with StartupWMClass and Keywords
- Build artifacts now use version from RELEASE.md
- Added post-install/post-remove scripts for reliable GNOME integration
- Added cross-platform CI builds (Linux, Windows, macOS)
- Added .deb packaging for amd64 and arm64

## 1.0.0

- Initial version with basic functionality.