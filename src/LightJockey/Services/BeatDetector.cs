using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services;

/// <summary>
/// Implementation of beat detector using energy-based onset detection
/// </summary>
public class BeatDetector : IBeatDetector
{
    private readonly ILogger<BeatDetector> _logger;
    private readonly ISpectralAnalyzer _spectralAnalyzer;
    private readonly Queue<double> _energyHistory;
    private readonly Queue<DateTime> _beatTimestamps;
    private readonly int _historySize;
    private readonly int _bpmHistorySize;
    private double _currentBPM;
    private bool _disposed;

    // Detection parameters
    private const double BeatThresholdMultiplier = 1.5; // Beat must be 1.5x average energy
    private const int MinBeatIntervalMs = 300; // Minimum 300ms between beats (200 BPM max)
    private DateTime _lastBeatTime;

    /// <inheritdoc/>
    public event EventHandler<BeatDetectedEventArgs>? BeatDetected;

    /// <inheritdoc/>
    public double CurrentBPM => _currentBPM;

    /// <summary>
    /// Initializes a new instance of the BeatDetector class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="spectralAnalyzer">Spectral analyzer instance</param>
    /// <param name="historySize">Size of energy history window (default 43, ~1 second at 43 FPS)</param>
    public BeatDetector(ILogger<BeatDetector> logger, ISpectralAnalyzer spectralAnalyzer, int historySize = 43)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _spectralAnalyzer = spectralAnalyzer ?? throw new ArgumentNullException(nameof(spectralAnalyzer));
        
        _historySize = historySize;
        _bpmHistorySize = 8; // Keep last 8 beat intervals for BPM calculation
        _energyHistory = new Queue<double>(_historySize);
        _beatTimestamps = new Queue<DateTime>(_bpmHistorySize);
        _lastBeatTime = DateTime.MinValue;

        // Subscribe to spectral data
        _spectralAnalyzer.SpectralDataAvailable += OnSpectralDataAvailable;

        _logger.LogDebug("BeatDetector initialized with history size: {HistorySize}", _historySize);
    }

    private void OnSpectralDataAvailable(object? sender, SpectralDataEventArgs e)
    {
        // Use low frequency energy for beat detection (bass)
        ProcessEnergy(e.LowFrequencyEnergy);
    }

    /// <inheritdoc/>
    public void ProcessEnergy(double energy)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(BeatDetector));
        }

        try
        {
            // Add to history
            _energyHistory.Enqueue(energy);
            if (_energyHistory.Count > _historySize)
            {
                _energyHistory.Dequeue();
            }

            // Need sufficient history for detection
            if (_energyHistory.Count < _historySize)
            {
                return;
            }

            // Calculate average energy from history
            double averageEnergy = _energyHistory.Average();

            // Detect beat if current energy exceeds threshold
            double threshold = averageEnergy * BeatThresholdMultiplier;
            bool isBeat = energy > threshold && averageEnergy > 0.001; // Avoid false positives in silence

            // Check minimum beat interval
            var now = DateTime.UtcNow;
            var timeSinceLastBeat = (now - _lastBeatTime).TotalMilliseconds;

            if (isBeat && timeSinceLastBeat >= MinBeatIntervalMs)
            {
                // Beat detected!
                _lastBeatTime = now;

                // Update BPM calculation
                _beatTimestamps.Enqueue(now);
                if (_beatTimestamps.Count > _bpmHistorySize)
                {
                    _beatTimestamps.Dequeue();
                }

                _currentBPM = CalculateBPM();

                // Calculate confidence based on how much energy exceeds threshold
                double confidence = Math.Min(energy / threshold - 1.0, 1.0);

                // Raise beat detected event
                var eventArgs = new BeatDetectedEventArgs(energy, _currentBPM, confidence);
                BeatDetected?.Invoke(this, eventArgs);

                _logger.LogTrace("Beat detected - Energy: {Energy:F2}, BPM: {BPM:F1}, Confidence: {Confidence:F2}",
                    energy, _currentBPM, confidence);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing energy for beat detection");
            throw;
        }
    }

    private double CalculateBPM()
    {
        if (_beatTimestamps.Count < 2)
        {
            return 0;
        }

        // Calculate average interval between beats
        var timestamps = _beatTimestamps.ToArray();
        double totalIntervalMs = 0;
        int intervalCount = 0;

        for (int i = 1; i < timestamps.Length; i++)
        {
            totalIntervalMs += (timestamps[i] - timestamps[i - 1]).TotalMilliseconds;
            intervalCount++;
        }

        if (intervalCount == 0)
        {
            return 0;
        }

        double averageIntervalMs = totalIntervalMs / intervalCount;
        
        // Convert to BPM (60000 ms per minute)
        return 60000.0 / averageIntervalMs;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        _energyHistory.Clear();
        _beatTimestamps.Clear();
        _currentBPM = 0;
        _lastBeatTime = DateTime.MinValue;
        _logger.LogDebug("BeatDetector reset");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _spectralAnalyzer.SpectralDataAvailable -= OnSpectralDataAvailable;
            _logger.LogDebug("BeatDetector disposed");
            _disposed = true;
        }
    }
}
