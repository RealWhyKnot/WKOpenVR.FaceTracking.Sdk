param(
    [string]$Configuration = "Release",
    [string]$Version = ""
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$versionPath = Join-Path $root "version.txt"
if (-not [string]::IsNullOrWhiteSpace($Version)) {
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($versionPath, $Version.Trim(), $utf8NoBom)
}
$version = [System.IO.File]::ReadAllText($versionPath).Trim()
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "version.txt is empty"
}
$packageDir = Join-Path $root "artifacts\packages"

New-Item -ItemType Directory -Force -Path $packageDir | Out-Null

dotnet restore (Join-Path $root "WKOpenVR.FaceTracking.Sdk.sln")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet build (Join-Path $root "WKOpenVR.FaceTracking.Sdk.sln") -c $Configuration --no-restore /p:Version=$version
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet pack (Join-Path $root "src\WKOpenVR.FaceTracking.Sdk\WKOpenVR.FaceTracking.Sdk.csproj") -c $Configuration --no-build -o $packageDir /p:PackageVersion=$version /p:Version=$version
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
