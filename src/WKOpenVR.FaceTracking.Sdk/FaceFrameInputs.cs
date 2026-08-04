namespace WKOpenVR.FaceTracking.Sdk;

/// <summary>
/// Optional per-update inputs the host pushes into the frame before UpdateAsync.
/// A host that predates this type never fills it and a module that ignores it sees
/// no difference; an empty target list always means "nothing to look at right now",
/// so modules must behave sensibly with zero targets.
/// </summary>
public sealed class FaceFrameInputs
{
    public const int MaxGazeTargets = 8;

    /// <summary>Floats per packed target: yaw, pitch, radius, velYaw, velPitch, weight, kind, trackId.</summary>
    public const int GazeTargetStride = 8;

    private readonly FaceGazeTarget[] targets = new FaceGazeTarget[MaxGazeTargets];
    private int count;

    public int GazeTargetCount => count;

    public ReadOnlySpan<FaceGazeTarget> GazeTargets => new(targets, 0, count);

    /// <summary>
    /// Host-facing setter. Targets arrive as a packed float array so the host can fill
    /// them across the load-context boundary without sharing SDK types. Entries with a
    /// non-finite value are dropped, weight is clamped to [0,1], radius floors at 0,
    /// and an unrecognized kind decodes as <see cref="FaceGazeTargetKind.Unknown"/>.
    /// </summary>
    public void SetGazeTargets(float[] packed, int targetCount)
    {
        count = 0;
        if (packed is null || targetCount <= 0)
        {
            return;
        }

        int available = Math.Min(targetCount, Math.Min(MaxGazeTargets, packed.Length / GazeTargetStride));
        for (int i = 0; i < available; i++)
        {
            int o = i * GazeTargetStride;
            float yaw = packed[o];
            float pitch = packed[o + 1];
            float radius = packed[o + 2];
            float velYaw = packed[o + 3];
            float velPitch = packed[o + 4];
            float weight = packed[o + 5];
            float kindRaw = packed[o + 6];
            float trackIdRaw = packed[o + 7];
            if (!float.IsFinite(yaw) || !float.IsFinite(pitch) || !float.IsFinite(radius) ||
                !float.IsFinite(velYaw) || !float.IsFinite(velPitch) || !float.IsFinite(weight) ||
                !float.IsFinite(kindRaw) || !float.IsFinite(trackIdRaw))
            {
                continue;
            }

            int kindValue = (int)kindRaw;
            FaceGazeTargetKind kind = kindValue is >= 0 and <= 2
                ? (FaceGazeTargetKind)kindValue
                : FaceGazeTargetKind.Unknown;
            targets[count++] = new FaceGazeTarget(
                yaw,
                pitch,
                MathF.Max(0.0f, radius),
                velYaw,
                velPitch,
                Math.Clamp(weight, 0.0f, 1.0f),
                kind,
                (int)trackIdRaw);
        }
    }

    public void Clear()
    {
        count = 0;
    }
}
