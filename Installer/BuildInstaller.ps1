# In order to run this script the Inno Setup Command-Line Compiler needs to be accessible to the terminal.
# This can be done by installing Inno Setup 'https://jrsoftware.org/isinfo.php' and adding the install path
# (defaults to 'C:\Program Files (x86)\Inno Setup 6') to your system environment.
# The dotnet SDK and runtime that supports 3.1 or above also needs to be installed.

# Get Version
$ProjectXML = [Xml](Get-Content ..\LoRWatcher\LoRWatcher.csproj)
$Version = [string]$ProjectXML.Project.PropertyGroup.Version
$Version = $Version.Trim()

Write-Host "Building installer for version ${Version} of the LoR Watcher"

# Build/Publish Source
if (Test-Path Publish)
{
    Remove-Item Publish -Force -Recurse
}

dotnet publish ..\LoRWatcher\LoRWatcher.csproj --output Publish --configuration Release --runtime win-x86 --framework net5.0-windows --self-contained

# Build Installer
if (Test-Path Output)
{
    Remove-Item Output -Force -Recurse
}

ISCC.exe installer.iss /DVersion=$Version