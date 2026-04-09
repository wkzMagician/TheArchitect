$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$artifacts = Join-Path $root '.artifacts'
$buildOut = Join-Path $artifacts 'build'
$modsOut = Join-Path $artifacts 'mods'
$architectProject = Join-Path $root 'TheArchitect.csproj'
$testsProject = Join-Path $root 'Tests\TheArchitect.Tests.csproj'
$nugetConfig = Join-Path $root 'Tests\nuget.config'

dotnet restore $testsProject --configfile $nugetConfig
dotnet build $architectProject --no-restore -p:OutputPath=$buildOut\ -p:ModsPath=$modsOut\
dotnet run --project $testsProject --no-restore
