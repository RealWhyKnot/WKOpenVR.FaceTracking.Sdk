namespace WKOpenVR.FaceTracking.Sdk;

public sealed class EyeState
{
    public float GazeX { get; set; }

    public float GazeY { get; set; }

    public float PupilDiameterMm { get; set; }

    public float Openness { get; set; }

    public void Clear()
    {
        GazeX = 0;
        GazeY = 0;
        PupilDiameterMm = 0;
        Openness = 0;
    }
}
