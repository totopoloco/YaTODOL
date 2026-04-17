# YaTODOL

**Yet Another To Do List** — a cross-platform todo/task manager built with [Avalonia UI](https://avaloniaui.net/) and .NET 10.

![License](https://img.shields.io/github/license/totopoloco/YaTODOL)
![Build](https://img.shields.io/github/actions/workflow/status/totopoloco/YaTODOL/dotnet-desktop.yml?branch=main)

## Features

- **Date-based task organization** — tasks grouped by date with expandable accordion sections
- **Task notes** — attach plain-text notes to any task, visible in print output
- **Carry forward** — automatically moves uncompleted past tasks to today at midnight
- **Multi-language support** — English, Spanish, German, and French
- **iCalendar export** — export selected tasks as `.ics` calendar events
- **Print support** — configurable scope (selected date or all) with filters, includes notes
- **Theme support** — Light, Dark, and System theme modes
- **Import/Export** — backup and restore your data
- **Date navigation** — previous/next day, jump to today, calendar date picker
- **Settings** — hide completed dates, show file path in title, print preferences, language selection

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

- **Windows** — self-contained `.exe` (x64, arm64)
- **Linux** — `.deb` package (amd64, arm64)

## Data Storage

Application data is stored in:

| Platform | Path |
|----------|------|
| Windows  | `%APPDATA%\YATODOL\` |
| Linux    | `~/.config/YATODOL/` |
| macOS    | `~/Library/Application Support/YATODOL/` |

Files: `todos.json` (tasks), `settings.json` (preferences).

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
