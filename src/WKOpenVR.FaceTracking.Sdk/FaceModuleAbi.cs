using System.Reflection;

namespace WKOpenVR.FaceTracking.Sdk;

/// <summary>
/// The binary contract version between a module and the host. A module carries the
/// <see cref="Version"/> of the SDK it was compiled against (this constant is baked into the module's
/// bundled SDK copy); the host reads it and decides compatibility via
/// <see cref="FaceModuleCompatibility"/>.
///
/// Versioning rule (kept deliberately simple):
/// - <see cref="Version"/> is a single integer bumped ONLY for a breaking change to the contract the
///   host depends on (the <c>IFaceTrackingModule</c> method shapes, <c>FaceFrame</c> layout,
///   <c>FaceModuleContext</c>/<c>FaceModuleInitRequest</c> constructors, etc.).
/// - Purely additive changes (new types, new optional constructors, new fields) do NOT bump it: the
///   host reflects the contract and tolerates additions, so old modules keep working on new hosts and
///   new modules keep working on old hosts.
/// - The host advertises a support window [minimum, supported]. A module inside the window loads
///   (current or "outdated but supported"); below it is refused as too old; above it is refused as
///   built for a newer host.
/// </summary>
public static class FaceModuleAbi
{
    /// <summary>Current module ABI contract version.</summary>
    public const int Version = 1;

    /// <summary>Informational SDK package version string, for display/logging.</summary>
    public static string SdkVersion =>
        typeof(FaceModuleAbi).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(FaceModuleAbi).Assembly.GetName().Version?.ToString()
        ?? "unknown";
}
