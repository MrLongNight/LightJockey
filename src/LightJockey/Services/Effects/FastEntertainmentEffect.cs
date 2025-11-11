using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services.Effects;

/// <summary>
/// Fast Entertainment V2-based effect using DTLS/UDP for high-performance audio-reactive lighting
/// </summary>
public class FastEntertainmentEffect : IEffectPlugin
{
    private readonly ILogger<FastEntertainmentEffect> _logger;
    private readonly IEntertainmentService _entertainmentService;
    private EffectConfig _config = new();
    private EffectState _state = EffectState.Uninitialized;
    private readonly object _lock = new();
    private readonly Dictionary<byte, (double hue, double brightness)> _channelStates = new();
    private bool _disposed;

    /// <summary>
    /// Event raised when the effect state changes
    /// </summary>
    public event EventHandler<EffectState>? StateChanged;

    /// <summary>
    /// Gets the unique name of this effect plugin
    /// </summary>
    public string Name => "FastEntertainmentEffect";

    /// <summary>
    /// Gets a description of what this effect does
    /// </summary>
    public string Description => "High-performance DTLS/UDP audio-reactive lighting using Entertainment V2";

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
    /// Initializes a new instance of the FastEntertainmentEffect class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="entertainmentService">Entertainment service for streaming</param>
    public FastEntertainmentEffect(ILogger<FastEntertainmentEffect> logger, IEntertainmentService entertainmentService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _entertainmentService = entertainmentService ?? throw new ArgumentNullException(nameof(entertainmentService));
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

            // Initialize channel states
            if (_entertainmentService.ActiveArea != null)
            {
                for (byte i = 0; i < _entertainmentService.ActiveArea.ChannelCount; i++)
                {
                    _channelStates[i] = (0, 0.5);
                }
            }

            State = EffectState.Initialized;
            _logger.LogInformation("FastEntertainmentEffect initialized");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize FastEntertainmentEffect");
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

        State = EffectState.Running;
        _logger.LogInformation("FastEntertainmentEffect started");
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

        // Note: We don't stop the entertainment service here as other effects might use it
        // The service should be managed at a higher level

        State = EffectState.Stopped;
        _logger.LogInformation("FastEntertainmentEffect stopped");
        await Task.CompletedTask;
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

        _logger.LogDebug("FastEntertainmentEffect configuration updated");
    }

    /// <summary>
    /// Handles spectral data from audio analysis
    /// </summary>
    /// <param name="spectralData">Spectral data event arguments</param>
    public void OnSpectralData(SpectralDataEventArgs spectralData)
    {
        if (!_config.AudioReactive || !_entertainmentService.IsStreaming)
        {
            return;
        }

        lock (_lock)
        {
            var totalEnergy = spectralData.LowFrequencyEnergy + spectralData.MidFrequencyEnergy + spectralData.HighFrequencyEnergy;

            if (totalEnergy > 0)
            {
                var lowRatio = spectralData.LowFrequencyEnergy / totalEnergy;
                var midRatio = spectralData.MidFrequencyEnergy / totalEnergy;
                var highRatio = spectralData.HighFrequencyEnergy / totalEnergy;

                // Update each channel with frequency-based colors
                var channelCount = _channelStates.Count;
                for (byte i = 0; i < channelCount; i++)
                {
                    double hue;
                    double brightness;

                    // Distribute frequency bands across channels
                    var channelRatio = (double)i / channelCount;
                    
                    if (channelRatio < 0.33)
                    {
                        // Low frequency channels - red/orange
                        hue = lowRatio * 60;
                        brightness = spectralData.LowFrequencyEnergy * _config.AudioSensitivity;
                    }
                    else if (channelRatio < 0.66)
                    {
                        // Mid frequency channels - green/yellow
                        hue = 60 + (midRatio * 120);
                        brightness = spectralData.MidFrequencyEnergy * _config.AudioSensitivity;
                    }
                    else
                    {
                        // High frequency channels - blue/purple
                        hue = 240 + (highRatio * 60);
                        brightness = spectralData.HighFrequencyEnergy * _config.AudioSensitivity;
                    }

                    brightness = Math.Clamp(brightness * _config.Brightness * _config.Intensity, 0.0, 1.0);
                    _channelStates[i] = (hue, brightness);

                    // Update channel immediately
                    UpdateChannel(i, hue, brightness);
                }
            }
        }
    }

    /// <summary>
    /// Handles beat detection events
    /// </summary>
    /// <param name="beatData">Beat detected event arguments</param>
    public void OnBeatDetected(BeatDetectedEventArgs beatData)
    {
        if (!_config.AudioReactive || !_entertainmentService.IsStreaming)
        {
            return;
        }

        lock (_lock)
        {
            // Flash all channels on beat
            foreach (var kvp in _channelStates)
            {
                var (hue, brightness) = kvp.Value;
                var flashBrightness = Math.Min(brightness * 1.5, 1.0);
                UpdateChannel(kvp.Key, hue, flashBrightness);
            }
        }
    }

    private void UpdateChannel(byte channelIndex, double hue, double brightness)
    {
        try
        {
            var color = HsvToRgb(hue, 1.0, brightness);
            _entertainmentService.UpdateChannel(channelIndex, color, brightness);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update channel {ChannelIndex}", channelIndex);
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
