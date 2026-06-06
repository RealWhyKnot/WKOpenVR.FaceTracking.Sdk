namespace WKOpenVR.FaceTracking.Sdk;

public sealed class HeadFrame
{
    public float Yaw { get; set; }

    public float Pitch { get; set; }

    public float Roll { get; set; }

    public float PosX { get; set; }

    public float PosY { get; set; }

    public float PosZ { get; set; }

    public void Clear()
    {
        Yaw = 0;
        Pitch = 0;
        Roll = 0;
        PosX = 0;
        PosY = 0;
        PosZ = 0;
    }
}
