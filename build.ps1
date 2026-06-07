param(
    [string]$Configuration = "Release",
    [string]$Version = ""
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$currentHooksPath = & git config --get core.hooksPath 2>$null
if ($currentHooksPath -ne ".githooks" -and (Test-Path -LiteralPath (Join-Path $root ".git"))) {
    & git config core.hooksPath ".githooks"
    Write-Host "Activated .githooks/ via core.hooksPath"
}

$versionPath = Join-Path $root "version.txt"
if ([string]::IsNullOrWhiteSpace($Version)) {
    $today = Get-Date -Format "yyyy.M.d"
    $stateDir = Join-Path $root "artifacts"
    $statePath = Join-Path $stateDir "local_build_state.json"
    $counter = 0
    if (Test-Path -LiteralPath $statePath) {
        $state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
        if ($state.date -eq $today) {
            $counter = [int]$state.counter + 1
        }
    }
    $uid = ([guid]::NewGuid().ToString("N").Substring(0, 4)).ToUpper()
    $Version = "$today.$counter-$uid"
    New-Item -ItemType Directory -Force -Path $stateDir | Out-Null
    @{ date = $today; counter = $counter } | ConvertTo-Json | Set-Content -LiteralPath $statePath
}
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($versionPath, $Version.Trim(), $utf8NoBom)

$version = [System.IO.File]::ReadAllText($versionPath).Trim()
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "version.txt is empty"
}
Write-Host "Build version: $version"

$packageDir = Join-Path $root "artifacts\packages"

New-Item -ItemType Directory -Force -Path $packageDir | Out-Null

dotnet restore (Join-Path $root "WKOpenVR.FaceTracking.Sdk.sln")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet build (Join-Path $root "WKOpenVR.FaceTracking.Sdk.sln") -c $Configuration --no-restore /p:Version=$version
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet pack (Join-Path $root "src\WKOpenVR.FaceTracking.Sdk\WKOpenVR.FaceTracking.Sdk.csproj") -c $Configuration --no-build -o $packageDir /p:PackageVersion=$version /p:Version=$version
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
