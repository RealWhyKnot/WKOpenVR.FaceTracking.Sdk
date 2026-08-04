using System.Diagnostics;

namespace WKOpenVR.FaceTracking.Sdk;

/// <summary>
/// Paces a module's update loop. The host calls UpdateAsync in a tight loop with no
/// delay between calls, so a module that does not already block on its input source
/// should call <see cref="WaitForNext"/> at the top of each update or it will spin a
/// CPU core. Drift-absorbing: a late frame borrows from the following period, but the
/// schedule never falls more than one period behind real time.
/// </summary>
public sealed class FrameRateLimiter
{
    private readonly double periodMs;
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private double dueMs;

    public FrameRateLimiter(float targetHz)
    {
        if (!float.IsFinite(targetHz) || targetHz <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(targetHz), targetHz, "target rate must be a positive frequency");
        }

        periodMs = 1000.0 / targetHz;
    }

    /// <summary>Blocks until the next frame is due. Returns immediately on the first call and after <see cref="Reset"/>.</summary>
    public void WaitForNext(CancellationToken cancellationToken = default)
    {
        double now = clock.Elapsed.TotalMilliseconds;
        if (dueMs <= 0.0)
        {
            dueMs = now + periodMs;
            return;
        }

        double waitMs = dueMs - now;
        if (waitMs > 0.0)
        {
            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(waitMs));
            }
            else
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(waitMs));
            }

            now = clock.Elapsed.TotalMilliseconds;
        }

        dueMs += periodMs;
        if (dueMs < now)
        {
            dueMs = now;
        }
    }

    public void Reset()
    {
        dueMs = 0.0;
    }
}
