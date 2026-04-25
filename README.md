# YaTODOL

**Yet Another To Do List** — a cross-platform todo/task manager built with [Avalonia UI](https://avaloniaui.net/) and .NET 10.

![License](https://img.shields.io/github/license/totopoloco/YaTODOL)
![Build](https://img.shields.io/github/actions/workflow/status/totopoloco/YaTODOL/dotnet-desktop.yml?branch=main)

## Features

- **Date-based task organization** — tasks grouped by date with expandable accordion sections
- **Drag-and-drop reordering** — rearrange tasks within a date by dragging them to any position
- **Task notes** — attach plain-text notes to any task, visible in print output
- **Carry forward** — automatically moves uncompleted past tasks to today at midnight
- **Multi-language support** — English, Spanish, German, and French
- **iCalendar export** — export selected tasks as `.ics` calendar events
- **Print support** — configurable scope (selected date or all) with filters; includes notes and color-coded tag chips
- **Theme support** — Light, Dark, and System theme modes
- **Import/Export** — backup and restore your data
- **Date navigation** — previous/next day, jump to today, calendar date picker
- **Settings** — hide completed dates, show file path in title, print preferences, language selection
- **Custom data folder** — store todos.json in any location, including shared network folders
- **Color-coded tags** — assign tags to tasks to categorize and highlight them at a glance
  - Three built-in tags (**Urgent**, **Important**, **Low**) are always available and mutually exclusive per task
  - Custom tags can be created in Settings with a name and color from the palette
- **Tag filtering** — click tag chips in the sidebar to narrow the task list to matching tasks
- **Search** — real-time search bar above the task list; supports plain text, wildcard patterns (`*`, `?`), and full regular expressions; searches both task titles and note content
- **Two-panel layout** — collapsible sidebar groups Navigation, Tasks, and App actions; main area is dedicated to task input and the task list

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Build & Run

```bash
git clone git@github.com:totopoloco/YaTODOL.git
cd YaTODOL
dotnet restore
dotnet run
```

### Windows first-time setup

If you get a `No NuGet sources are defined or enabled` error, add the default feed and install the Avalonia templates:

```powershell
dotnet nuget add source https://api.nuget.org/v3/index.json --name nuget.org
dotnet new install Avalonia.Templates
```

## Downloads

Pre-built packages are available from [GitHub Actions](https://github.com/totopoloco/YaTODOL/actions) artifacts:

### Linux

Download the `.deb` package for your architecture (amd64 or arm64) and install it:

```bash
sudo dpkg -i yatodol_<version>_amd64.deb
```

The app will appear in the Applications menu after installation.

### Windows

Two artifacts are available for each architecture (x64, arm64):

| Artifact | Description |
|----------|-------------|
| `yatodol-setup-<version>-win-<arch>.exe` | Inno Setup installer — adds a Start Menu entry and an optional desktop shortcut |
| `YATODOL.exe` | Portable single-file executable — run it from any folder, no installation needed |

Both are installed to `%USERPROFILE%\YaTODOL` (no admin rights required for the installer).

#### SmartScreen warnings

Because the binaries are not signed with a trusted code-signing certificate, Windows may block them. If you see a **"Windows protected your PC"** SmartScreen dialog, or if the file is silently blocked after download, use one of the following workarounds:

**Option 1 — Unblock via File Explorer**
1. Right-click the downloaded file → **Properties**
2. On the **General** tab, check **Unblock** at the bottom
3. Click **OK**, then run the file

**Option 2 — Unblock via PowerShell**
```powershell
Unblock-File -Path .\yatodol-setup-<version>-win-x64.exe
```

**Option 3 — Bypass SmartScreen at runtime**  
In the SmartScreen dialog click **More info** → **Run anyway**.

## Data Storage

Application data is stored in:

| Platform | Path |
|----------|------|
| Windows  | `%APPDATA%\YATODOL\` |
| Linux    | `~/.config/YATODOL/` |
| macOS    | `~/Library/Application Support/YATODOL/` |

Files: `todos.json` (tasks), `settings.json` (preferences).

The default path can be overridden in Settings to use a custom folder (e.g. a shared network drive).

## Contributing

Contributions are welcome! Here's how to get started:

1. **Fork** the repository
2. **Create a branch** for your feature or fix:
   ```bash
   git checkout -b feature/my-feature
   ```
3. **Make your changes** and ensure the project builds:
   ```bash
   dotnet build
   ```
4. **Commit** with a clear message:
   ```bash
   git commit -m "Add my feature"
   ```
5. **Push** your branch and open a **Pull Request**

### Guidelines

- Keep changes focused — one feature or fix per PR
- Follow existing code style and naming conventions
- Test on at least one platform before submitting

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
