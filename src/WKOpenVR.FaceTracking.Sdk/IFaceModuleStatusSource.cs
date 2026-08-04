namespace WKOpenVR.FaceTracking.Sdk;

/// <summary>
/// Optional. A module that implements this is polled by the host (roughly once a
/// second) and its health is surfaced in the app UI, so users can see a lost capture
/// device instead of a silently frozen face. Return quickly and never throw.
/// </summary>
public interface IFaceModuleStatusSource
{
    FaceModuleStatus GetStatus();
}
