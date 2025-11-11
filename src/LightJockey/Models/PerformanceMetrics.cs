namespace LightJockey.Models;

/// <summary>
/// Performance metrics snapshot
/// </summary>
public class PerformanceMetrics
{
    /// <summary>
    /// Gets or sets the current frames per second (FPS)
    /// </summary>
    public double StreamingFPS { get; set; }

    /// <summary>
    /// Gets or sets the average audio processing latency in milliseconds
    /// </summary>
    public double AudioLatencyMs { get; set; }

    /// <summary>
    /// Gets or sets the average FFT processing latency in milliseconds
    /// </summary>
    public double FFTLatencyMs { get; set; }

    /// <summary>
    /// Gets or sets the average effect processing latency in milliseconds
    /// </summary>
    public double EffectLatencyMs { get; set; }

    /// <summary>
    /// Gets or sets the total end-to-end latency in milliseconds
    /// </summary>
    public double TotalLatencyMs { get; set; }

    /// <summary>
    /// Gets or sets the number of frames processed
    /// </summary>
    public long FrameCount { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when metrics were captured
    /// </summary>
    public DateTime Timestamp { get; set; }
}
