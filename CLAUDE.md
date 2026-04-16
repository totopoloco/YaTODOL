# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

YaTODOL (Yet Another To Do List) is a cross-platform desktop task manager built with C# (.NET 10) and [Avalonia UI](https://avaloniaui.net/). It organizes tasks by date with accordion sections, carries forward uncompleted past tasks at midnight, and supports iCalendar export, printing, and import/export for backup.

## Commands

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run in development
dotnet run

# Publish self-contained release
dotnet publish YATODOL.csproj -c Release -r <RID> --self-contained true
# RIDs: win-x64, win-arm64, linux-x64, linux-arm64, osx-x64, osx-arm64
```

There are no linting or automated tests — the CI pipeline verifies compilation only.

## Architecture

The app is a small, monolithic GUI application. All core logic lives in `MainWindow.axaml.cs`.

### Key Files

| File | Purpose |
|------|---------|
| `Program.cs` | Entry point; enables Avalonia DevTools in Debug builds |
| `App.axaml` / `App.axaml.cs` | Applies Fluent theme, creates MainWindow |
| `MainWindow.axaml` / `MainWindow.axaml.cs` | Primary window with all task management logic |
| `TodoItem.cs` | Data model; implements `INotifyPropertyChanged` for binding |
| `AppSettings.cs` | Enums and settings POCO (theme, print options, general prefs) |
| `BoolConverters.cs` | Avalonia value converters (strikethrough, opacity) |
| `SettingsWindow.axaml.cs` | Modal for appearance and print settings |
| `AboutWindow.axaml.cs` | Displays version and release notes from `RELEASE.md` |
| `ICalExportWindow.axaml.cs` | Task selection UI for `.ics` export |

### Data Flow in MainWindow

```
Load() → loads todos.json
LoadSettings() → loads settings.json
CarryForwardPastTasks() → moves uncompleted past tasks to today (if enabled)
RebuildAccordion() → builds date-grouped Expander controls from _allItems
ScheduleMidnightCheck() → timer fires at midnight to repeat carry-forward

User actions (Add/Delete/Complete/Navigate) → Save() → writes todos.json
```

### State

A single `ObservableCollection<TodoItem> _allItems` in MainWindow holds all tasks. The accordion UI is rebuilt from scratch on every relevant change via `RebuildAccordion()`. Settings are a `AppSettings` instance loaded once at startup; written back on any settings change.

### Persistence Paths

| Platform | Path |
|----------|------|
| Windows | `%APPDATA%\YATODOL\` |
| Linux | `~/.config/YATODOL/` |
| macOS | `~/Library/Application Support/YATODOL/` |

Files: `todos.json` (task list) and `settings.json` (app settings). Serialized with `System.Text.Json`.

### Modal Dialogs

Secondary windows (`SettingsWindow`, `AboutWindow`, `ICalExportWindow`) are opened with `ShowDialog<T>()` and return results to the caller. They have no persistent state of their own.

## CI/CD

`.github/workflows/dotnet-desktop.yml` builds all platform/configuration combinations in a matrix. On pushes to `main`:
- Produces self-contained Windows `.exe` artifacts (x64, arm64)
- Produces `.deb` packages for Linux (amd64, arm64) using scripts in `packaging/linux/`
- Version is extracted from the first `## X.Y.Z` heading in `RELEASE.md`

When bumping the version, update `RELEASE.md` — this is the single source of truth for the version number used in packaging.
