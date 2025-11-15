using HueApi;
using HueApi.ColorConverters;
using HueApi.Entertainment;
using HueApi.Entertainment.Extensions;
using HueApi.Entertainment.Models;
using HueApi.Models;
using LightJockey.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LightJockey.Services;

/// <summary>
/// Implementation of Entertainment service for Philips Hue Entertainment V2 streaming via DTLS/UDP
/// </summary>
public class EntertainmentService : IEntertainmentService
{
    private readonly ILogger<EntertainmentService> _logger;
    private readonly IAudioService _audioService;
    private StreamingHueClient? _streamingClient;
    private EntertainmentLayer? _entertainmentLayer;
    private StreamingGroup? _streamingGroup;
    private LocalHueApi? _hueApi;
    private string? _bridgeIp;
    private EntertainmentArea? _activeArea;
    private LightJockeyEntertainmentConfig? _configuration;
    private CancellationTokenSource? _streamingCancellationTokenSource;
    private Task? _streamingTask;
    private bool _disposed;
    private readonly Stopwatch _frameStopwatch = new();
    private double _currentFrameRate;
    private int _frameCount;
    private readonly object _channelLock = new();
    private readonly Dictionary<byte, (HueColor Color, double Brightness)> _channelUpdates = new();

    /// <inheritdoc/>
    public event EventHandler? StreamingStarted;

    /// <inheritdoc/>
    public event EventHandler? StreamingStopped;

    /// <inheritdoc/>
    public event EventHandler<string>? StreamingError;

    /// <inheritdoc/>
    public bool IsStreaming => _streamingTask != null && !_streamingTask.IsCompleted;

    /// <inheritdoc/>
    public EntertainmentArea? ActiveArea => _activeArea;

    /// <inheritdoc/>
    public LightJockeyEntertainmentConfig? Configuration => _configuration;

    /// <inheritdoc/>
    public double CurrentFrameRate => _currentFrameRate;

