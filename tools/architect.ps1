param(
    [ValidateSet('build', 'test', 'run')]
    [string]$Mode = 'build',
    [string]$GodotPath = 'D:/godot_v4.5.1/Godot_v4.5.1-stable_mono_win64.exe',
    [string]$TestGameProject = 'D:/game/sts2/Slay the Spire 2',
    [string]$TestModsPath = 'D:/godot_v4.5.1/mods/TheArchitect',
    [switch]$VerifyOnly
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)
$OutputEncoding = [Console]::OutputEncoding

$repo = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $repo

$project = Join-Path $repo 'TheArchitect.csproj'
$testProject = Join-Path $repo 'Tests/TheArchitect.Tests.csproj'
$buildOut = Join-Path $repo '.artifacts/build'
$testAssembly = Join-Path $repo 'Tests/bin/Debug/net9.0/TheArchitect.Tests.dll'
$testReport = Join-Path $repo 'TheArchitect-combat-tests.log'
$launchLog = Join-Path $repo '.artifacts/launch-sts2.log'

function Invoke-ModBuild {
    param(
        [string]$OutputPath,
        [switch]$NoRestore,
        [string]$BuildGodotPath
    )

    $buildArguments = @($project)
    if ($NoRestore) {
        $buildArguments += '--no-restore'
    }
    if ($OutputPath) {
        $buildArguments += "-p:OutputPath=$OutputPath\"
    }
    $buildArguments += '-p:ExportMod=true'
    if ($BuildGodotPath) {
        $buildArguments += "-p:GodotPath=$BuildGodotPath"
    }

    & dotnet build @buildArguments
    if ($LASTEXITCODE -ne 0) {
        throw 'Building TheArchitect failed.'
    }
}

function Invoke-BuildMode {
    if (Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue) {
        throw 'Close Slay the Spire 2 before rebuilding its mod.'
    }

    Invoke-ModBuild
}

function Invoke-TestMode {
    if (Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue) {
        throw 'Close Slay the Spire 2 before running combat tests; its mod assembly is locked.'
    }

    foreach ($requiredPath in @($GodotPath, $TestGameProject, $TestModsPath)) {
        if (!(Test-Path -LiteralPath $requiredPath)) {
            throw "Required combat-test path not found: $requiredPath"
        }
    }

    $extensionList = Join-Path $TestGameProject '.godot/extension_list.cfg'
    if (Test-Path -LiteralPath $extensionList) {
        $entries = @(Get-Content -LiteralPath $extensionList)
        $spineEntries = @($entries | Where-Object { $_ -match '/spine_godot_extension\.gdextension$' })
        if ($spineEntries.Count -gt 1) {
            $activeSpine = $spineEntries | Where-Object {
                Test-Path -LiteralPath (Join-Path $TestGameProject ($_.Replace('res://', '')))
            } | Select-Object -First 1
            if (!$activeSpine) {
                throw 'No installed Spine GDExtension was found in the test game project.'
            }
            $entries | Where-Object {
                $_ -notmatch '/spine_godot_extension\.gdextension$' -or $_ -eq $activeSpine
            } | Set-Content -LiteralPath $extensionList -Encoding utf8
            Write-Output "Using one Spine GDExtension in the test game project: $activeSpine"
        }
    }

    New-Item -ItemType Directory -Force -Path $buildOut | Out-Null
    Invoke-ModBuild -OutputPath $buildOut -NoRestore -BuildGodotPath $GodotPath

    & dotnet build $testProject --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw 'Building the test assembly failed.'
    }
    if (!(Test-Path -LiteralPath $testAssembly)) {
        throw "Test assembly was not produced: $testAssembly"
    }

    $deployedTestAssembly = Join-Path $TestModsPath 'TheArchitect.Tests.dll'
    Copy-Item -LiteralPath $testAssembly -Destination $deployedTestAssembly -Force

    $env:THEARCHITECT_RUN_TESTS = '1'
    $env:THEARCHITECT_TEST_ASSEMBLY = $deployedTestAssembly
    $env:THEARCHITECT_TEST_REPORT = $testReport
    $env:THEARCHITECT_REPO_ROOT = $repo
    $env:THEARCHITECT_GAME_PROJECT = $TestGameProject
    Set-Content -LiteralPath $testReport -Value "STATUS $(Get-Date -Format o) launcher-started"

    $game = Start-Process -FilePath $GodotPath `
        -ArgumentList @('--path', "`"$TestGameProject`"") `
        -WindowStyle Hidden -PassThru

    try {
        $deadline = (Get-Date).AddSeconds(600)
        while ((Get-Date) -lt $deadline) {
            if ((Test-Path -LiteralPath $testReport) -and
                (Select-String -Path $testReport -Pattern '^Executed \d+ tests' -Quiet)) {
                break
            }
            if ($game.HasExited) {
                break
            }
            Start-Sleep -Seconds 5
            $game.Refresh()
        }

        $reportLines = Select-String -Path $testReport `
            -Pattern '^(Executed \d+ tests|FAIL |STATUS )' |
            ForEach-Object { $_.Line }
        $reportLines | Write-Output

        if (!(Select-String -Path $testReport `
            -Pattern '^Executed \d+ tests: \d+ passed, 0 failed\.$' -Quiet)) {
            throw 'Combat tests did not finish cleanly. See the report above.'
        }
    }
    finally {
        if (!$game.HasExited) {
            [void]$game.CloseMainWindow()
            if (!$game.WaitForExit(15000)) {
                $game.Kill()
                $game.WaitForExit()
            }
        }
        $game.Dispose()
        Remove-Item Env:THEARCHITECT_RUN_TESTS -ErrorAction SilentlyContinue
        Remove-Item Env:THEARCHITECT_TEST_ASSEMBLY -ErrorAction SilentlyContinue
        Remove-Item Env:THEARCHITECT_TEST_REPORT -ErrorAction SilentlyContinue
        Remove-Item Env:THEARCHITECT_REPO_ROOT -ErrorAction SilentlyContinue
        Remove-Item Env:THEARCHITECT_GAME_PROJECT -ErrorAction SilentlyContinue
    }
}

