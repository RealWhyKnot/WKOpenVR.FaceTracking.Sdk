namespace WKOpenVR.FaceTracking.Sdk;

public sealed record FaceModuleInitResult(
    bool EyeActive,
    bool ExpressionActive,
    bool HeadActive);
