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

## Logging

`FaceModuleContext.Logger` is an `IFaceModuleLogger` with `Trace`/`Debug`/`Information`/`Warning`/`Error`
levels and an `IsEnabled(level)` check. The host decides the minimum level it forwards, so a module
can emit a verbose per-frame `Trace` firehose that costs nothing unless the host runs in a verbose
mode. Guard expensive messages:

```csharp
if (context.Logger.IsEnabled(FaceModuleLogLevel.Trace))
    context.Logger.Trace($"rms={rms:F3} jaw={jaw:F2}");
```

The legacy `FaceModuleContext.Log` (`Action<string>`) still exists and maps to `Information`.

## Versioning and compatibility

`FaceModuleAbi.Version` is the contract (ABI) version baked into a module when it is built. The host
reads it and decides compatibility with `FaceModuleCompatibility.Evaluate` against its support window
`[minimum, supported]`:

- **Current** - same ABI the host targets.
- **Outdated but supported** - older than the host targets but within its window; loads in
  compatibility mode (the host reflects the contract and tolerates additive differences).
- **Unsupported** - below the host's minimum; refused as too old (rebuild against a newer SDK).
- **Newer** - built against a newer ABI than the host understands; refused (update the WKOpenVR app).

`Version` is bumped ONLY for a breaking change to the contract the host depends on. Purely additive
changes (new types, optional constructors, new fields) do not bump it, so old modules keep working on
new hosts and new modules keep working on old hosts. The host surfaces the result in its logs when a
module loads, indicating when a module was built against an older SDK than the running app.
