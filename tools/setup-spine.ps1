# Install the editor build, not the game's renamed release DLL.
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$cache = Join-Path $projectRoot '.artifacts/spine-setup'
New-Item -ItemType Directory -Force $cache | Out-Null
$archive = Join-Path $cache 'spine-godot-4.2-4.5.1.zip'
$url = 'https://spine-godot.s3.eu-central-1.amazonaws.com/4.2/4.5.1-stable/spine-godot-extension-4.2-4.5.1-stable.zip'
Invoke-WebRequest $url -OutFile $archive
$expected = '47C823A47373374530443C71E6D827DF1DD6D441390D95C7CEA31C54BC64EE57'
if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $expected) {
    throw 'Spine archive checksum changed. Verify the upstream release before installing.'
}
Expand-Archive -LiteralPath $archive -DestinationPath $cache -Force
$destination = Join-Path $projectRoot 'bin/windows'
New-Item -ItemType Directory -Force $destination | Out-Null
Copy-Item (Join-Path $cache 'bin/windows/*.dll') -Destination $destination -Force
Copy-Item (Join-Path $cache 'bin/spine_godot_extension.gdextension') -Destination (Join-Path $projectRoot 'bin') -Force
Write-Output 'Spine 4.2 for Godot 4.5.1 installed. Reopen the Godot editor.'
