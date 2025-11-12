using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services.Effects;

/// <summary>
/// Chase effect using DTLS/UDP - sequential light chase pattern
/// </summary>
public class ChaseEffect : IEffectPlugin
{
    private readonly ILogger<ChaseEffect> _logger;
    private readonly IEntertainmentService _entertainmentService;
    private EffectConfig _config = new();
    private EffectState _state = EffectState.Uninitialized;
    private readonly object _lock = new();
    private int _currentPosition;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private bool _disposed;

    /// <summary>
    /// Event raised when the effect state changes
    /// </summary>
    public event EventHandler<EffectState>? StateChanged;

    /// <summary>
    /// Gets the unique name of this effect plugin
    /// </summary>
    public string Name => "ChaseEffect";

    /// <summary>
    /// Gets a description of what this effect does
    /// </summary>
    public string Description => "DTLS - Sequential light chase pattern";

    /// <summary>
    /// Gets the current state of the effect
    /// </summary>
    public EffectState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                StateChanged?.Invoke(this, value);
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the ChaseEffect class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="entertainmentService">Entertainment service for streaming</param>
    public ChaseEffect(ILogger<ChaseEffect> logger, IEntertainmentService entertainmentService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _entertainmentService = entertainmentService ?? throw new ArgumentNullException(nameof(entertainmentService));
        _currentPosition = 0;
    }

    /// <summary>
    /// Initializes the effect with the given configuration
    /// </summary>
    /// <param name="config">Effect configuration</param>
    /// <returns>True if initialization was successful</returns>
    public Task<bool> InitializeAsync(EffectConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        try
        {
            _config = config;
            State = EffectState.Initialized;
            _logger.LogInformation("ChaseEffect initialized");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ChaseEffect");
            State = EffectState.Error;
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Starts the effect
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (State != EffectState.Initialized && State != EffectState.Stopped)
        {
            throw new InvalidOperationException($"Cannot start effect in state {State}");
        }

        // Start Entertainment streaming if not already started
        if (!_entertainmentService.IsStreaming && _entertainmentService.Configuration != null)
        {
            await _entertainmentService.StartStreamingAsync(_entertainmentService.Configuration, cancellationToken);
        }

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _updateTask = Task.Run(() => RunEffectLoopAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

        State = EffectState.Running;
        _logger.LogInformation("ChaseEffect started");
    }

    /// <summary>
    /// Stops the effect
    /// </summary>
    public async Task StopAsync()
    {
        if (State != EffectState.Running && State != EffectState.Paused)
        {
            return;
        }

        _cancellationTokenSource?.Cancel();

        if (_updateTask != null)
        {
            try
            {
                await _updateTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in effect loop during shutdown");
            }
        }

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _updateTask = null;

        State = EffectState.Stopped;
        _logger.LogInformation("ChaseEffect stopped");
    }

    /// <summary>
    /// Updates the effect configuration
    /// </summary>
    /// <param name="config">New configuration</param>
    public void UpdateConfig(EffectConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        lock (_lock)
        {
            _config = config;
        }

        _logger.LogDebug("ChaseEffect configuration updated");
    }

    /// <summary>
    /// Handles spectral data from audio analysis
    /// </summary>
    /// <param name="spectralData">Spectral data event arguments</param>
    public void OnSpectralData(SpectralDataEventArgs spectralData)
    {
        // Chase effect doesn't react to spectral data directly
    }

    /// <summary>
    /// Handles beat detection events
    /// </summary>
    /// <param name="beatData">Beat detected event arguments</param>
    public void OnBeatDetected(BeatDetectedEventArgs beatData)
    {
        if (!_config.AudioReactive)
        {
            return;
        }

        // Advance chase position on beat
        lock (_lock)
        {
            if (_entertainmentService.ActiveArea != null)
            {
                _currentPosition = (_currentPosition + 1) % _entertainmentService.ActiveArea.ChannelCount;
            }
        }
    }

    private async Task RunEffectLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                double speed;
                lock (_lock)
                {
                    speed = _config.Speed;
                }

                // Calculate update interval based on speed
                // Speed range is 0.1 to 5.0, map to interval range 500ms to 50ms
                var intervalMs = Math.Clamp(500 / speed, 50, 500);
                var updateInterval = TimeSpan.FromMilliseconds(intervalMs);

                UpdateLights();
                
                // Advance position if not in audio-reactive mode
                if (!_config.AudioReactive && _entertainmentService.ActiveArea != null)
                {
                    lock (_lock)
                    {
                        _currentPosition = (_currentPosition + 1) % _entertainmentService.ActiveArea.ChannelCount;
                    }
                }

                await Task.Delay(updateInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChaseEffect loop");
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }

    private void UpdateLights()
    {
        if (!_entertainmentService.IsStreaming || _entertainmentService.ActiveArea == null)
        {
            return;
        }

        double brightness;
        int position;
        lock (_lock)
        {
            brightness = _config.Brightness * _config.Intensity;
            position = _currentPosition;
        }

        var channelCount = _entertainmentService.ActiveArea.ChannelCount;
        
        // Orange color for chase effect
        var hue = 30.0;
        var onColor = HsvToRgb(hue, 1.0, brightness);
        var offColor = new HueColor { Red = 0, Green = 0, Blue = 0 };

        // Update all channels - only the current position is lit, others are dark with trailing effect
        for (byte i = 0; i < channelCount; i++)
        {
            try
            {
                HueColor color;
                double channelBrightness;

                if (i == position)
                {
                    // Active position - full brightness
                    color = onColor;
                    channelBrightness = brightness;
                }
                else
                {
                    // Calculate trailing effect
                    var distance = Math.Abs(i - position);
                    if (distance > channelCount / 2)
                    {
                        distance = channelCount - distance; // Wrap around
                    }

                    if (distance <= 2)
                    {
                        // Trailing lights with reduced brightness
                        var trailBrightness = brightness * (1.0 - (distance / 3.0));
                        color = HsvToRgb(hue, 1.0, trailBrightness);
                        channelBrightness = trailBrightness;
                    }
                    else
                    {
                        // Off
                        color = offColor;
                        channelBrightness = 0;
                    }
                }

                _entertainmentService.UpdateChannel(i, color, channelBrightness);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update channel {ChannelIndex}", i);
            }
        }
    }

    private static HueColor HsvToRgb(double hue, double saturation, double value)
    {
        var h = hue / 60.0;
        var c = value * saturation;
        var x = c * (1 - Math.Abs(h % 2 - 1));
        var m = value - c;

        double r, g, b;
        if (h < 1)
        {
            r = c; g = x; b = 0;
        }
        else if (h < 2)
        {
            r = x; g = c; b = 0;
        }
        else if (h < 3)
        {
            r = 0; g = c; b = x;
        }
        else if (h < 4)
        {
            r = 0; g = x; b = c;
        }
        else if (h < 5)
        {
            r = x; g = 0; b = c;
        }
        else
        {
            r = c; g = 0; b = x;
        }

        return new HueColor
        {
            Red = (byte)Math.Clamp((r + m) * 255, 0, 255),
            Green = (byte)Math.Clamp((g + m) * 255, 0, 255),
            Blue = (byte)Math.Clamp((b + m) * 255, 0, 255)
        };
    }

    /// <summary>
    /// Disposes the effect
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        StopAsync().GetAwaiter().GetResult();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
