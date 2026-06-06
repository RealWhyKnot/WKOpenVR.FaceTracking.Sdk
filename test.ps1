param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$testProject = Join-Path $root "tests\WKOpenVR.FaceTracking.Sdk.Tests\WKOpenVR.FaceTracking.Sdk.Tests.csproj"

dotnet run --project $testProject -c $Configuration
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
