using WKOpenVR.FaceTracking.Sdk;

namespace WKOpenVR.FaceTracking.SampleModule;

public sealed class SampleFaceModule : IFaceTrackingModule
{
    private float phase;

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
        return ValueTask.FromResult(new FaceModuleInitResult(
            EyeActive: false,
            ExpressionActive: request.ExpressionAvailable,
            HeadActive: request.HeadAvailable));
    }

    public ValueTask UpdateAsync(FaceFrame frame, CancellationToken cancellationToken)
    {
        frame.Clear();
        phase += 0.05f;

        frame.SetExpression(FaceExpression.JawOpen, (MathF.Sin(phase) + 1.0f) * 0.25f);
        frame.SetExpression(FaceExpression.MouthClosed, 0.1f);
        frame.Head.Yaw = MathF.Sin(phase * 0.5f) * 5.0f;
        frame.Head.Pitch = 0;
        frame.Head.Roll = 0;
        frame.Flags |= FaceFrameFlags.HeadValid;

        FaceFrameValidator.Sanitize(frame);
        return ValueTask.CompletedTask;
    }

    public ValueTask TeardownAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}
