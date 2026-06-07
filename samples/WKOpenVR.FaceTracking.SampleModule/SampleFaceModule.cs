using WKOpenVR.FaceTracking.Sdk;

namespace WKOpenVR.FaceTracking.SampleModule;

public sealed class SampleFaceModule : IFaceTrackingModule
{
    private float phase;
    private IFaceModuleLogger _logger = NullFaceModuleLogger.Instance;

    public FaceModuleInfo ModuleInfo { get; } = new(
        "3e9b9b76-ea55-4c73-beb4-0af3c1c4c900",
        "WKOpenVR SDK Sample",
        "WhyKnot",
        new Version(0, 1, 0));

    public FaceModuleCapabilities Capabilities =>
        FaceModuleCapabilities.Expression | FaceModuleCapabilities.Head;

    public ValueTask<FaceModuleInitResult> InitializeAsync(
        FaceModuleContext context,
        FaceModuleInitRequest request,
        CancellationToken cancellationToken)
    {
        _logger = context.Logger;
        _logger.Info($"SampleFaceModule init (SDK ABI {FaceModuleAbi.Version}, {FaceModuleAbi.SdkVersion}).");

        return ValueTask.FromResult(new FaceModuleInitResult(
            EyeActive: false,
            ExpressionActive: request.ExpressionAvailable,
            HeadActive: request.HeadAvailable));
    }

    public ValueTask UpdateAsync(FaceFrame frame, CancellationToken cancellationToken)
    {
        frame.Clear();
        phase += 0.05f;

        float jaw = (MathF.Sin(phase) + 1.0f) * 0.25f;
        frame.SetExpression(FaceExpression.JawOpen, jaw);
        frame.SetExpression(FaceExpression.MouthClosed, 0.1f);
        frame.Head.Yaw = MathF.Sin(phase * 0.5f) * 5.0f;
        frame.Head.Pitch = 0;
        frame.Head.Roll = 0;
        frame.Flags |= FaceFrameFlags.HeadValid;

        // Verbose per-frame data costs nothing unless the host enabled Trace.
        if (_logger.IsEnabled(FaceModuleLogLevel.Trace))
        {
            _logger.Trace($"phase={phase:F2} jaw={jaw:F3} yaw={frame.Head.Yaw:F2}");
        }

        FaceFrameValidator.Sanitize(frame);
        return ValueTask.CompletedTask;
    }

    public ValueTask TeardownAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}
