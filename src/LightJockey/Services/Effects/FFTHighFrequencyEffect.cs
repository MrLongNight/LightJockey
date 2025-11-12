using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services.Effects;

/// <summary>
/// FFT High Frequency effect using DTLS/UDP - treble-reactive lighting
/// </summary>
public class FFTHighFrequencyEffect : IEffectPlugin
{
    private readonly ILogger<FFTHighFrequencyEffect> _logger;
    private readonly IEntertainmentService _entertainmentService;
    private EffectConfig _config = new();
    private EffectState _state = EffectState.Uninitialized;
    private readonly object _lock = new();
    private readonly Dictionary<byte, double> _channelBrightness = new();
    private bool _disposed;

    /// <summary>
    /// Event raised when the effect state changes
    /// </summary>
    public event EventHandler<EffectState>? StateChanged;

    /// <summary>
    /// Gets the unique name of this effect plugin
    /// </summary>
    public string Name => "FFTHighFrequencyEffect";

    /// <summary>
    /// Gets a description of what this effect does
    /// </summary>
    public string Description => "DTLS - Treble-reactive lighting using high frequency FFT data";

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
    /// Initializes a new instance of the FFTHighFrequencyEffect class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="entertainmentService">Entertainment service for streaming</param>
    public FFTHighFrequencyEffect(ILogger<FFTHighFrequencyEffect> logger, IEntertainmentService entertainmentService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _entertainmentService = entertainmentService ?? throw new ArgumentNullException(nameof(entertainmentService));
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

            // Initialize channel brightness states
            if (_entertainmentService.ActiveArea != null)
            {
                for (byte i = 0; i < _entertainmentService.ActiveArea.ChannelCount; i++)
                {
                    _channelBrightness[i] = 0.0;
                }
            }

            State = EffectState.Initialized;
            _logger.LogInformation("FFTHighFrequencyEffect initialized");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize FFTHighFrequencyEffect");
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

        State = EffectState.Running;
        _logger.LogInformation("FFTHighFrequencyEffect started");
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

        State = EffectState.Stopped;
        _logger.LogInformation("FFTHighFrequencyEffect stopped");
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

        _logger.LogDebug("FFTHighFrequencyEffect configuration updated");
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
            // React to high frequency energy (treble)
            var highEnergy = spectralData.HighFrequencyEnergy;
            var sensitivity = _config.AudioSensitivity;
            var brightness = Math.Clamp(highEnergy * sensitivity * 2, 0.0, 1.0);

            // Update all channels with blue/purple color for high frequencies
            var hue = 240.0; // Blue hue
            var channelCount = _channelBrightness.Count;
            
            for (byte i = 0; i < channelCount; i++)
            {
                _channelBrightness[i] = brightness;
                var finalBrightness = brightness * _config.Brightness * _config.Intensity;
                var color = HsvToRgb(hue, 1.0, finalBrightness);
                UpdateChannel(i, color, finalBrightness);
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
            // Flash all channels on beat with cyan color
            var hue = 180.0; // Cyan hue
            foreach (var kvp in _channelBrightness)
            {
                var flashBrightness = Math.Min(kvp.Value * 1.5, 1.0);
                var color = HsvToRgb(hue, 1.0, flashBrightness);
                UpdateChannel(kvp.Key, color, flashBrightness);
            }
        }
    }

    private void UpdateChannel(byte channelIndex, HueColor color, double brightness)
    {
        try
        {
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
