namespace WKOpenVR.FaceTracking.Sdk;

public interface IFaceTrackingModule
{
    FaceModuleInfo ModuleInfo { get; }

    FaceModuleCapabilities Capabilities { get; }

    ValueTask<FaceModuleInitResult> InitializeAsync(
        FaceModuleContext context,
        FaceModuleInitRequest request,
        CancellationToken cancellationToken);

    ValueTask UpdateAsync(FaceFrame frame, CancellationToken cancellationToken);

    ValueTask TeardownAsync(CancellationToken cancellationToken);
}
