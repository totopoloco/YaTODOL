; Inno Setup script for YaTODOL
; MyAppVersion and MyAppArch are passed from the command line via /D flags.

#define MyAppName "YaTODOL"
#define MyAppExeName "YATODOL.exe"

[Setup]
AppId={{C7E9F2A3-B8D4-4E61-9F12-3A5C7D8E9F01}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=Marco Tulio Ávila Cerón
AppPublisherURL=https://github.com/totopoloco/YaTODOL
AppUpdatesURL=https://github.com/totopoloco/YaTODOL/releases
; Install to %USERPROFILE%\YaTODOL — no admin rights required
DefaultDirName={%USERPROFILE}\{#MyAppName}
PrivilegesRequired=lowest
OutputBaseFilename=yatodol-setup-{#MyAppVersion}-win-{#MyAppArch}
OutputDir=..\..\installer-output
SetupIconFile=..\..\app-icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
