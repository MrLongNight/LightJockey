using LightJockey.Models;

namespace LightJockey.Services;

/// <summary>
/// Interface for performance metrics tracking service
/// Tracks FPS, latency, and other performance indicators across the application
/// </summary>
public interface IPerformanceMetricsService
{
    /// <summary>
    /// Gets the current frames per second (FPS) for entertainment streaming
    /// </summary>
    double StreamingFPS { get; }

    /// <summary>
    /// Gets the average audio processing latency in milliseconds
    /// </summary>
    double AudioLatencyMs { get; }

    /// <summary>
    /// Gets the average FFT processing latency in milliseconds
    /// </summary>
    double FFTLatencyMs { get; }

    /// <summary>
    /// Gets the average effect processing latency in milliseconds
    /// </summary>
    double EffectLatencyMs { get; }

    /// <summary>
    /// Gets the total end-to-end latency from audio input to light output in milliseconds
    /// </summary>
    double TotalLatencyMs { get; }

    /// <summary>
    /// Gets the number of frames processed
    /// </summary>
    long FrameCount { get; }

    /// <summary>
    /// Gets the current metrics snapshot
    /// </summary>
    PerformanceMetrics GetMetrics();

    /// <summary>
    /// Records the start of streaming frame processing
    /// </summary>
    void StartStreamingFrame();

    /// <summary>
    /// Records the end of streaming frame processing
    /// </summary>
    void EndStreamingFrame();

    /// <summary>
    /// Records audio processing latency
    /// </summary>
    /// <param name="latencyMs">Latency in milliseconds</param>
    void RecordAudioLatency(double latencyMs);

    /// <summary>
    /// Records FFT processing latency
    /// </summary>
    /// <param name="latencyMs">Latency in milliseconds</param>
    void RecordFFTLatency(double latencyMs);

    /// <summary>
    /// Records effect processing latency
    /// </summary>
    /// <param name="latencyMs">Latency in milliseconds</param>
    void RecordEffectLatency(double latencyMs);

    /// <summary>
    /// Resets all metrics
    /// </summary>
    void Reset();
}
