param(
    [string]$GodotPath = 'D:/godot_v4.5.1/Godot_v4.5.1-stable_mono_win64.exe',
    [switch]$VerifyOnly
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)
$OutputEncoding = [Console]::OutputEncoding
$projectRoot = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $projectRoot
$logPath = Join-Path $projectRoot '.artifacts/launch-sts2.log'
New-Item -ItemType Directory -Force (Split-Path $logPath) | Out-Null
try {
    if (!(Test-Path -LiteralPath $GodotPath)) { throw "Godot executable not found: $GodotPath" }
    $version = & $GodotPath --version
    if ($version -notmatch '^4\.5\.1\..*mono') { throw "Use Godot 4.5.1 .NET. Found: $version" }
    if (Get-Process SlayTheSpire2 -ErrorAction SilentlyContinue) {
        throw 'Close Slay the Spire 2 before rebuilding its mod.'
    }
    & dotnet build TheArchitect.csproj '-p:ExportMod=true' "-p:GodotPath=$GodotPath" *> $logPath
    if ($LASTEXITCODE -ne 0) { throw "Build/export failed. See $logPath" }
    if (Select-String -LiteralPath $logPath -Pattern 'ERROR:|SCRIPT ERROR:|CrashHandlerException' -Quiet) {
        throw "Godot reported an export error. See $logPath"
    }
    $gamePath = (& dotnet msbuild TheArchitect.csproj -getProperty:Sts2Path | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Could not resolve Sts2Path.' }
    $gameExe = Join-Path $gamePath 'SlayTheSpire2.exe'
    if (!(Test-Path -LiteralPath $gameExe)) { throw "Game executable not found: $gameExe" }
    $modPath = Join-Path $gamePath 'mods/TheArchitect'
    foreach ($file in @('TheArchitect.dll', 'TheArchitect.pck', 'TheArchitect.json')) {
        if (!(Test-Path -LiteralPath (Join-Path $modPath $file))) { throw "Missing deployed mod file: $file" }
    }
    if (!$VerifyOnly) {
        # Steam normally supplies these when launching the game. Scope them to
        # this launcher process so direct editor launches can initialize Steam.
        $env:SteamAppId = '2868840'
        $env:SteamGameId = '2868840'
        $startInfo = New-Object System.Diagnostics.ProcessStartInfo
        $startInfo.FileName = $gameExe
        $startInfo.WorkingDirectory = $gamePath
        $startInfo.UseShellExecute = $true
        $gameProcess = [System.Diagnostics.Process]::Start($startInfo)
        $gameProcess.Dispose()
        Write-Output 'Slay the Spire 2 started with the rebuilt TheArchitect mod.'
    } else {
        Write-Output 'Build, export, and deployment verified.'
    }
    exit 0
} catch {
    Write-Output $_.Exception.Message
    if (Test-Path -LiteralPath $logPath) { Get-Content -LiteralPath $logPath -Tail 30 }
    exit 1
}