function Invoke-RunMode {
    if (!(Test-Path -LiteralPath $GodotPath)) {
        throw "Godot executable not found: $GodotPath"
    }
    $version = & $GodotPath --version
    if ($version -notmatch '^4\.5\.1\..*mono') {
        throw "Use Godot 4.5.1 .NET. Found: $version"
    }
    if (Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue) {
        throw 'Close Slay the Spire 2 before rebuilding its mod.'
    }

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $launchLog) | Out-Null
    & dotnet build $project '-p:ExportMod=true' "-p:GodotPath=$GodotPath" *> $launchLog
    if ($LASTEXITCODE -ne 0) {
        throw "Build/export failed. See $launchLog"
    }
    if (Select-String -LiteralPath $launchLog `
        -Pattern 'ERROR:|SCRIPT ERROR:|CrashHandlerException' -Quiet) {
        throw "Godot reported an export error. See $launchLog"
    }

    $gamePath = (& dotnet msbuild $project -getProperty:Sts2Path | Out-String).Trim()
    if ($LASTEXITCODE -ne 0 -or !$gamePath) {
        throw 'Could not resolve Sts2Path from TheArchitect.csproj.'
    }
    $gameExe = Join-Path $gamePath 'SlayTheSpire2.exe'
    if (!(Test-Path -LiteralPath $gameExe)) {
        throw "Game executable not found: $gameExe"
    }

    $modPath = Join-Path $gamePath 'mods/TheArchitect'
    foreach ($file in @('TheArchitect.dll', 'TheArchitect.pck', 'TheArchitect.json')) {
        if (!(Test-Path -LiteralPath (Join-Path $modPath $file))) {
            throw "Missing deployed mod file: $file"
        }
    }

    if (!$VerifyOnly) {
        $env:SteamAppId = '2868840'
        $env:SteamGameId = '2868840'
        $startInfo = New-Object System.Diagnostics.ProcessStartInfo
        $startInfo.FileName = $gameExe
        $startInfo.WorkingDirectory = $gamePath
        $startInfo.UseShellExecute = $true
        $gameProcess = [System.Diagnostics.Process]::Start($startInfo)
        $gameProcess.Dispose()
        Write-Output 'Slay the Spire 2 started with the rebuilt TheArchitect mod.'
    }

    Write-Output 'Build, export, and deployment verified.'
}

try {
    switch ($Mode) {
        'build' { Invoke-BuildMode }
        'test' { Invoke-TestMode }
        'run' { Invoke-RunMode }
    }
}
catch {
    Write-Error $_.Exception.Message
    if ($Mode -eq 'run' -and (Test-Path -LiteralPath $launchLog)) {
        Get-Content -LiteralPath $launchLog -Tail 30
    }
    exit 1
}
