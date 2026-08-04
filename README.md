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
  itself -- block on its input (audio buffer, capture frame) or use `FrameRateLimiter` -- or it
  will spin a CPU core at 100%. Downstream consumers read at roughly 120 Hz, so producing faster
  than that is wasted work.
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

## Per-update inputs: gaze targets

`FaceFrame.Inputs` carries optional host-supplied data, filled before each `UpdateAsync`. Today
that is a list of up to 8 `FaceGazeTarget` records: candidate look-targets in the wearer's view
(head-relative radians, saliency weight, a `Kind` of face/motion, and a `TrackId` that stays
stable while the detector tracks the same thing). A module that drives eyes can saccade to and
hold these targets; a module that ignores `Inputs` behaves exactly as before. The list is often
empty -- no capture source, nothing salient in view -- and modules must treat an empty list as
"idle gaze", not an error. Hosts without a target source simply never fill it.

## Module status

A module may implement `IFaceModuleStatusSource`. The host polls `GetStatus()` about once a
second and shows the result in the app UI: `Healthy`, `Degraded`, or `DeviceLost`, plus a short
detail string. Report `DeviceLost` when a capture device disappears instead of silently freezing
output; return quickly and never throw.

## Configuration and settings UI

User-editable settings belong in a JSON file under the WKOpenVR profiles directory
(`%UserProfile%\AppData\LocalLow\WKOpenVR\profiles\<file>.json`); a packaged default with the
same name can ship next to the module DLL. `FaceModuleConfigWatcher` implements this lookup and
hot-reload: user file wins, mtime changes are detected with a built-in poll throttle, and the
module keeps its last good config when a rewrite is momentarily unreadable or malformed.

A module can also ship a `settings_descriptor.json` next to its `manifest.json` so the WKOpenVR
overlay renders editing controls for those settings:

```json
{
  "schema": 1,
  "file": "my_module.json",
  "settings": [
    { "key": "DriveMouth", "type": "bool", "label": "Drive mouth", "default": true },
    { "key": "MouthIntensity", "type": "float", "label": "Mouth intensity", "min": 0.0, "max": 2.0, "default": 1.0 },
    { "key": "MicDeviceNumber", "type": "int", "label": "Mic device", "min": -1, "max": 32, "default": -1 },
    { "key": "QualityMode", "type": "enum", "label": "Quality", "choices": ["standard", "model"], "default": "standard" },
    { "key": "MicDeviceName", "type": "string", "label": "Mic device name", "default": "" }
  ]
}
```

The overlay writes values as top-level keys into the named profiles-dir file; the module picks
them up through its config watcher. Types: `bool`, `int`, `float` (with `min`/`max`), `enum`
(with `choices`), `string`.

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
