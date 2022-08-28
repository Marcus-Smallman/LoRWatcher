$HostsFilePath = "$Env:WinDir\System32\drivers\etc\hosts"

$HostsContent = Get-Content -Path $HostsFilePath

$Domain = "lor.watcher"
$Value = "`n# LoR Watcher`n`t127.0.0.1`t$Domain"

$Found = $False
foreach ($Line in $HostsContent)
{
    if ($Line.EndsWith($Domain))
    {
        $Found = $True

        break;
    }
}

if ($Found -eq $False)
{
    Add-Content -Path $HostsFilePath -Value $Value -Force
}

