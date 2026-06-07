# WKOpenVR Face Tracking SDK

Native face module contract for WKOpenVR modules.

This repository builds a local NuGet package:

```powershell
.\build.ps1
.\test.ps1
```

Packages are written to `artifacts\packages`. Publishing to public or shared feeds is intentionally not part of these scripts.
Tagged releases attach the NuGet package to GitHub Releases so developers can download it directly without a public package feed.

## Module Contract

Modules implement `WKOpenVR.FaceTracking.Sdk.IFaceTrackingModule` and fill a reusable `FaceFrame` in `UpdateAsync`. WKOpenVR adapts those frames into the existing host packet path, so driver protocol and shared memory layout stay unchanged.

The public `FaceExpression` enum contains the 88 expression slots from the current upstream UnifiedExpressions order. It excludes the upstream `Max` sentinel.
