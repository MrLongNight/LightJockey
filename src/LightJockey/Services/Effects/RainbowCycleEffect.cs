using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services.Effects;

/// <summary>
/// Rainbow cycle effect using HTTPS API - smooth color transitions through the spectrum
/// </summary>
public class RainbowCycleEffect : IEffectPlugin
{
    private readonly ILogger<RainbowCycleEffect> _logger;
    private readonly IHueService _hueService;
    private EffectConfig _config = new();
    private EffectState _state = EffectState.Uninitialized;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private IReadOnlyList<HueLight>? _lights;
    private readonly object _lock = new();
    private double _currentHue;
    private bool _disposed;

    /// <summary>
    /// Event raised when the effect state changes
    /// </summary>
    public event EventHandler<EffectState>? StateChanged;

    /// <summary>
    /// Gets the unique name of this effect plugin
    /// </summary>
    public string Name => "RainbowCycleEffect";

    /// <summary>
    /// Gets a description of what this effect does
    /// </summary>
    public string Description => "HTTPS - Smooth rainbow color cycling through the spectrum";

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
    /// Initializes a new instance of the RainbowCycleEffect class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="hueService">Hue service for light control</param>
    public RainbowCycleEffect(ILogger<RainbowCycleEffect> logger, IHueService hueService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hueService = hueService ?? throw new ArgumentNullException(nameof(hueService));
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
                _logger.LogWarning("No lights available for RainbowCycleEffect");
            }

            State = EffectState.Initialized;
            _logger.LogInformation("RainbowCycleEffect initialized with {LightCount} lights", _lights.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RainbowCycleEffect");
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
        _logger.LogInformation("RainbowCycleEffect started");
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
        _logger.LogInformation("RainbowCycleEffect stopped");
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

        _logger.LogDebug("RainbowCycleEffect configuration updated");
    }

    /// <summary>
    /// Handles spectral data from audio analysis
    /// </summary>
    /// <param name="spectralData">Spectral data event arguments</param>
    public void OnSpectralData(SpectralDataEventArgs spectralData)
    {
        // Rainbow cycle effect doesn't react to spectral data
    }

    /// <summary>
    /// Handles beat detection events
    /// </summary>
    /// <param name="beatData">Beat detected event arguments</param>
    public void OnBeatDetected(BeatDetectedEventArgs beatData)
    {
        // Rainbow cycle effect doesn't react to beats
    }

    private async Task RunEffectLoopAsync(CancellationToken cancellationToken)
    {
        // Update interval for HTTPS effect
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
                _logger.LogError(ex, "Error in RainbowCycleEffect loop");
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

        // Advance the hue based on speed
        double speed;
        double brightness;
        lock (_lock)
        {
            speed = _config.Speed;
            brightness = _config.Brightness * _config.Intensity;
            _currentHue = (_currentHue + speed * 2) % 360;
        }

        // Create rainbow effect by offsetting each light's hue
        for (int i = 0; i < _lights.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var light = _lights[i];
            
            try
            {
                // Calculate hue offset for this light
                var hueOffset = (360.0 / _lights.Count) * i;
                var lightHue = (_currentHue + hueOffset) % 360;

                // Convert HSV to RGB
                var color = HsvToRgb(lightHue, 1.0, brightness);

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
