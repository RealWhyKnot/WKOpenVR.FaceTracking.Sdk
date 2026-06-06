namespace WKOpenVR.FaceTracking.Sdk;

public sealed class EyeFrame
{
    public EyeState Left { get; } = new();

    public EyeState Right { get; } = new();

    public float MinDilation { get; set; }

    public float MaxDilation { get; set; }

    public void Clear()
    {
        Left.Clear();
        Right.Clear();
        MinDilation = 0;
        MaxDilation = 0;
    }
}
