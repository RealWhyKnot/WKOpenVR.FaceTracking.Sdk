namespace WKOpenVR.FaceTracking.Sdk;

/// <summary>
/// Per-session services handed to a module at initialization: a config directory and a logger.
///
/// Two constructors exist for host/module version compatibility. Older hosts call the
/// <c>(string, Action&lt;string&gt;?)</c> form (all messages map to Information). Newer hosts call the
/// leveled <c>(string, Action&lt;int, string&gt;?, int)</c> form, which lets a module emit verbose
/// Trace/Debug output that is dropped cheaply unless the host opted into a verbose level. The level
/// crosses the boundary as an int matching <see cref="FaceModuleLogLevel"/>. <see cref="Logger"/> is
/// always non-null.
/// </summary>
public sealed class FaceModuleContext
{
    /// <summary>Legacy constructor. All forwarded messages are treated as Information.</summary>
    public FaceModuleContext(string configDirectory, Action<string>? log = null)
    {
        ConfigDirectory = configDirectory;
        Log = log;
        Logger = new DelegateFaceModuleLogger(
            log is null ? null : (_, message) => log(message),
            FaceModuleLogLevel.Information);
    }

    /// <summary>Leveled constructor. <paramref name="minimumLevel"/> matches <see cref="FaceModuleLogLevel"/>.</summary>
    public FaceModuleContext(string configDirectory, Action<int, string>? logSink, int minimumLevel)
    {
        ConfigDirectory = configDirectory;
        var minimum = (FaceModuleLogLevel)Math.Clamp(minimumLevel, (int)FaceModuleLogLevel.Trace, (int)FaceModuleLogLevel.Error);
        Logger = new DelegateFaceModuleLogger(logSink, minimum);
        Log = logSink is null ? null : message => logSink((int)FaceModuleLogLevel.Information, message);
    }

    public string ConfigDirectory { get; }

    /// <summary>Backward-compatible single-level sink. Prefer <see cref="Logger"/> for leveled logging.</summary>
    public Action<string>? Log { get; }

    /// <summary>Leveled logger; never null (silent when the host supplies no sink).</summary>
    public IFaceModuleLogger Logger { get; }
}
