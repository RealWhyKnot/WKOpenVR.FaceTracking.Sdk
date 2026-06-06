param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$versionPath = Join-Path $root "version.txt"
$version = [System.IO.File]::ReadAllText($versionPath).Trim()
$packageDir = Join-Path $root "artifacts\packages"

New-Item -ItemType Directory -Force -Path $packageDir | Out-Null

dotnet restore (Join-Path $root "WKOpenVR.FaceTracking.Sdk.sln")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet build (Join-Path $root "WKOpenVR.FaceTracking.Sdk.sln") -c $Configuration --no-restore /p:Version=$version
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet pack (Join-Path $root "src\WKOpenVR.FaceTracking.Sdk\WKOpenVR.FaceTracking.Sdk.csproj") -c $Configuration --no-build -o $packageDir /p:PackageVersion=$version /p:Version=$version
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
