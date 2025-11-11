using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services.Effects;

/// <summary>
/// Fast Rainbow effect using DTLS/UDP - rapid color cycling through the spectrum
/// </summary>
public class RainbowFastEffect : IEffectPlugin
{
    private readonly ILogger<RainbowFastEffect> _logger;
    private readonly IEntertainmentService _entertainmentService;
    private EffectConfig _config = new();
    private EffectState _state = EffectState.Uninitialized;
    private readonly object _lock = new();
    private double _currentHue;
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
    public string Name => "RainbowFastEffect";

    /// <summary>
    /// Gets a description of what this effect does
    /// </summary>
    public string Description => "DTLS - Fast rainbow color cycling using Entertainment V2";

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
    /// Initializes a new instance of the RainbowFastEffect class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="entertainmentService">Entertainment service for streaming</param>
    public RainbowFastEffect(ILogger<RainbowFastEffect> logger, IEntertainmentService entertainmentService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _entertainmentService = entertainmentService ?? throw new ArgumentNullException(nameof(entertainmentService));
        _currentHue = 0;
    }

    /// <summary>
    /// Initializes the effect with the given configuration
    /// </summary>
    /// <param name="config">Effect configuration</param>
    /// <returns>True if initialization was successful</returns>
    public async Task<bool> InitializeAsync(EffectConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        try
        {
            _config = config;
            State = EffectState.Initialized;
            _logger.LogInformation("RainbowFastEffect initialized");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RainbowFastEffect");
            State = EffectState.Error;
            return false;
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
        _logger.LogInformation("RainbowFastEffect started");
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
        _logger.LogInformation("RainbowFastEffect stopped");
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

        _logger.LogDebug("RainbowFastEffect configuration updated");
    }

    /// <summary>
    /// Handles spectral data from audio analysis
    /// </summary>
    /// <param name="spectralData">Spectral data event arguments</param>
    public void OnSpectralData(SpectralDataEventArgs spectralData)
    {
        // Rainbow fast effect doesn't react to spectral data
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

        // Accelerate hue change on beat
        lock (_lock)
        {
            _currentHue = (_currentHue + 30) % 360;
        }
    }

    private async Task RunEffectLoopAsync(CancellationToken cancellationToken)
    {
        // Fast update interval for DTLS effect (~60 FPS)
        var updateInterval = TimeSpan.FromMilliseconds(16);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                UpdateLights();
                await Task.Delay(updateInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RainbowFastEffect loop");
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

        double speed;
        double brightness;
        lock (_lock)
        {
            speed = _config.Speed;
            brightness = _config.Brightness * _config.Intensity;
            _currentHue = (_currentHue + speed * 0.5) % 360;
        }

        var channelCount = _entertainmentService.ActiveArea.ChannelCount;
        
        // Create rainbow effect by offsetting each channel's hue
        for (byte i = 0; i < channelCount; i++)
        {
            try
            {
                // Calculate hue offset for this channel
                var hueOffset = (360.0 / channelCount) * i;
                var channelHue = (_currentHue + hueOffset) % 360;

                // Convert HSV to RGB
                var color = HsvToRgb(channelHue, 1.0, brightness);
                
                // Update channel
                _entertainmentService.UpdateChannel(i, color, brightness);
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
