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
UninstallDisplayName=LoR Watcher {#Version}
UninstallDisplayIcon={app}\LoR Watcher.exe
Compression=lzma2
SolidCompression=yes
OutputBaseFilename=LoR Watcher Installer {#Version}
VersionInfoVersion={#Version}
InfoBeforeFile=info.txt

[Dirs]
Name: "{app}\wwwroot\assets"

[Files]
Source: "unzip.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall
Source: "{tmp}\core-en_us.zip"; DestDir: "{tmp}"; Flags: external
Source: "{tmp}\set1-lite-en_us.zip"; DestDir: "{tmp}"; Flags: external
Source: "{tmp}\set2-lite-en_us.zip"; DestDir: "{tmp}"; Flags: external
Source: "{tmp}\set3-lite-en_us.zip"; DestDir: "{tmp}"; Flags: external
Source: "{tmp}\set4-lite-en_us.zip"; DestDir: "{tmp}"; Flags: external
Source: "{tmp}\set5-lite-en_us.zip"; DestDir: "{tmp}"; Flags: external
Source: "{tmp}\set6-lite-en_us.zip"; DestDir: "{tmp}"; Flags: external
Source: "Publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\LoR Watcher"; Filename: "{app}\LoR Watcher.exe"

[Run]
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\core-en_us.zip"" -d ""{app}\wwwroot\assets\core-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set1-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set1-lite-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set2-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set2-lite-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set3-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set3-lite-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set4-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set4-lite-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set5-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set5-lite-en_us"""; Flags: runhidden
Filename: "{tmp}\unzip.exe"; Parameters: "-o ""{tmp}\set6-lite-en_us.zip"" -d ""{app}\wwwroot\assets\set6-lite-en_us"""; Flags: runhidden
Filename: {app}\LoR Watcher.exe; Description: Start the LoR Watcher; Flags: nowait postinstall skipifsilent

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
    DownloadPage.Add('https://dd.b.pvp.net/1_0_0/core-en_us.zip', 'core-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/1_0_0/set1-lite-en_us.zip', 'set1-lite-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/1_0_0/set2-lite-en_us.zip', 'set2-lite-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/1_8_0/set3-lite-en_us.zip', 'set3-lite-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/2_3_0/set4-lite-en_us.zip', 'set4-lite-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/2_14_0/set5-lite-en_us.zip', 'set5-lite-en_us.zip', '');
    DownloadPage.Add('https://dd.b.pvp.net/3_8_0/set6-lite-en_us.zip', 'set6-lite-en_us.zip', '');
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