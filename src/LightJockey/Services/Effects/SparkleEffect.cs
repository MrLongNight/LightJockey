using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services.Effects;

/// <summary>
/// Sparkle effect using DTLS/UDP - random twinkling lights
/// </summary>
public class SparkleEffect : IEffectPlugin
{
    private readonly ILogger<SparkleEffect> _logger;
    private readonly IEntertainmentService _entertainmentService;
    private EffectConfig _config = new();
    private EffectState _state = EffectState.Uninitialized;
    private readonly object _lock = new();
    private readonly Random _random = new();
    private readonly Dictionary<byte, double> _channelBrightness = new();
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
    public string Name => "SparkleEffect";

    /// <summary>
    /// Gets a description of what this effect does
    /// </summary>
    public string Description => "DTLS - Random twinkling sparkle effect";

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
    /// Initializes a new instance of the SparkleEffect class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="entertainmentService">Entertainment service for streaming</param>
    public SparkleEffect(ILogger<SparkleEffect> logger, IEntertainmentService entertainmentService)
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
            _logger.LogInformation("SparkleEffect initialized");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SparkleEffect");
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
        _logger.LogInformation("SparkleEffect started");
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
        _logger.LogInformation("SparkleEffect stopped");
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

        _logger.LogDebug("SparkleEffect configuration updated");
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
            // Increase sparkle frequency based on audio energy
            var totalEnergy = spectralData.TotalEnergy;
            if (totalEnergy > 0.5 && _random.NextDouble() < totalEnergy * _config.AudioSensitivity)
            {
                // Trigger random sparkle on high energy
                TriggerRandomSparkle();
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

        // Trigger multiple sparkles on beat
        lock (_lock)
        {
            for (int i = 0; i < 3; i++)
            {
                TriggerRandomSparkle();
            }
        }
    }

    private void TriggerRandomSparkle()
    {
        if (_channelBrightness.Count == 0)
        {
            return;
        }

        // Pick a random channel to sparkle
        var channelIndex = (byte)_random.Next(_channelBrightness.Count);
        _channelBrightness[channelIndex] = 1.0;
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
                _logger.LogError(ex, "Error in SparkleEffect loop");
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
        lock (_lock)
        {
            speed = _config.Speed;
            maxBrightness = _config.Brightness * _config.Intensity;

            // Randomly trigger new sparkles based on speed
            var sparkleChance = speed * 0.02; // Higher speed = more sparkles
            if (_random.NextDouble() < sparkleChance)
            {
                TriggerRandomSparkle();
            }

            // Decay existing sparkles
            foreach (var key in _channelBrightness.Keys.ToList())
            {
                if (_channelBrightness[key] > 0)
                {
                    _channelBrightness[key] -= 0.05; // Fade out
                    if (_channelBrightness[key] < 0)
                    {
                        _channelBrightness[key] = 0;
                    }
                }
            }
        }

        var channelCount = _entertainmentService.ActiveArea.ChannelCount;
        
        // White/yellow sparkle color
        var hue = 60.0; // Yellow hue
        
        // Update all channels
        for (byte i = 0; i < channelCount; i++)
        {
            try
            {
                double channelBrightness;
                lock (_lock)
                {
                    channelBrightness = _channelBrightness.ContainsKey(i) ? _channelBrightness[i] : 0;
                }

                var brightness = channelBrightness * maxBrightness;
                var color = HsvToRgb(hue, 0.3, brightness); // Low saturation for white sparkle

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
