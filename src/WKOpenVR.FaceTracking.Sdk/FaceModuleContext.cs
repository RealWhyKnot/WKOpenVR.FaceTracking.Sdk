namespace WKOpenVR.FaceTracking.Sdk;

public sealed class FaceModuleContext
{
    public FaceModuleContext(string configDirectory, Action<string>? log = null)
    {
        ConfigDirectory = configDirectory;
        Log = log;
    }

    public string ConfigDirectory { get; }

    public Action<string>? Log { get; }
}
