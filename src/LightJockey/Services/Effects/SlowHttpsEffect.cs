using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services.Effects;

/// <summary>
/// Slow HTTPS-based effect using the HueService for audio-reactive lighting
/// </summary>
public class SlowHttpsEffect : IEffectPlugin
{
    private readonly ILogger<SlowHttpsEffect> _logger;
    private readonly IHueService _hueService;
    private EffectConfig _config = new();
    private EffectState _state = EffectState.Uninitialized;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private IReadOnlyList<HueLight>? _lights;
    private readonly object _lock = new();
    private double _currentHue;
    private double _currentBrightness;
    private bool _disposed;

    /// <summary>
    /// Event raised when the effect state changes
    /// </summary>
    public event EventHandler<EffectState>? StateChanged;

    /// <summary>
    /// Gets the unique name of this effect plugin
    /// </summary>
    public string Name => "SlowHttpsEffect";

    /// <summary>
    /// Gets a description of what this effect does
    /// </summary>
    public string Description => "Slow HTTPS-based audio-reactive lighting effect using standard Hue API";

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
    /// Initializes a new instance of the SlowHttpsEffect class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="hueService">Hue service for light control</param>
    public SlowHttpsEffect(ILogger<SlowHttpsEffect> logger, IHueService hueService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hueService = hueService ?? throw new ArgumentNullException(nameof(hueService));
        _currentHue = 0;
        _currentBrightness = 0.5;
    }

    /// <summary>
    /// Initializes the effect with the given configuration
    /// </summary>
    /// <param name="config">Effect configuration</param>
    /// <returns>True if initialization was successful</returns>
    public async Task<bool> InitializeAsync(EffectConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (!_hueService.IsConnected)
        {
            _logger.LogError("HueService is not connected to a bridge");
            State = EffectState.Error;
            return false;
        }

        try
        {
            _config = config;
            _lights = await _hueService.GetLightsAsync();

            if (_lights.Count == 0)
            {
                _logger.LogWarning("No lights available for SlowHttpsEffect");
            }

            State = EffectState.Initialized;
            _logger.LogInformation("SlowHttpsEffect initialized with {LightCount} lights", _lights.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SlowHttpsEffect");
            State = EffectState.Error;
            return false;
        }
    }

    /// <summary>
    /// Starts the effect
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (State != EffectState.Initialized && State != EffectState.Stopped)
        {
            throw new InvalidOperationException($"Cannot start effect in state {State}");
        }

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _updateTask = Task.Run(() => RunEffectLoopAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

        State = EffectState.Running;
        _logger.LogInformation("SlowHttpsEffect started");
        return Task.CompletedTask;
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
        _logger.LogInformation("SlowHttpsEffect stopped");
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

        _logger.LogDebug("SlowHttpsEffect configuration updated");
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
            // Map low frequencies to red/orange hues (0-60)
            // Map mid frequencies to green/yellow hues (60-180)
            // Map high frequencies to blue/purple hues (240-300)
            var totalEnergy = spectralData.LowFrequencyEnergy + spectralData.MidFrequencyEnergy + spectralData.HighFrequencyEnergy;
            
            if (totalEnergy > 0)
            {
                var lowRatio = spectralData.LowFrequencyEnergy / totalEnergy;
                var midRatio = spectralData.MidFrequencyEnergy / totalEnergy;
                var highRatio = spectralData.HighFrequencyEnergy / totalEnergy;

                // Calculate hue based on dominant frequency band
                if (lowRatio > midRatio && lowRatio > highRatio)
                {
                    _currentHue = 0 + (lowRatio * 60); // Red to orange
                }
                else if (midRatio > highRatio)
                {
                    _currentHue = 60 + (midRatio * 120); // Yellow to green
                }
                else
                {
                    _currentHue = 240 + (highRatio * 60); // Blue to purple
                }

                // Adjust brightness based on total energy
                var sensitivity = _config.AudioSensitivity;
                _currentBrightness = Math.Clamp(totalEnergy * sensitivity * 2, 0.1, 1.0);
            }
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

        // Flash brightness on beat
        lock (_lock)
        {
            _currentBrightness = Math.Min(_currentBrightness * 1.3, 1.0);
        }
    }

    private async Task RunEffectLoopAsync(CancellationToken cancellationToken)
    {
        // Update interval for HTTPS effect (slower, ~2-5 updates per second)
        var updateInterval = TimeSpan.FromMilliseconds(200);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await UpdateLightsAsync(cancellationToken);
                await Task.Delay(updateInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SlowHttpsEffect loop");
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }

    private async Task UpdateLightsAsync(CancellationToken cancellationToken)
    {
        if (_lights == null || _lights.Count == 0)
        {
            return;
        }

        double hue, brightness;
        lock (_lock)
        {
            hue = _currentHue;
            brightness = _currentBrightness * _config.Brightness * _config.Intensity;
        }

        // Convert HSV to RGB (simplified)
        var color = HsvToRgb(hue, 1.0, brightness);

        // Update all lights
        foreach (var light in _lights)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                // Turn on light if off
                if (!light.IsOn)
                {
                    await _hueService.SetLightOnOffAsync(light.Id, true, cancellationToken);
                }

                // Set color
                await _hueService.SetLightColorAsync(light.Id, color, cancellationToken);

                // Set brightness
                var brightnessValue = (byte)Math.Clamp(brightness * 254, 1, 254);
                await _hueService.SetLightBrightnessAsync(light.Id, brightnessValue, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update light {LightId}", light.Id);
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
