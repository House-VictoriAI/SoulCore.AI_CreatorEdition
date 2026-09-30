# Bump Presence and/or Host build versions so Kayleigh can verify she is on the latest fix.
#
# Usage (repo root):
#   powershell -NoProfile -ExecutionPolicy Bypass -File House/scripts/bump-versions.ps1
#   powershell -NoProfile -ExecutionPolicy Bypass -File House/scripts/bump-versions.ps1 -Target presence -Part patch
#   powershell -NoProfile -ExecutionPolicy Bypass -File House/scripts/bump-versions.ps1 -Target host -Part patch
#   powershell -NoProfile -ExecutionPolicy Bypass -File House/scripts/bump-versions.ps1 -Target all -Part minor
#
# Then: rebuild/restart Host (ALLSTART -RestartHost) and/or pack Presence (pack-presence.ps1).

param(
  [ValidateSet('presence', 'host', 'all')]
  [string]$Target = 'all',
  [ValidateSet('patch', 'minor', 'major')]
  [string]$Part = 'patch'
)

$ErrorActionPreference = 'Stop'
$repo = Resolve-Path (Join-Path $PSScriptRoot '..\..')

function Get-CsprojVersion([string]$path) {
  [xml]$xml = Get-Content $path
  $ver = $xml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
  if (-not $ver) { $ver = '0.0.0' }
  return [string]$ver
}

function Set-CsprojVersion([string]$path, [string]$newVersion) {
  $raw = Get-Content -Raw -Path $path
  if ($raw -notmatch '<Version>') {
    throw "No <Version> element in $path — add Version/InformationalVersion first."
  }
  $updated = [regex]::Replace($raw, '(?<=<Version>)[^<]+(?=</Version>)', $newVersion)
  $updated = [regex]::Replace($updated, '(?<=<InformationalVersion>)[^<]+(?=</InformationalVersion>)', $newVersion)
  if ($updated -eq $raw -and $raw -notmatch [regex]::Escape("<Version>$newVersion</Version>")) {
    # InformationalVersion may be missing — ensure Version changed at least.
    if ($raw -notmatch "<Version>$newVersion</Version>") {
      throw "Failed to rewrite Version in $path"
    }
  }
  # If InformationalVersion missing, inject next to Version.
  if ($updated -notmatch '<InformationalVersion>') {
    $updated = [regex]::Replace(
      $updated,
      '(<Version>[^<]+</Version>)',
      "`$1`r`n    <InformationalVersion>$newVersion</InformationalVersion>")
  }
  Set-Content -Path $path -Value $updated -NoNewline
}

function Bump-SemVer([string]$version, [string]$part) {
  $core = $version.Trim()
  if ($core.Contains('+')) { $core = $core.Split('+')[0] }
  if ($core.Contains('-')) { $core = $core.Split('-')[0] }
  $bits = $core.Split('.')
  $major = 0; $minor = 0; $patch = 0
  if ($bits.Length -gt 0) { [void][int]::TryParse($bits[0], [ref]$major) }
  if ($bits.Length -gt 1) { [void][int]::TryParse($bits[1], [ref]$minor) }
  if ($bits.Length -gt 2) { [void][int]::TryParse($bits[2], [ref]$patch) }
  switch ($part) {
    'major' { $major++; $minor = 0; $patch = 0 }
    'minor' { $minor++; $patch = 0 }
    default { $patch++ }
  }
  return "$major.$minor.$patch"
}

$presenceProj = Join-Path $repo 'House\House.ChatDesktop\House.ChatDesktop.csproj'
$hostProj = Join-Path $repo 'SoulCore\SoulCore.Host\SoulCore.Host.csproj'

$targets = @()
if ($Target -eq 'presence' -or $Target -eq 'all') { $targets += ,@('Presence', $presenceProj) }
if ($Target -eq 'host' -or $Target -eq 'all') { $targets += ,@('Host', $hostProj) }

foreach ($pair in $targets) {
  $name = $pair[0]
  $path = $pair[1]
  if (-not (Test-Path $path)) { throw "Missing $path" }
  $old = Get-CsprojVersion $path
  $new = Bump-SemVer $old $Part
  Set-CsprojVersion $path $new
  Write-Host "$name : $old -> $new ($Part)"
}

Write-Host ''
Write-Host 'Next:'
if ($Target -eq 'host' -or $Target -eq 'all') {
  Write-Host '  Restart Host: .\ALLSTART.ps1 -RestartHost'
  Write-Host '  Confirm: Settings > Updates shows Host version, or GET http://127.0.0.1:7700/health -> version'
}
if ($Target -eq 'presence' -or $Target -eq 'all') {
  Write-Host '  Pack Presence: House/scripts/pack-presence.ps1  (then install/Update from that release)'
  Write-Host '  Or run unpackaged from this tree and check Settings > Updates / title bar.'
}
