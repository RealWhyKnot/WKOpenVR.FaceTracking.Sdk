param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

dotnet build (Join-Path $root "WKOpenVR.FaceTracking.Sdk.sln") -c $Configuration /warnaserror
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
