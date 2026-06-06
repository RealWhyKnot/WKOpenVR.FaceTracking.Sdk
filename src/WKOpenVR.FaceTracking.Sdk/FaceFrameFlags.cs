namespace WKOpenVR.FaceTracking.Sdk;

[Flags]
public enum FaceFrameFlags
{
    None = 0,
    ExpressionsValid = 1,
    EyeValid = 2,
    HeadValid = 4
}