    /// <summary>
    /// Initializes a new instance of the EntertainmentService class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="audioService">Audio service for reactive lighting</param>
    public EntertainmentService(ILogger<EntertainmentService> logger, IAudioService audioService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _logger.LogDebug("EntertainmentService initialized");
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<EntertainmentArea>> GetEntertainmentAreasAsync(
        HueBridge bridge,
        string appKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting entertainment areas from bridge {BridgeId}", bridge.BridgeId);

            // Create API instance if not already connected
            if (_hueApi == null || _bridgeIp != bridge.IpAddress)
            {
                _hueApi = new LocalHueApi(bridge.IpAddress, appKey);
                _bridgeIp = bridge.IpAddress;
            }

            // Get entertainment configurations
#pragma warning disable CS0618
            var entertainmentConfigs = await _hueApi.GetEntertainmentConfigurationsAsync();
#pragma warning restore CS0618
            
            var areas = new List<EntertainmentArea>();
            
            if (entertainmentConfigs?.Data != null)
            {
                foreach (var config in entertainmentConfigs.Data)
                {
                    // Get the light IDs from the channels
                    var lightIds = config.Channels?
                        .Where(c => c.Members != null)
                        .SelectMany(c => c.Members!)
                        .Select(m => m.Service?.Rid)
                        .Where(id => id.HasValue)
                        .Select(id => id!.Value)
                        .Distinct()
                        .ToList() ?? new List<Guid>();

                    areas.Add(new EntertainmentArea
                    {
                        Id = config.Id,
                        Name = config.Metadata?.Name ?? "Unknown Area",
                        IsActive = config.Status == EntertainmentConfigurationStatus.active,
                        LightIds = lightIds.AsReadOnly()
                    });
                }
            }

            _logger.LogInformation("Found {AreaCount} entertainment areas", areas.Count);
            return areas.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get entertainment areas");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> InitializeAsync(
        HueBridge bridge,
        string appKey,
        string entertainmentKey,
        EntertainmentArea entertainmentArea,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Initializing entertainment streaming for area {AreaName}", entertainmentArea.Name);

            // Create API instance
            _hueApi = new LocalHueApi(bridge.IpAddress, appKey);
            _bridgeIp = bridge.IpAddress;

            // Get the entertainment configuration to get channel info
#pragma warning disable CS0618
            var config = await _hueApi.GetEntertainmentConfigurationAsync(entertainmentArea.Id);
#pragma warning restore CS0618
            if (config?.Data == null || !config.Data.Any())
            {
                _logger.LogError("Entertainment area {AreaId} not found", entertainmentArea.Id);
                return false;
            }

            var entertainmentConfig = config.Data.First();

            // Create streaming client with DTLS
            _streamingClient = new StreamingHueClient(bridge.IpAddress, appKey, entertainmentKey);

            // Create streaming group from channels
            _streamingGroup = new StreamingGroup(entertainmentConfig.Channels);

            // Create entertainment layer
            _entertainmentLayer = _streamingGroup.GetNewLayer(isBaseLayer: true);

            _activeArea = entertainmentArea;

            _logger.LogInformation("Entertainment service initialized for area {AreaName}", entertainmentArea.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize entertainment service");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task StartStreamingAsync(
        LightJockeyEntertainmentConfig configuration,
        CancellationToken cancellationToken = default)
    {
        if (_streamingClient == null || _activeArea == null || _streamingGroup == null)
        {
            throw new InvalidOperationException("Entertainment service not initialized. Call InitializeAsync first.");
        }

        if (IsStreaming)
        {
            _logger.LogWarning("Streaming already active");
            return;
        }

        try
        {
            _logger.LogDebug("Starting entertainment streaming");
            _configuration = configuration;

            // Connect to the entertainment area
            await _streamingClient.ConnectAsync(_activeArea.Id);

            // Subscribe to audio events if audio reactive mode is enabled
            if (configuration.AudioReactive)
            {
                _audioService.AudioDataAvailable += OnAudioDataAvailable;
            }

            // Start streaming task
            _streamingCancellationTokenSource = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                _streamingCancellationTokenSource.Token,
                cancellationToken);

            _streamingTask = Task.Run(
                () => StreamingLoop(linkedCts.Token),
                linkedCts.Token);

            StreamingStarted?.Invoke(this, EventArgs.Empty);
            _logger.LogInformation("Entertainment streaming started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start streaming");
            StreamingError?.Invoke(this, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task StopStreamingAsync()
    {
        if (!IsStreaming)
        {
            return;
        }

        try
        {
            _logger.LogDebug("Stopping entertainment streaming");

            // Unsubscribe from audio events
            _audioService.AudioDataAvailable -= OnAudioDataAvailable;

            // Cancel streaming task
            _streamingCancellationTokenSource?.Cancel();

            // Wait for streaming task to complete
            if (_streamingTask != null)
            {
                try
                {
                    await _streamingTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelling
                }
            }

            // Disconnect streaming client
            _streamingClient?.Close();

            StreamingStopped?.Invoke(this, EventArgs.Empty);
            _logger.LogInformation("Entertainment streaming stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping streaming");
            StreamingError?.Invoke(this, ex.Message);
        }
        finally
        {
            _streamingCancellationTokenSource?.Dispose();
            _streamingCancellationTokenSource = null;
            _streamingTask = null;
        }
    }

    /// <inheritdoc/>
    public void UpdateChannels(IEnumerable<LightChannel> channels)
    {
        if (_entertainmentLayer == null)
        {
            return;
        }

        lock (_channelLock)
        {
            foreach (var channel in channels)
            {
                _channelUpdates[channel.Index] = (channel.Color, channel.Brightness);
            }
        }
    }

    /// <inheritdoc/>
    public void UpdateChannel(byte channelIndex, HueColor color, double brightness = 1.0)
    {
        if (_entertainmentLayer == null)
        {
            return;
        }

        lock (_channelLock)
        {
            _channelUpdates[channelIndex] = (color, Math.Clamp(brightness, 0.0, 1.0));
        }
    }

    private async Task StreamingLoop(CancellationToken cancellationToken)
    {
        if (_streamingClient == null || _entertainmentLayer == null || _configuration == null || _streamingGroup == null)
        {
            return;
        }

        var updateRateMs = 1000 / _configuration.TargetFrameRate;
        _frameStopwatch.Start();
        var lastFrameTime = _frameStopwatch.Elapsed;

        try
        {
            // Start auto-update for the streaming group
            _ = _streamingClient.AutoUpdateAsync(_streamingGroup, cancellationToken, (int)updateRateMs, onlySendDirtyStates: false);

            while (!cancellationToken.IsCancellationRequested)
            {
                var frameStartTime = _frameStopwatch.Elapsed;

                // Apply queued channel updates
                ApplyChannelUpdates();

                // Calculate frame rate
                _frameCount++;
                if (_frameCount >= _configuration.TargetFrameRate)
                {
                    var elapsed = _frameStopwatch.Elapsed - lastFrameTime;
                    _currentFrameRate = _frameCount / elapsed.TotalSeconds;
                    _frameCount = 0;
                    lastFrameTime = _frameStopwatch.Elapsed;
                }

                // Wait for next frame
                var targetFrameTime = TimeSpan.FromMilliseconds(updateRateMs);
                var frameTime = _frameStopwatch.Elapsed - frameStartTime;
                var sleepTime = targetFrameTime - frameTime;
                
                if (sleepTime > TimeSpan.Zero)
                {
                    await Task.Delay(sleepTime, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in streaming loop");
            StreamingError?.Invoke(this, ex.Message);
        }
    }

    private void ApplyChannelUpdates()
    {
        if (_entertainmentLayer == null)
        {
            return;
        }

        Dictionary<byte, (HueColor Color, double Brightness)> updates;
        
        lock (_channelLock)
        {
            if (_channelUpdates.Count == 0)
            {
                return;
            }

            updates = new Dictionary<byte, (HueColor Color, double Brightness)>(_channelUpdates);
            _channelUpdates.Clear();
        }

        foreach (var (channelIndex, (color, brightness)) in updates)
        {
            try
            {
                // Apply brightness clamping from configuration
                var clampedBrightness = _configuration != null
                    ? Math.Clamp(brightness, _configuration.MinBrightness, _configuration.MaxBrightness)
                    : brightness;

                // Set the color with brightness for the channel
                var rgbColor = new RGBColor(color.Red / 255.0, color.Green / 255.0, color.Blue / 255.0);
                _entertainmentLayer[channelIndex].SetState(CancellationToken.None, rgbColor, clampedBrightness, TimeSpan.Zero);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update channel {ChannelIndex}", channelIndex);
            }
        }
    }

    private void OnAudioDataAvailable(object? sender, AudioDataEventArgs e)
    {
        if (_configuration == null || !_configuration.AudioReactive || _activeArea == null)
        {
            return;
        }

        try
        {
            // Calculate average amplitude
            var amplitude = e.Samples.Select(Math.Abs).Average();
            var normalizedAmplitude = Math.Clamp(amplitude * _configuration.AudioSensitivity, 0.0, 1.0);

            // Update all channels based on audio amplitude
            var channelCount = _activeArea.ChannelCount;
            for (byte i = 0; i < channelCount; i++)
            {
                // Simple white color with audio-reactive brightness
                var color = new HueColor(255, 255, 255);
                var brightness = _configuration.MinBrightness + 
                                (normalizedAmplitude * (_configuration.MaxBrightness - _configuration.MinBrightness));
                
                UpdateChannel(i, color, brightness);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing audio data for entertainment");
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            StopStreamingAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during disposal");
        }

        _streamingClient?.Dispose();
        _streamingCancellationTokenSource?.Dispose();
        _disposed = true;

        _logger.LogDebug("EntertainmentService disposed");
    }
}
