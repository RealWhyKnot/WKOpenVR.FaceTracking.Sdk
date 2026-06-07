namespace WKOpenVR.FaceTracking.Sdk;

/// <summary>
/// Logging surface handed to a module via <see cref="FaceModuleContext"/>. Always non-null; when the
/// host supplies no sink it is a silent no-op. Prefer guarding expensive per-frame messages with
/// <see cref="IsEnabled"/> so the string is never built when the level is disabled:
/// <code>if (logger.IsEnabled(FaceModuleLogLevel.Trace)) logger.Trace($"...{value}...");</code>
/// </summary>
public interface IFaceModuleLogger
{
    /// <summary>True when messages at <paramref name="level"/> will actually be forwarded.</summary>
    bool IsEnabled(FaceModuleLogLevel level);

    void Log(FaceModuleLogLevel level, string message);

    void Trace(string message) => Log(FaceModuleLogLevel.Trace, message);

    void Debug(string message) => Log(FaceModuleLogLevel.Debug, message);

    void Info(string message) => Log(FaceModuleLogLevel.Information, message);

    void Warn(string message) => Log(FaceModuleLogLevel.Warning, message);

    void Error(string message) => Log(FaceModuleLogLevel.Error, message);
}
