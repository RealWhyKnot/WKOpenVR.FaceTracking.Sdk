namespace WKOpenVR.FaceTracking.Sdk;

public enum FaceModuleHealth
{
    Healthy = 0,
    Degraded = 1,
    DeviceLost = 2,
}

/// <summary>
/// A module's self-reported condition. <see cref="Detail"/> is a short human-readable
/// note shown in the app UI next to the health indicator (device name, error summary).
/// </summary>
public sealed record FaceModuleStatus(FaceModuleHealth Health, string? Detail = null);
