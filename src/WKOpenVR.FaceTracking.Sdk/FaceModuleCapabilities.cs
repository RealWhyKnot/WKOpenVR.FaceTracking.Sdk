namespace WKOpenVR.FaceTracking.Sdk;

[Flags]
public enum FaceModuleCapabilities
{
    None = 0,
    Eye = 1,
    Expression = 2,
    Head = 4,
    AudioInput = 8
}
