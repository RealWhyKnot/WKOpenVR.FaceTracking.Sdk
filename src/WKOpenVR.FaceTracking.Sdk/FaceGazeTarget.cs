namespace WKOpenVR.FaceTracking.Sdk;

public enum FaceGazeTargetKind
{
    Unknown = 0,
    Face = 1,
    Motion = 2,
}

/// <summary>
/// A candidate look-target in the wearer's view, supplied by the host per update.
/// Angles are head-relative radians (positive yaw = right, positive pitch = up),
/// matching the sign conventions of <see cref="HeadFrame"/>. <see cref="Weight"/>
/// is saliency in [0,1]; <see cref="TrackId"/> stays stable across updates while
/// the detector keeps tracking the same thing, so a module can hold a lock-on.
/// </summary>
public readonly record struct FaceGazeTarget(
    float Yaw,
    float Pitch,
    float Radius,
    float VelYaw,
    float VelPitch,
    float Weight,
    FaceGazeTargetKind Kind,
    int TrackId);
