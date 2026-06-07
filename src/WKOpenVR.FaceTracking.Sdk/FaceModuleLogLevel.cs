namespace WKOpenVR.FaceTracking.Sdk;

/// <summary>
/// Severity of a module log message. Values match the host's logging tiers; the host decides the
/// minimum level it forwards, so a module can emit very verbose <see cref="Trace"/>/<see cref="Debug"/>
/// output that costs nothing when the host is not in a verbose mode.
/// </summary>
public enum FaceModuleLogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
}
