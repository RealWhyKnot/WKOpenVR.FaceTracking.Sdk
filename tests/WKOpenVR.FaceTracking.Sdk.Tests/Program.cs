using WKOpenVR.FaceTracking.SampleModule;
using WKOpenVR.FaceTracking.Sdk;

var tests = new (string Name, Action Body)[]
{
    ("FaceExpression order", FaceExpressionOrder),
    ("Frame validation", FrameValidation),
    ("Sample module output", SampleModuleOutput),
    ("ABI version is exposed", AbiVersionExposed),
    ("Compatibility matrix", CompatibilityMatrix),
    ("Leveled logger gates by level", LeveledLoggerGates),
    ("Trace level forwards everything", TraceLevelForwards),
    ("Legacy context maps to information", LegacyContextMapsToInformation)
};

foreach (var test in tests)
{
    test.Body();
    Console.WriteLine("PASS " + test.Name);
}

static void FaceExpressionOrder()
{
    AssertEqual(88, FaceExpressionCount.Value);
    AssertEqual(0, (int)FaceExpression.EyeSquintRight);
    AssertEqual(48, (int)FaceExpression.NoseSneerRight);
    AssertEqual(49, (int)FaceExpression.NoseSneerLeft);
    AssertEqual(87, (int)FaceExpression.NeckFlexLeft);
    AssertEqual(FaceExpressionCount.Value, Enum.GetValues<FaceExpression>().Length);
}

static void FrameValidation()
{
    var frame = new FaceFrame();
    frame.SetExpression(FaceExpression.JawOpen, 2.0f);
    frame.SetExpression(FaceExpression.MouthClosed, float.NaN);
    frame.Flags |= FaceFrameFlags.EyeValid | FaceFrameFlags.HeadValid;
    frame.Eye.Left.GazeX = 3.0f;
    frame.Eye.Left.GazeY = float.NegativeInfinity;
    frame.Eye.Left.Openness = -2.0f;
    frame.Eye.MinDilation = 3.0f;
    frame.Eye.MaxDilation = 1.0f;
    frame.Head.Yaw = float.PositiveInfinity;

    FaceFrameValidator.Sanitize(frame);

    AssertEqual(1.0f, frame.GetExpression(FaceExpression.JawOpen));
    AssertEqual(0.0f, frame.GetExpression(FaceExpression.MouthClosed));
    AssertEqual(1.0f, frame.Eye.Left.GazeX);
    AssertEqual(0.0f, frame.Eye.Left.GazeY);
    AssertEqual(0.0f, frame.Eye.Left.Openness);
    AssertEqual(3.0f, frame.Eye.MaxDilation);
    AssertEqual(0.0f, frame.Head.Yaw);
}

static void SampleModuleOutput()
{
    var module = new SampleFaceModule();
    var init = module.InitializeAsync(
        new FaceModuleContext(Path.GetTempPath()),
        new FaceModuleInitRequest(EyeAvailable: true, ExpressionAvailable: true, HeadAvailable: true),
        CancellationToken.None).AsTask().GetAwaiter().GetResult();

    var frame = new FaceFrame();
    module.UpdateAsync(frame, CancellationToken.None).AsTask().GetAwaiter().GetResult();

    AssertTrue(init.ExpressionActive);
    AssertTrue(init.HeadActive);
    AssertTrue((frame.Flags & FaceFrameFlags.ExpressionsValid) != 0);
    AssertTrue((frame.Flags & FaceFrameFlags.HeadValid) != 0);
    AssertTrue(frame.GetExpression(FaceExpression.JawOpen) > 0);
}

static void AbiVersionExposed()
{
    AssertTrue(FaceModuleAbi.Version >= 1);
    AssertTrue(!string.IsNullOrWhiteSpace(FaceModuleAbi.SdkVersion));
}

static void CompatibilityMatrix()
{
    AssertEqual((int)FaceModuleCompatibilityStatus.Current, (int)FaceModuleCompatibility.Evaluate(1, 1, 1));
    AssertEqual((int)FaceModuleCompatibilityStatus.OutdatedSupported, (int)FaceModuleCompatibility.Evaluate(1, 2, 1));
    AssertEqual((int)FaceModuleCompatibilityStatus.Unsupported, (int)FaceModuleCompatibility.Evaluate(1, 3, 2));
    AssertEqual((int)FaceModuleCompatibilityStatus.Newer, (int)FaceModuleCompatibility.Evaluate(3, 2, 1));

    AssertTrue(FaceModuleCompatibility.CanLoad(FaceModuleCompatibilityStatus.Current));
    AssertTrue(FaceModuleCompatibility.CanLoad(FaceModuleCompatibilityStatus.OutdatedSupported));
    AssertTrue(!FaceModuleCompatibility.CanLoad(FaceModuleCompatibilityStatus.Unsupported));
    AssertTrue(!FaceModuleCompatibility.CanLoad(FaceModuleCompatibilityStatus.Newer));
}

static void LeveledLoggerGates()
{
    var captured = new List<(int Level, string Message)>();
    var context = new FaceModuleContext("x", (level, message) => captured.Add((level, message)), (int)FaceModuleLogLevel.Information);

    AssertTrue(!context.Logger.IsEnabled(FaceModuleLogLevel.Debug));
    AssertTrue(context.Logger.IsEnabled(FaceModuleLogLevel.Information));

    context.Logger.Debug("dropped");
    context.Logger.Info("kept");

    AssertEqual(1, captured.Count);
    AssertEqual((int)FaceModuleLogLevel.Information, captured[0].Level);
    AssertEqual("kept", captured[0].Message);
}

static void TraceLevelForwards()
{
    var captured = new List<(int Level, string Message)>();
    var context = new FaceModuleContext("x", (level, message) => captured.Add((level, message)), (int)FaceModuleLogLevel.Trace);

    AssertTrue(context.Logger.IsEnabled(FaceModuleLogLevel.Trace));
    context.Logger.Trace("verbose");
    AssertEqual(1, captured.Count);
    AssertEqual((int)FaceModuleLogLevel.Trace, captured[0].Level);
}

static void LegacyContextMapsToInformation()
{
    var messages = new List<string>();
    var context = new FaceModuleContext("x", (string message) => messages.Add(message));

    AssertTrue(!context.Logger.IsEnabled(FaceModuleLogLevel.Debug));
    AssertTrue(context.Logger.IsEnabled(FaceModuleLogLevel.Information));

    context.Logger.Debug("dropped");
    context.Logger.Info("kept");

    AssertEqual(1, messages.Count);
    AssertEqual("kept", messages[0]);
}

static void AssertTrue(bool value)
{
    if (!value)
    {
        throw new InvalidOperationException("Assertion failed");
    }
}

static void AssertEqual<T>(T expected, T actual)
    where T : IEquatable<T>
{
    if (!expected.Equals(actual))
    {
        throw new InvalidOperationException("Expected " + expected + " but got " + actual);
    }
}
