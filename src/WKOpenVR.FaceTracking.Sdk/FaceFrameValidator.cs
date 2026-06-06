namespace WKOpenVR.FaceTracking.Sdk;

public static class FaceFrameValidator
{
    public static void Sanitize(FaceFrame frame)
    {
        if ((frame.Flags & FaceFrameFlags.ExpressionsValid) != 0)
        {
            for (int i = 0; i < frame.Expressions.Length; i++)
            {
                frame.Expressions[i] = Clamp01Finite(frame.Expressions[i]);
            }
        }

        if ((frame.Flags & FaceFrameFlags.EyeValid) != 0)
        {
            SanitizeEye(frame.Eye.Left);
            SanitizeEye(frame.Eye.Right);
            frame.Eye.MinDilation = ClampNonNegativeFinite(frame.Eye.MinDilation);
            frame.Eye.MaxDilation = ClampNonNegativeFinite(frame.Eye.MaxDilation);
            if (frame.Eye.MaxDilation < frame.Eye.MinDilation)
            {
                frame.Eye.MaxDilation = frame.Eye.MinDilation;
            }
        }

        if ((frame.Flags & FaceFrameFlags.HeadValid) != 0)
        {
            frame.Head.Yaw = FiniteOrZero(frame.Head.Yaw);
            frame.Head.Pitch = FiniteOrZero(frame.Head.Pitch);
            frame.Head.Roll = FiniteOrZero(frame.Head.Roll);
            frame.Head.PosX = FiniteOrZero(frame.Head.PosX);
            frame.Head.PosY = FiniteOrZero(frame.Head.PosY);
            frame.Head.PosZ = FiniteOrZero(frame.Head.PosZ);
        }
    }

    private static void SanitizeEye(EyeState eye)
    {
        eye.GazeX = ClampSignedUnitFinite(eye.GazeX);
        eye.GazeY = ClampSignedUnitFinite(eye.GazeY);
        eye.PupilDiameterMm = ClampNonNegativeFinite(eye.PupilDiameterMm);
        eye.Openness = Clamp01Finite(eye.Openness);
    }

    private static float Clamp01Finite(float value)
    {
        if (!float.IsFinite(value))
        {
            return 0;
        }

        return Math.Clamp(value, 0, 1);
    }

    private static float ClampSignedUnitFinite(float value)
    {
        if (!float.IsFinite(value))
        {
            return 0;
        }

        return Math.Clamp(value, -1, 1);
    }

    private static float ClampNonNegativeFinite(float value)
    {
        if (!float.IsFinite(value))
        {
            return 0;
        }

        return Math.Max(0, value);
    }

    private static float FiniteOrZero(float value)
    {
        return float.IsFinite(value) ? value : 0;
    }
}
