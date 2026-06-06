namespace WKOpenVR.FaceTracking.Sdk;

public sealed record FaceModuleInitRequest(
    bool EyeAvailable,
    bool ExpressionAvailable,
    bool HeadAvailable);
