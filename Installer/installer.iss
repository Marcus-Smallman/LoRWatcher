; TODO:
; Clean up LoR Watcher DB if exists on uninstall, is user agrees
; Clean up LoR Watcher Logs if exists on uninstall
; If uninstall is executed while LoR Watcher is running, ensure it is closed before continuing

#define AppName "LoR Watcher"
#define AppExeName "LoR Watcher.exe"

[Setup]
AppName={#AppName}
AppVersion={#Version}
AppPublisher=Marcus Smallman
AppPublisherURL=https://github.com/Marcus-Smallman/LoRWatcher
WizardStyle=modern
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
UninstallDisplayName={#AppName} {#Version}
UninstallDisplayIcon={app}\{#AppExeName}
CloseApplications=force
Compression=lzma2
SolidCompression=yes
OutputBaseFilename={#AppName} Installer {#Version}
VersionInfoVersion={#Version}
InfoBeforeFile=info.txt

[Dirs]
Name: "{app}\wwwroot\assets"

[Files]
Source: "unzip.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall
Source: "UpdateHosts.ps1"; DestDir: "{tmp}"; Flags: deleteafterinstall
Source: "{tmp}\core-en_us.zip"; DestDir: "{tmp}"; Flags: deleteafterinstall external
Source: "{tmp}\set1-lite-en_us.zip"; DestDir: "{tmp}"; Flags: deleteafterinstall external
Source: "{tmp}\set2-lite-en_us.zip"; DestDir: "{tmp}"; Flags: deleteafterinstall external
Source: "{tmp}\set3-lite-en_us.zip"; DestDir: "{tmp}"; Flags: deleteafterinstall external
Source: "{tmp}\set4-lite-en_us.zip"; DestDir: "{tmp}"; Flags: deleteafterinstall external
Source: "{tmp}\set5-lite-en_us.zip"; DestDir: "{tmp}"; Flags: deleteafterinstall external
Source: "{tmp}\set6-lite-en_us.zip"; DestDir: "{tmp}"; Flags: deleteafterinstall external
Source: "{tmp}\set6cde-lite-en_us.zip"; DestDir: "{tmp}"; Flags: deleteafterinstall external
Source: "Publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"

[Run]
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\core-en_us.zip"" -d ""{app}\wwwroot\assets\core-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set1-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set1-lite-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set2-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set2-lite-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set3-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set3-lite-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set4-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set4-lite-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set5-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set5-lite-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set6-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set6-lite-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set6cde-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set6cde-lite-en_us"""; Flags: runhidden
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{tmp}\UpdateHosts.ps1"""; Flags: runhidden
Filename: {app}\{#AppExeName}; Description: Start the {#AppName}; Flags: nowait postinstall skipifsilent

[Code]
var
  DownloadPage: TDownloadWizardPage;

function OnDownloadProgress(const Url, FileName: String; const Progress, ProgressMax: Int64): Boolean;
begin
  if Progress = ProgressMax then
    Log(Format('Successfully downloaded file to {tmp}: %s', [FileName]));
  Result := True;
end;

procedure InitializeWizard;
begin
  DownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), @OnDownloadProgress);
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  if CurPageID = wpReady then begin
    DownloadPage.Clear;
    DownloadPage.Add('https://dd.b.pvp.net/latest/core-en_us.zip', 'core-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/latest/set1-lite-en_us.zip', 'set1-lite-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/latest/set2-lite-en_us.zip', 'set2-lite-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/latest/set3-lite-en_us.zip', 'set3-lite-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/latest/set4-lite-en_us.zip', 'set4-lite-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/latest/set5-lite-en_us.zip', 'set5-lite-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/latest/set6-lite-en_us.zip', 'set6-lite-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/latest/set6cde-lite-en_us.zip', 'set6cde-lite-en_us.zip', '');
    DownloadPage.Show;
    try
      try
        DownloadPage.Download; // This downloads the files to {tmp}
        Result := True;
      except
        if DownloadPage.AbortedByUser then
          Log('Aborted by user.')
        else
          SuppressibleMsgBox(AddPeriod(GetExceptionMessage), mbCriticalError, MB_OK, IDOK);
        Result := False;
      end;
    finally
      DownloadPage.Hide;
    end;
  end else
    Result := True;
end;