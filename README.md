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

## Lifecycle and threading

The host loads a module into its own process and drives it like this:

- `InitializeAsync` and `TeardownAsync` run on the host's command thread. `UpdateAsync` runs on a
  separate dedicated thread, so state handed from init to update crosses threads; publish it safely
  (set it before returning from `InitializeAsync`, or use volatile/lock semantics).
- **`UpdateAsync` is called in a tight loop with no delay between calls.** The module must pace
  itself -- block on its input (audio buffer, capture frame) or sleep -- or it will spin a CPU core
  at 100%. Downstream consumers read at roughly 120 Hz, so producing faster than that is wasted work.
- An exception escaping `UpdateAsync` terminates the module process. Catch and degrade instead of
  throwing for recoverable conditions (a lost capture device, a transient read failure).
- The host owns the `FaceFrame`: it clears the frame before each `UpdateAsync` and sanitizes it
  afterwards (`FaceFrameValidator.Sanitize`). Write your outputs and set `Flags` every update;
  a section without its flag set is ignored regardless of the values in it.
- Section validity is `FaceFrameFlags` only. The `FaceModuleInitResult` booleans select which
  channels the host activates; `HeadAvailable` in the request is always true today.
- `FaceModuleContext.ConfigDirectory` is the module's install directory for the installed version.
  It is replaced on module update, so treat it as read-only package content and keep user-writable
  state under the WKOpenVR profiles directory instead.

## Capabilities

`FaceModuleCapabilities` is declarative metadata. `AudioInput` declares that the module captures
audio; the module opens and owns its capture device (WASAPI/NAudio or similar) -- the host does not
provide an audio stream. `Eye` and `Expression` describe which output channels the module can drive;
the per-session decision is made in `FaceModuleInitResult`.

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
