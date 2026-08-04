using System.Diagnostics;

namespace WKOpenVR.FaceTracking.Sdk;

/// <summary>
/// Watches a module's JSON config file and reports content changes. User settings live
/// in the WKOpenVR profiles directory and win over a packaged default next to the module;
/// <see cref="FaceModuleContext.ConfigDirectory"/> is replaced on module update, so keep
/// user-writable state out of it. Parsing stays with the module -- this class only
/// detects changes (mtime, throttled) and hands back the raw text.
/// </summary>
public sealed class FaceModuleConfigWatcher
{
    private readonly double pollSeconds;
    private readonly Stopwatch throttle = Stopwatch.StartNew();
    private bool firstPoll = true;
    private string? lastPath;
    private DateTime lastWriteUtc;

    public FaceModuleConfigWatcher(
        FaceModuleContext context,
        string fileName,
        double pollSeconds = 1.0,
        string? profilesDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        string profiles = profilesDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "AppData", "LocalLow", "WKOpenVR", "profiles");
        UserConfigPath = Path.Combine(profiles, fileName);
        PackagedConfigPath = Path.Combine(context.ConfigDirectory, fileName);
        this.pollSeconds = Math.Max(0.0, pollSeconds);
    }

    /// <summary>Per-user settings file; survives module updates and wins when present.</summary>
    public string UserConfigPath { get; }

    /// <summary>Packaged default next to the module; used when no user file exists.</summary>
    public string PackagedConfigPath { get; }

    /// <summary>The file a read would use right now, or null when neither exists.</summary>
    public string? ActivePath =>
        File.Exists(UserConfigPath) ? UserConfigPath :
        File.Exists(PackagedConfigPath) ? PackagedConfigPath : null;

    /// <summary>
    /// True when the active config changed since the last successful read; the file's
    /// current text is in <paramref name="json"/>. Safe to call every update -- checks
    /// are throttled to the configured poll interval. A malformed file is still
    /// reported; keeping the last good parse on failure is the caller's job.
    /// </summary>
    public bool TryReadChanged(out string json)
    {
        json = string.Empty;
        if (!firstPoll && throttle.Elapsed.TotalSeconds < pollSeconds)
        {
            return false;
        }

        throttle.Restart();
        firstPoll = false;

        string? path = ActivePath;
        if (path is null)
        {
            return false;
        }

        DateTime writeUtc;
        try
        {
            writeUtc = File.GetLastWriteTimeUtc(path);
        }
        catch (IOException)
        {
            return false;
        }

        if (path == lastPath && writeUtc == lastWriteUtc)
        {
            return false;
        }

        try
        {
            json = File.ReadAllText(path);
        }
        catch (IOException)
        {
            json = string.Empty;
            return false;
        }

        lastPath = path;
        lastWriteUtc = writeUtc;
        return true;
    }
}
