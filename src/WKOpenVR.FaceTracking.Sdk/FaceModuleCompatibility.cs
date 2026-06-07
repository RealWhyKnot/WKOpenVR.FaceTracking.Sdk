namespace WKOpenVR.FaceTracking.Sdk;

/// <summary>Result of comparing a module's ABI against the host's support window.</summary>
public enum FaceModuleCompatibilityStatus
{
    /// <summary>Built against the same ABI the host targets.</summary>
    Current,

    /// <summary>Older than the host targets but within its support window - loads in compatibility mode.</summary>
    OutdatedSupported,

    /// <summary>Older than the host's minimum supported ABI - refused.</summary>
    Unsupported,

    /// <summary>Built against a newer ABI than the host understands - refused; update the host app.</summary>
    Newer,
}

/// <summary>
/// Pure compatibility decision between a module's ABI (<see cref="FaceModuleAbi.Version"/> from the
/// module's bundled SDK) and the host's support window [minimum, supported]. The host mirrors these
/// constants on its side; this helper exists so the rule is defined once, documented, and testable.
/// </summary>
public static class FaceModuleCompatibility
{
    public static FaceModuleCompatibilityStatus Evaluate(int moduleAbi, int hostSupportedAbi, int hostMinimumAbi)
    {
        if (moduleAbi > hostSupportedAbi)
        {
            return FaceModuleCompatibilityStatus.Newer;
        }

        if (moduleAbi == hostSupportedAbi)
        {
            return FaceModuleCompatibilityStatus.Current;
        }

        return moduleAbi >= hostMinimumAbi
            ? FaceModuleCompatibilityStatus.OutdatedSupported
            : FaceModuleCompatibilityStatus.Unsupported;
    }

    /// <summary>Whether a module with this status should be loaded at all.</summary>
    public static bool CanLoad(FaceModuleCompatibilityStatus status) =>
        status is FaceModuleCompatibilityStatus.Current or FaceModuleCompatibilityStatus.OutdatedSupported;

    /// <summary>A user-facing one-line explanation of the status.</summary>
    public static string Describe(FaceModuleCompatibilityStatus status, int moduleAbi, int hostSupportedAbi, int hostMinimumAbi)
    {
        return status switch
        {
            FaceModuleCompatibilityStatus.Current =>
                $"module SDK ABI {moduleAbi} matches the host (supported {hostSupportedAbi}).",
            FaceModuleCompatibilityStatus.OutdatedSupported =>
                $"module SDK ABI {moduleAbi} is older than the host (supported {hostSupportedAbi}); " +
                $"loading in compatibility mode. Consider rebuilding the module against a newer SDK.",
            FaceModuleCompatibilityStatus.Unsupported =>
                $"module SDK ABI {moduleAbi} is below the host minimum ({hostMinimumAbi}); " +
                "the module is too old to load. Rebuild it against a supported SDK.",
            FaceModuleCompatibilityStatus.Newer =>
                $"module SDK ABI {moduleAbi} is newer than the host supports ({hostSupportedAbi}); " +
                "update the WKOpenVR app to use this module.",
            _ => $"module SDK ABI {moduleAbi}.",
        };
    }
}
