using LightJockey.Models;

namespace LightJockey.Services;

/// <summary>
/// Service for managing Philips Hue Entertainment V2 streaming via DTLS/UDP
/// </summary>
public interface IEntertainmentService : IDisposable
{
    /// <summary>
    /// Event raised when streaming starts
    /// </summary>
    event EventHandler? StreamingStarted;

    /// <summary>
    /// Event raised when streaming stops
    /// </summary>
    event EventHandler? StreamingStopped;

    /// <summary>
    /// Event raised when a streaming error occurs
    /// </summary>
    event EventHandler<string>? StreamingError;

    /// <summary>
    /// Gets all entertainment areas available on the connected bridge
    /// </summary>
    /// <param name="bridge">The bridge to query</param>
    /// <param name="appKey">The application key for authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of available entertainment areas</returns>
    Task<IReadOnlyList<EntertainmentArea>> GetEntertainmentAreasAsync(
        HueBridge bridge,
        string appKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes streaming for a specific entertainment area
    /// </summary>
    /// <param name="bridge">The bridge to connect to</param>
    /// <param name="appKey">The application key for authentication</param>
    /// <param name="entertainmentKey">The entertainment key (clientkey) from registration</param>
    /// <param name="entertainmentArea">The entertainment area to activate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if initialization was successful</returns>
    Task<bool> InitializeAsync(
        HueBridge bridge,
        string appKey,
        string entertainmentKey,
        EntertainmentArea entertainmentArea,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts streaming to the entertainment area
    /// </summary>
    /// <param name="configuration">Configuration for the streaming session</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StartStreamingAsync(
        LightJockeyEntertainmentConfig configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the current streaming session
    /// </summary>
    Task StopStreamingAsync();

    /// <summary>
    /// Updates colors for all channels in the entertainment area
    /// </summary>
    /// <param name="channels">Channel data with colors and brightness</param>
    void UpdateChannels(IEnumerable<LightChannel> channels);

    /// <summary>
    /// Updates a single channel's color and brightness
    /// </summary>
    /// <param name="channelIndex">Index of the channel to update</param>
    /// <param name="color">RGB color to set</param>
    /// <param name="brightness">Brightness level (0.0 - 1.0)</param>
    void UpdateChannel(byte channelIndex, HueColor color, double brightness = 1.0);

    /// <summary>
    /// Gets a value indicating whether streaming is currently active
    /// </summary>
    bool IsStreaming { get; }

    /// <summary>
    /// Gets the currently active entertainment area
    /// </summary>
    EntertainmentArea? ActiveArea { get; }

    /// <summary>
    /// Gets the current configuration
    /// </summary>
    LightJockeyEntertainmentConfig? Configuration { get; }

    /// <summary>
    /// Gets the current frame rate (actual FPS)
    /// </summary>
    double CurrentFrameRate { get; }
}
