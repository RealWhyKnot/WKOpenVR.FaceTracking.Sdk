namespace WKOpenVR.FaceTracking.Sdk;

/// <summary>A silent <see cref="IFaceModuleLogger"/>. Handy as a default before initialization and in tests.</summary>
public sealed class NullFaceModuleLogger : IFaceModuleLogger
{
    public static readonly NullFaceModuleLogger Instance = new();

    private NullFaceModuleLogger()
    {
    }

    public bool IsEnabled(FaceModuleLogLevel level) => false;

    public void Log(FaceModuleLogLevel level, string message)
    {
    }
}
