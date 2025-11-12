using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services.Effects;

/// <summary>
/// Pulse effect using DTLS/UDP - beat-synchronized pulsing
/// </summary>
public class PulseEffect : IEffectPlugin
{
    private readonly ILogger<PulseEffect> _logger;
    private readonly IEntertainmentService _entertainmentService;
    private EffectConfig _config = new();
    private EffectState _state = EffectState.Uninitialized;
    private readonly object _lock = new();
    private double _pulsePhase;
    private double _pulseIntensity;
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
    public string Name => "PulseEffect";

    /// <summary>
    /// Gets a description of what this effect does
    /// </summary>
    public string Description => "DTLS - Beat-synchronized pulsing effect";

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
    /// Initializes a new instance of the PulseEffect class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="entertainmentService">Entertainment service for streaming</param>
    public PulseEffect(ILogger<PulseEffect> logger, IEntertainmentService entertainmentService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _entertainmentService = entertainmentService ?? throw new ArgumentNullException(nameof(entertainmentService));
        _pulsePhase = 0;
        _pulseIntensity = 0.5;
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
            _logger.LogInformation("PulseEffect initialized");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize PulseEffect");
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
        _logger.LogInformation("PulseEffect started");
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
        _logger.LogInformation("PulseEffect stopped");
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

        _logger.LogDebug("PulseEffect configuration updated");
    }

    /// <summary>
    /// Handles spectral data from audio analysis
    /// </summary>
    /// <param name="spectralData">Spectral data event arguments</param>
    public void OnSpectralData(SpectralDataEventArgs spectralData)
    {
        if (!_config.AudioReactive)
        {
            return;
        }

        lock (_lock)
        {
            // Modulate pulse intensity based on total audio energy
            var totalEnergy = spectralData.TotalEnergy;
            _pulseIntensity = Math.Clamp(totalEnergy * _config.AudioSensitivity, 0.2, 1.0);
        }
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

        // Reset pulse phase on beat for synchronized pulsing
        lock (_lock)
        {
            _pulsePhase = 0;
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
                _logger.LogError(ex, "Error in PulseEffect loop");
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
        double maxBrightness;
        double pulseIntensity;
        lock (_lock)
        {
            speed = _config.Speed;
            maxBrightness = _config.Brightness * _config.Intensity;
            pulseIntensity = _pulseIntensity;
            
            // Advance pulse phase
            _pulsePhase += speed * 0.05;
            if (_pulsePhase > Math.PI * 2)
            {
                _pulsePhase -= Math.PI * 2;
            }
        }

        // Calculate brightness using sine wave for smooth pulsing
        var pulseBrightness = (Math.Sin(_pulsePhase) + 1) / 2; // 0 to 1
        var brightness = pulseBrightness * maxBrightness * pulseIntensity;

        // Magenta/purple color for pulse effect
        var hue = 300.0;
        var color = HsvToRgb(hue, 1.0, brightness);

        var channelCount = _entertainmentService.ActiveArea.ChannelCount;
        
        // Update all channels with the same pulse
        for (byte i = 0; i < channelCount; i++)
        {
            try
            {
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
