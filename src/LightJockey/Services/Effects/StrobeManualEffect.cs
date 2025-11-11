using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services.Effects;

/// <summary>
/// Strobe effect using HTTPS API - configurable strobe light effect
/// </summary>
public class StrobeManualEffect : IEffectPlugin
{
    private readonly ILogger<StrobeManualEffect> _logger;
    private readonly IHueService _hueService;
    private EffectConfig _config = new();
    private EffectState _state = EffectState.Uninitialized;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private IReadOnlyList<HueLight>? _lights;
    private readonly object _lock = new();
    private bool _strobeOn = false;
    private bool _disposed;

    /// <summary>
    /// Event raised when the effect state changes
    /// </summary>
    public event EventHandler<EffectState>? StateChanged;

    /// <summary>
    /// Gets the unique name of this effect plugin
    /// </summary>
    public string Name => "StrobeManualEffect";

    /// <summary>
    /// Gets a description of what this effect does
    /// </summary>
    public string Description => "HTTPS - Configurable strobe light effect";

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
    /// Initializes a new instance of the StrobeManualEffect class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="hueService">Hue service for light control</param>
    public StrobeManualEffect(ILogger<StrobeManualEffect> logger, IHueService hueService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hueService = hueService ?? throw new ArgumentNullException(nameof(hueService));
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
                _logger.LogWarning("No lights available for StrobeManualEffect");
            }

            State = EffectState.Initialized;
            _logger.LogInformation("StrobeManualEffect initialized with {LightCount} lights", _lights.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize StrobeManualEffect");
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
        _logger.LogInformation("StrobeManualEffect started");
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
        _logger.LogInformation("StrobeManualEffect stopped");
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

        _logger.LogDebug("StrobeManualEffect configuration updated");
    }

    /// <summary>
    /// Handles spectral data from audio analysis
    /// </summary>
    /// <param name="spectralData">Spectral data event arguments</param>
    public void OnSpectralData(SpectralDataEventArgs spectralData)
    {
        // Strobe manual effect doesn't react to spectral data
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

        // Synchronize strobe with beats
        lock (_lock)
        {
            _strobeOn = !_strobeOn;
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

                // Calculate strobe interval based on speed (faster speed = shorter interval)
                // Speed range is 0.1 to 5.0, map to interval range 500ms to 50ms
                var intervalMs = Math.Clamp(500 / speed, 50, 500);
                var updateInterval = TimeSpan.FromMilliseconds(intervalMs);

                // Toggle strobe state
                lock (_lock)
                {
                    _strobeOn = !_strobeOn;
                }

                await UpdateLightsAsync(cancellationToken);
                await Task.Delay(updateInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StrobeManualEffect loop");
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

        bool strobeState;
        double brightness;
        lock (_lock)
        {
            strobeState = _strobeOn;
            brightness = _config.Brightness * _config.Intensity;
        }

        // White color for strobe effect
        var color = new HueColor { Red = 255, Green = 255, Blue = 255 };

        // Update all lights
        foreach (var light in _lights)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                if (strobeState)
                {
                    // Turn on light
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
                else
                {
                    // Turn off light for strobe effect
                    await _hueService.SetLightOnOffAsync(light.Id, false, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update light {LightId}", light.Id);
            }
        }
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
