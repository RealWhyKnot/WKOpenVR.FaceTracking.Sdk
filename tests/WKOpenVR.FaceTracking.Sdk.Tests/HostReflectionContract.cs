using System.Reflection;
using WKOpenVR.FaceTracking.Sdk;

namespace WKOpenVR.FaceTracking.Sdk.Tests;

/// <summary>
/// Pins every name the module host binds by reflection across the assembly-load-context
/// boundary. The host resolves these types, members, and constructor shapes by string,
/// so a rename here compiles fine everywhere and then fails at module load time on user
/// machines. This test makes such a rename fail in CI instead. Every entry corresponds
/// to a lookup in the host's NativeSdkTrackingModuleAdapter or ModuleAssembly.
/// </summary>
public static class HostReflectionContract
{
    private static readonly Assembly Sdk = typeof(FaceFrame).Assembly;

    public static void Verify()
    {
        // Types resolved by full name.
        Type moduleInterface = RequireType("WKOpenVR.FaceTracking.Sdk.IFaceTrackingModule");
        Type faceFrame = RequireType("WKOpenVR.FaceTracking.Sdk.FaceFrame");
        Type context = RequireType("WKOpenVR.FaceTracking.Sdk.FaceModuleContext");
        Type initRequest = RequireType("WKOpenVR.FaceTracking.Sdk.FaceModuleInitRequest");
        Type initResult = RequireType("WKOpenVR.FaceTracking.Sdk.FaceModuleInitResult");
        Type validator = RequireType("WKOpenVR.FaceTracking.Sdk.FaceFrameValidator");
        Type abi = RequireType("WKOpenVR.FaceTracking.Sdk.FaceModuleAbi");

        // Module surface resolved by member name on the implementing type.
        RequireMethod(moduleInterface, "InitializeAsync");
        RequireMethod(moduleInterface, "UpdateAsync");
        RequireMethod(moduleInterface, "TeardownAsync");
        RequireProperty(moduleInterface, "ModuleInfo");
        RequireProperty(moduleInterface, "Capabilities");
        RequireProperty(typeof(FaceModuleInfo), "Name");

        // Frame surface: parameterless ctor, Clear(), section properties.
        Require(faceFrame.GetConstructor(Type.EmptyTypes) is not null, "FaceFrame() ctor");
        Require(faceFrame.GetMethod("Clear", Type.EmptyTypes) is not null, "FaceFrame.Clear()");
        RequireProperty(faceFrame, "Flags");
        RequireProperty(faceFrame, "Expressions");
        RequireProperty(faceFrame, "Eye");
        RequireProperty(faceFrame, "Head");

        Require(
            validator.GetMethod("Sanitize", BindingFlags.Public | BindingFlags.Static) is not null,
            "FaceFrameValidator.Sanitize (public static)");

        // Context ctors: leveled preferred, legacy fallback.
        Require(
            context.GetConstructor([typeof(string), typeof(Action<int, string>), typeof(int)]) is not null,
            "FaceModuleContext(string, Action<int,string>, int) ctor");
        Require(
            context.GetConstructor([typeof(string), typeof(Action<string>)]) is not null,
            "FaceModuleContext(string, Action<string>) ctor");

        Require(
            initRequest.GetConstructor([typeof(bool), typeof(bool), typeof(bool)]) is not null,
            "FaceModuleInitRequest(bool, bool, bool) ctor");
        RequireProperty(initResult, "EyeActive");
        RequireProperty(initResult, "ExpressionActive");

        // Eye and head members read per frame.
        RequireProperty(typeof(EyeFrame), "Left");
        RequireProperty(typeof(EyeFrame), "Right");
        RequireProperty(typeof(EyeFrame), "MinDilation");
        RequireProperty(typeof(EyeFrame), "MaxDilation");
        RequireProperty(typeof(EyeState), "GazeX");
        RequireProperty(typeof(EyeState), "GazeY");
        RequireProperty(typeof(EyeState), "PupilDiameterMm");
        RequireProperty(typeof(EyeState), "Openness");
        RequireProperty(typeof(HeadFrame), "Yaw");
        RequireProperty(typeof(HeadFrame), "Pitch");
        RequireProperty(typeof(HeadFrame), "Roll");
        RequireProperty(typeof(HeadFrame), "PosX");
        RequireProperty(typeof(HeadFrame), "PosY");
        RequireProperty(typeof(HeadFrame), "PosZ");

        // ABI version: read via GetField + GetRawConstantValue, must stay a public const int.
        FieldInfo version = abi.GetField("Version", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("FaceModuleAbi.Version field missing");
        Require(version.IsLiteral, "FaceModuleAbi.Version must be a const");
        Require(version.GetRawConstantValue() is 1, "FaceModuleAbi.Version must be 1 on this branch");

        // Per-update inputs pushed by the host (probed as nullable: absence on either
        // side of the boundary degrades to zero targets).
        RequireProperty(faceFrame, "Inputs");
        Type inputs = RequireType("WKOpenVR.FaceTracking.Sdk.FaceFrameInputs");
        Require(
            inputs.GetMethod("SetGazeTargets", [typeof(float[]), typeof(int)]) is not null,
            "FaceFrameInputs.SetGazeTargets(float[], int) method");

        // Optional status source detected by interface full name, polled via GetStatus.
        Type statusSource = RequireType("WKOpenVR.FaceTracking.Sdk.IFaceModuleStatusSource");
        RequireMethod(statusSource, "GetStatus");
        RequireProperty(typeof(FaceModuleStatus), "Health");
        RequireProperty(typeof(FaceModuleStatus), "Detail");

        // Numeric flag values the host hardcodes.
        Require((long)FaceFrameFlags.ExpressionsValid == 1, "FaceFrameFlags.ExpressionsValid == 1");
        Require((long)FaceFrameFlags.EyeValid == 2, "FaceFrameFlags.EyeValid == 2");
        Require((long)FaceFrameFlags.HeadValid == 4, "FaceFrameFlags.HeadValid == 4");
        Require((long)FaceModuleCapabilities.Eye == 1, "FaceModuleCapabilities.Eye == 1");
        Require((long)FaceModuleCapabilities.Expression == 2, "FaceModuleCapabilities.Expression == 2");
        Require((long)FaceModuleCapabilities.Head == 4, "FaceModuleCapabilities.Head == 4");
        Require((long)FaceModuleCapabilities.AudioInput == 8, "FaceModuleCapabilities.AudioInput == 8");
    }

    private static Type RequireType(string fullName)
    {
        return Sdk.GetType(fullName)
            ?? throw new InvalidOperationException($"host-bound type missing: {fullName}");
    }

    private static void RequireMethod(Type type, string name)
    {
        Require(type.GetMethod(name) is not null, $"{type.Name}.{name} method");
    }

    private static void RequireProperty(Type type, string name)
    {
        Require(type.GetProperty(name) is not null, $"{type.Name}.{name} property");
    }

    private static void Require(bool condition, string what)
    {
        if (!condition)
        {
            throw new InvalidOperationException("host reflection contract broken: " + what);
        }
    }
}
