; TODO:
; Clean up LoR Watcher DB if exists on uninstall
; Clean up LoR Watcher Logs if exists on uninstall

[Setup]
AppName=LoR Watcher
AppVersion={#Version}
AppPublisher=Marcus Smallman
AppPublisherURL=https://github.com/Marcus-Smallman/LoRWatcher
WizardStyle=modern
DefaultDirName={autopf}\LoR Watcher
DefaultGroupName=LoR Watcher
UninstallDisplayName=LoR Watcher ({#Version})
UninstallDisplayIcon={app}\LoR Watcher.exe
Compression=lzma2
SolidCompression=yes
OutputBaseFilename=LoR Watcher Installer
VersionInfoVersion={#Version}
InfoBeforeFile=info.txt

[Files]
Source: "Publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\LoR Watcher"; Filename: "{app}\LoR Watcher.exe"

[Run]
Filename: {app}\LoR Watcher.exe; Description: Start the LoR Watcher; Flags: nowait postinstall skipifsilent
