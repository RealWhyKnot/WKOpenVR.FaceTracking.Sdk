namespace WKOpenVR.FaceTracking.Sdk;

/// <summary>
/// Default <see cref="IFaceModuleLogger"/> that forwards to a host-provided sink, gated by a minimum
/// level. The sink takes the level as an int so it crosses the host/module reflection boundary
/// without a shared enum type. A null sink makes the logger a silent no-op.
/// </summary>
internal sealed class DelegateFaceModuleLogger : IFaceModuleLogger
{
    private readonly Action<int, string>? _sink;
    private readonly FaceModuleLogLevel _minimum;

    public DelegateFaceModuleLogger(Action<int, string>? sink, FaceModuleLogLevel minimum)
    {
        _sink = sink;
        _minimum = minimum;
    }

    public bool IsEnabled(FaceModuleLogLevel level) => _sink is not null && level >= _minimum;

    public void Log(FaceModuleLogLevel level, string message)
    {
        if (IsEnabled(level))
        {
            _sink!((int)level, message);
        }
    }
}
