using LightJockey.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;

namespace LightJockey.Services;

/// <summary>
/// Implementation of performance metrics tracking service
/// Thread-safe tracking of FPS, latency, and performance indicators
/// </summary>
public class PerformanceMetricsService : IPerformanceMetricsService
{
    private readonly ILogger<PerformanceMetricsService> _logger;
    private readonly IMetricsService _metricsService;
    private readonly object _lock = new();
    private readonly Stopwatch _fpsStopwatch = new();
    private readonly Stopwatch _frameStopwatch = new();

    private long _frameCount;
    private double _streamingFPS;
    private double _audioLatencyMs;
    private double _fftLatencyMs;
    private double _effectLatencyMs;

    // Moving average tracking
    private readonly int _windowSize = 30; // 30 samples for moving average
    private readonly Queue<double> _audioLatencyWindow = new();
    private readonly Queue<double> _fftLatencyWindow = new();
    private readonly Queue<double> _effectLatencyWindow = new();

    public PerformanceMetricsService(ILogger<PerformanceMetricsService> logger, IMetricsService metricsService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metricsService = metricsService;
        _logger.LogDebug("PerformanceMetricsService initialized");
    }

    /// <inheritdoc/>
    public double StreamingFPS
    {
        get
        {
            lock (_lock)
            {
                return _streamingFPS;
            }
        }
    }

    /// <inheritdoc/>
    public double AudioLatencyMs
    {
        get
        {
            lock (_lock)
            {
                return _audioLatencyMs;
            }
        }
    }

    /// <inheritdoc/>
    public double FFTLatencyMs
    {
        get
        {
            lock (_lock)
            {
                return _fftLatencyMs;
            }
        }
    }

    /// <inheritdoc/>
    public double EffectLatencyMs
    {
        get
        {
            lock (_lock)
            {
                return _effectLatencyMs;
            }
        }
    }

    /// <inheritdoc/>
    public double TotalLatencyMs
    {
        get
        {
            lock (_lock)
            {
                return _audioLatencyMs + _fftLatencyMs + _effectLatencyMs;
            }
        }
    }

    /// <inheritdoc/>
    public long FrameCount
    {
        get
        {
            lock (_lock)
            {
                return _frameCount;
            }
        }
    }

    /// <summary>
    /// Gets the number of streaming frames that have ended (thread-safe atomic read).
    /// </summary>
    public long StreamingFrameCount => Interlocked.Read(ref _frameCount);

    /// <inheritdoc/>
    public PerformanceMetrics GetMetrics()
    {
        lock (_lock)
        {
            return new PerformanceMetrics
            {
                StreamingFPS = _streamingFPS,
                AudioLatencyMs = _audioLatencyMs,
                FFTLatencyMs = _fftLatencyMs,
                EffectLatencyMs = _effectLatencyMs,
                TotalLatencyMs = _audioLatencyMs + _fftLatencyMs + _effectLatencyMs,
                FrameCount = _frameCount,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <inheritdoc/>
    public void StartStreamingFrame()
    {
        lock (_lock)
        {
            if (!_fpsStopwatch.IsRunning)
            {
                _fpsStopwatch.Start();
            }

            _frameStopwatch.Restart();
        }
    }

    /// <inheritdoc/>
    public void EndStreamingFrame()
    {
        lock (_lock)
        {
            Interlocked.Increment(ref _frameCount);

            // Calculate FPS every 10 frames
            if (_frameCount % 10 == 0 && _fpsStopwatch.IsRunning)
            {
                var elapsed = _fpsStopwatch.Elapsed.TotalSeconds;
                if (elapsed > 0)
                {
                    _streamingFPS = 10.0 / elapsed;
                    _fpsStopwatch.Restart();
                }
            }
            
            // Defensive: only record metrics if service is available
            _metricsService?.RecordMetrics(GetMetrics());
        }
    }

    /// <inheritdoc/>
    public void RecordAudioLatency(double latencyMs)
    {
        if (latencyMs < 0)
            throw new ArgumentException("Latency cannot be negative", nameof(latencyMs));

        lock (_lock)
        {
            _audioLatencyWindow.Enqueue(latencyMs);
            if (_audioLatencyWindow.Count > _windowSize)
            {
                _audioLatencyWindow.Dequeue();
            }
            _audioLatencyMs = _audioLatencyWindow.Average();
        }
    }

    /// <inheritdoc/>
    public void RecordFFTLatency(double latencyMs)
    {
        if (latencyMs < 0)
            throw new ArgumentException("Latency cannot be negative", nameof(latencyMs));

        lock (_lock)
        {
            _fftLatencyWindow.Enqueue(latencyMs);
            if (_fftLatencyWindow.Count > _windowSize)
            {
                _fftLatencyWindow.Dequeue();
            }
            _fftLatencyMs = _fftLatencyWindow.Average();
        }
    }

    /// <inheritdoc/>
    public void RecordEffectLatency(double latencyMs)
    {
        if (latencyMs < 0)
            throw new ArgumentException("Latency cannot be negative", nameof(latencyMs));

        lock (_lock)
        {
            _effectLatencyWindow.Enqueue(latencyMs);
            if (_effectLatencyWindow.Count > _windowSize)
            {
                _effectLatencyWindow.Dequeue();
            }
            _effectLatencyMs = _effectLatencyWindow.Average();
        }
    }

    /// <inheritdoc/>
    public void Reset()
    {
        lock (_lock)
        {
            _frameCount = 0;
            _streamingFPS = 0;
            _audioLatencyMs = 0;
            _fftLatencyMs = 0;
            _effectLatencyMs = 0;
            _audioLatencyWindow.Clear();
            _fftLatencyWindow.Clear();
            _effectLatencyWindow.Clear();
            _fpsStopwatch.Reset();
            _frameStopwatch.Reset();

            _logger.LogDebug("Performance metrics reset");
        }
    }
}
