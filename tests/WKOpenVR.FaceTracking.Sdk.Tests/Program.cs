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
    ("Legacy context maps to information", LegacyContextMapsToInformation),
    ("Gaze target packing round-trip", GazeTargetPackingRoundTrip),
    ("Gaze target sanitization", GazeTargetSanitization),
    ("Frame clear resets inputs", FrameClearResetsInputs),
    ("Frame rate limiter paces", FrameRateLimiterPaces),
    ("Module status values are pinned", ModuleStatusValuesPinned),
    ("Config watcher detects changes", ConfigWatcherDetectsChanges),
    ("Config watcher prefers user file", ConfigWatcherPrefersUserFile),
    ("Host reflection contract", WKOpenVR.FaceTracking.Sdk.Tests.HostReflectionContract.Verify)
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

static void GazeTargetPackingRoundTrip()
{
    var frame = new FaceFrame();
    float[] packed =
    [
        0.10f, -0.20f, 0.05f, 0.01f, -0.02f, 0.90f, 1.0f, 42.0f,
        -0.30f, 0.15f, 0.10f, 0.00f, 0.00f, 0.40f, 2.0f, 7.0f,
    ];
    frame.Inputs.SetGazeTargets(packed, 2);

    AssertEqual(2, frame.Inputs.GazeTargetCount);
    FaceGazeTarget first = frame.Inputs.GazeTargets[0];
    AssertEqual(0.10f, first.Yaw);
    AssertEqual(-0.20f, first.Pitch);
    AssertEqual(0.90f, first.Weight);
    AssertEqual((int)FaceGazeTargetKind.Face, (int)first.Kind);
    AssertEqual(42, first.TrackId);
    AssertEqual((int)FaceGazeTargetKind.Motion, (int)frame.Inputs.GazeTargets[1].Kind);
}

static void GazeTargetSanitization()
{
    var inputs = new FaceFrameInputs();

    // Second entry has a NaN yaw and is dropped; weight above 1 clamps; kind 9 decodes Unknown.
    float[] packed =
    [
        0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.7f, 9.0f, 1.0f,
        float.NaN, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f, 1.0f, 2.0f,
    ];
    inputs.SetGazeTargets(packed, 2);
    AssertEqual(1, inputs.GazeTargetCount);
    AssertEqual(1.0f, inputs.GazeTargets[0].Weight);
    AssertEqual((int)FaceGazeTargetKind.Unknown, (int)inputs.GazeTargets[0].Kind);

    // Count clamps to what the packed array actually holds and to the maximum.
    float[] one = [0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.0f, 3.0f];
    inputs.SetGazeTargets(one, 99);
    AssertEqual(1, inputs.GazeTargetCount);

    inputs.SetGazeTargets(one, 0);
    AssertEqual(0, inputs.GazeTargetCount);
}

static void FrameClearResetsInputs()
{
    var frame = new FaceFrame();
    float[] one = [0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.0f, 3.0f];
    frame.Inputs.SetGazeTargets(one, 1);
    frame.SetExpression(FaceExpression.JawOpen, 0.5f);
    AssertEqual(1, frame.Inputs.GazeTargetCount);

    // Sanitize must leave inputs alone; Clear must reset them.
    FaceFrameValidator.Sanitize(frame);
    AssertEqual(1, frame.Inputs.GazeTargetCount);
    frame.Clear();
    AssertEqual(0, frame.Inputs.GazeTargetCount);
}

static void FrameRateLimiterPaces()
{
    var limiter = new FrameRateLimiter(250.0f);
    var clock = System.Diagnostics.Stopwatch.StartNew();
    for (int i = 0; i < 6; i++)
    {
        limiter.WaitForNext();
    }

    // First call is free; five paced waits at 4 ms each. Keep the bound loose for CI.
    AssertTrue(clock.Elapsed.TotalMilliseconds >= 5 * 4.0 * 0.5);

    limiter.Reset();
    var reset = System.Diagnostics.Stopwatch.StartNew();
    limiter.WaitForNext();
    AssertTrue(reset.Elapsed.TotalMilliseconds < 50.0);
}

static void ModuleStatusValuesPinned()
{
    AssertEqual(0, (int)FaceModuleHealth.Healthy);
    AssertEqual(1, (int)FaceModuleHealth.Degraded);
    AssertEqual(2, (int)FaceModuleHealth.DeviceLost);
    AssertEqual("mic gone", new FaceModuleStatus(FaceModuleHealth.DeviceLost, "mic gone").Detail ?? "");
}

static void ConfigWatcherDetectsChanges()
{
    string root = Path.Combine(Path.GetTempPath(), "wkovr-sdk-watcher-" + Guid.NewGuid().ToString("N"));
    string packaged = Path.Combine(root, "module");
    string profiles = Path.Combine(root, "profiles");
    Directory.CreateDirectory(packaged);
    Directory.CreateDirectory(profiles);
    try
    {
        var watcher = new FaceModuleConfigWatcher(
            new FaceModuleContext(packaged), "settings.json", pollSeconds: 0.0, profilesDirectory: profiles);

        AssertTrue(!watcher.TryReadChanged(out _));

        File.WriteAllText(Path.Combine(packaged, "settings.json"), "{\"a\":1}");
        AssertTrue(watcher.TryReadChanged(out string json));
        AssertEqual("{\"a\":1}", json);
        AssertTrue(!watcher.TryReadChanged(out _));

        File.SetLastWriteTimeUtc(Path.Combine(packaged, "settings.json"), DateTime.UtcNow.AddSeconds(5));
        AssertTrue(watcher.TryReadChanged(out _));
    }
    finally
    {
        Directory.Delete(root, recursive: true);
    }
}

static void ConfigWatcherPrefersUserFile()
{
    string root = Path.Combine(Path.GetTempPath(), "wkovr-sdk-watcher-" + Guid.NewGuid().ToString("N"));
    string packaged = Path.Combine(root, "module");
    string profiles = Path.Combine(root, "profiles");
    Directory.CreateDirectory(packaged);
    Directory.CreateDirectory(profiles);
    try
    {
        File.WriteAllText(Path.Combine(packaged, "settings.json"), "packaged");
        File.WriteAllText(Path.Combine(profiles, "settings.json"), "user");
        var watcher = new FaceModuleConfigWatcher(
            new FaceModuleContext(packaged), "settings.json", pollSeconds: 0.0, profilesDirectory: profiles);

        AssertTrue(watcher.TryReadChanged(out string json));
        AssertEqual("user", json);
        AssertEqual(watcher.UserConfigPath, watcher.ActivePath!);
    }
    finally
    {
        Directory.Delete(root, recursive: true);
    }
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
