; Inno Setup script for YaTODOL
; MyAppVersion, MyAppArch, and MySourceDir are passed from the command line via /D flags.

; Default source directory for local builds (relative to this .iss file).
; CI overrides this to point at the signed publish output.
#ifndef MySourceDir
  #define MySourceDir "..\..\publish"
#endif

#define MyAppName "YaTODOL"
#define MyAppExeName "YATODOL.exe"

[Setup]
AppId={{C7E9F2A3-B8D4-4E61-9F12-3A5C7D8E9F01}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=Marco Tulio Ávila Cerón
AppPublisherURL=https://github.com/totopoloco/YaTODOL
AppUpdatesURL=https://github.com/totopoloco/YaTODOL/releases
AppComments=A simple to-do list that organizes tasks by date and automatically carries forward unfinished work. Open source under the MIT license.
LicenseFile=..\..\LICENSE
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
Source: "{#MySourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
