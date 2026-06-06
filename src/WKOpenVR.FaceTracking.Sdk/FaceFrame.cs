namespace WKOpenVR.FaceTracking.Sdk;

public sealed class FaceFrame
{
    public FaceFrame()
    {
        Expressions = new float[FaceExpressionCount.Value];
    }

    public FaceFrameFlags Flags { get; set; }

    public float[] Expressions { get; }

    public EyeFrame Eye { get; } = new();

    public HeadFrame Head { get; } = new();

    public void Clear()
    {
        Array.Clear(Expressions);
        Eye.Clear();
        Head.Clear();
        Flags = FaceFrameFlags.None;
    }

    public float GetExpression(FaceExpression expression)
    {
        return Expressions[(int)expression];
    }

    public void SetExpression(FaceExpression expression, float value)
    {
        Expressions[(int)expression] = value;
        Flags |= FaceFrameFlags.ExpressionsValid;
    }
}
