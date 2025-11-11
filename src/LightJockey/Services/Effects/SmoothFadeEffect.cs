using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services.Effects;

/// <summary>
/// Smooth fade effect using HTTPS API - gradual brightness fade in and out
/// </summary>
public class SmoothFadeEffect : IEffectPlugin
{
    private readonly ILogger<SmoothFadeEffect> _logger;
    private readonly IHueService _hueService;
    private EffectConfig _config = new();
    private EffectState _state = EffectState.Uninitialized;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private IReadOnlyList<HueLight>? _lights;
    private readonly object _lock = new();
    private double _currentBrightness;
    private bool _fadingUp = true;
    private bool _disposed;

    /// <summary>
    /// Event raised when the effect state changes
    /// </summary>
    public event EventHandler<EffectState>? StateChanged;

    /// <summary>
    /// Gets the unique name of this effect plugin
    /// </summary>
    public string Name => "SmoothFadeEffect";

    /// <summary>
    /// Gets a description of what this effect does
    /// </summary>
    public string Description => "HTTPS - Gradual brightness fade in and out";

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
    /// Initializes a new instance of the SmoothFadeEffect class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="hueService">Hue service for light control</param>
    public SmoothFadeEffect(ILogger<SmoothFadeEffect> logger, IHueService hueService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hueService = hueService ?? throw new ArgumentNullException(nameof(hueService));
        _currentBrightness = 0.0;
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
                _logger.LogWarning("No lights available for SmoothFadeEffect");
            }

            State = EffectState.Initialized;
            _logger.LogInformation("SmoothFadeEffect initialized with {LightCount} lights", _lights.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SmoothFadeEffect");
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
        _logger.LogInformation("SmoothFadeEffect started");
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
        _logger.LogInformation("SmoothFadeEffect stopped");
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

        _logger.LogDebug("SmoothFadeEffect configuration updated");
    }

    /// <summary>
    /// Handles spectral data from audio analysis
    /// </summary>
    /// <param name="spectralData">Spectral data event arguments</param>
    public void OnSpectralData(SpectralDataEventArgs spectralData)
    {
        // Smooth fade effect doesn't react to spectral data unless audio reactive is enabled
        if (!_config.AudioReactive)
        {
            return;
        }

        lock (_lock)
        {
            // Modulate fade speed based on audio energy
            var totalEnergy = spectralData.TotalEnergy;
            if (totalEnergy > 0.5)
            {
                // Speed up fade on high energy
                _fadingUp = !_fadingUp;
            }
        }
    }

    /// <summary>
    /// Handles beat detection events
    /// </summary>
    /// <param name="beatData">Beat detected event arguments</param>
    public void OnBeatDetected(BeatDetectedEventArgs beatData)
    {
        // Smooth fade effect doesn't react to beats
    }

    private async Task RunEffectLoopAsync(CancellationToken cancellationToken)
    {
        // Update interval for HTTPS effect
        var updateInterval = TimeSpan.FromMilliseconds(100);

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
                _logger.LogError(ex, "Error in SmoothFadeEffect loop");
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

        double speed;
        double maxBrightness;
        lock (_lock)
        {
            speed = _config.Speed;
            maxBrightness = _config.Brightness * _config.Intensity;

            // Update brightness based on direction
            var fadeStep = speed * 0.01;
            if (_fadingUp)
            {
                _currentBrightness += fadeStep;
                if (_currentBrightness >= maxBrightness)
                {
                    _currentBrightness = maxBrightness;
                    _fadingUp = false;
                }
            }
            else
            {
                _currentBrightness -= fadeStep;
                if (_currentBrightness <= 0)
                {
                    _currentBrightness = 0;
                    _fadingUp = true;
                }
            }
        }

        // White color for fade effect
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
                // Turn on light if off
                if (!light.IsOn)
                {
                    await _hueService.SetLightOnOffAsync(light.Id, true, cancellationToken);
                }

                // Set color
                await _hueService.SetLightColorAsync(light.Id, color, cancellationToken);

                // Set brightness
                var brightnessValue = (byte)Math.Clamp(_currentBrightness * 254, 1, 254);
                await _hueService.SetLightBrightnessAsync(light.Id, brightnessValue, cancellationToken);
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
