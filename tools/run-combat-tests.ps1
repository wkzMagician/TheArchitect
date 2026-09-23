$ErrorActionPreference='Stop'
$repo=Split-Path -Parent $PSScriptRoot
$buildOut=Join-Path $repo '.artifacts/build'
$godot='D:/godot_v4.5.1/Godot_v4.5.1-stable_mono_win64.exe'
$gameProject='D:/game/sts2/Slay the Spire 2'
$mods='D:/godot_v4.5.1/mods/TheArchitect'
$report=Join-Path $repo 'TheArchitect-combat-tests.log'
# Build and deploy the whole mod (DLL + PCK) so the tests exercise the current source
# and resources, then write the DLL where the test project references it from.
& dotnet build $repo/TheArchitect.csproj --no-restore "-p:OutputPath=$buildOut\" -p:ExportMod=true
if ($LASTEXITCODE -ne 0) { throw 'Building the mod failed. See the output above.' }
& dotnet build $repo/Tests/TheArchitect.Tests.csproj --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Building the test project failed. See the output above.' }
Copy-Item $repo/Tests/bin/Debug/net9.0/TheArchitect.Tests.dll "$mods/TheArchitect.Tests.dll" -Force
$env:THEARCHITECT_RUN_TESTS='1'
$env:THEARCHITECT_TEST_ASSEMBLY="$mods/TheArchitect.Tests.dll"
$env:THEARCHITECT_TEST_REPORT=$report
$env:THEARCHITECT_REPO_ROOT=$repo
$env:THEARCHITECT_GAME_PROJECT=$gameProject
Set-Content $report ('STATUS ' + (Get-Date -Format o) + ' launcher-started')
# Run the game project directly. Driving the editor with F5 leaves an editor process
# behind, which locks the built DLLs and breaks the next run.
$game=Start-Process $godot -ArgumentList @('--path', "`"$gameProject`"") -WindowStyle Hidden -PassThru
try {
    $deadline=(Get-Date).AddSeconds(600)
    while ((Get-Date) -lt $deadline) {
        Start-Sleep 5
        if ((Test-Path $report) -and (Select-String -Path $report -Pattern '^Executed \d+ tests' -Quiet)) { break }
        if ($game.HasExited) { break }
        $game.Refresh()
    }
    $lines=Select-String -Path $report -Pattern '^(Executed \d+ tests|FAIL |STATUS )' | ForEach-Object { $_.Line }
    $lines | Write-Output
    if (!(Select-String -Path $report -Pattern '^Executed \d+ tests: \d+ passed, 0 failed\.$' -Quiet)) {
        throw 'Combat tests did not finish cleanly. See the report above.'
    }
} finally {
    if (!$game.HasExited) { $game.Kill() }
}
