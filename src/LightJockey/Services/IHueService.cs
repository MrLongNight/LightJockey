using LightJockey.Models;

namespace LightJockey.Services;

/// <summary>
/// Service for managing Philips Hue bridges and lights via HTTPS
/// </summary>
public interface IHueService : IDisposable
{
    /// <summary>
    /// Discovers Philips Hue bridges on the local network
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of discovered bridges</returns>
    Task<IReadOnlyList<HueBridge>> DiscoverBridgesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers the application with a Hue bridge (requires pressing the link button)
    /// </summary>
    /// <param name="bridge">The bridge to register with</param>
    /// <param name="appName">Application name for registration</param>
    /// <param name="deviceName">Device name for registration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication result containing the app key</returns>
    Task<HueAuthResult> RegisterAsync(
        HueBridge bridge,
        string appName,
        string deviceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Connects to a Hue bridge using an existing app key.
    /// If appKey is not provided, it will try to retrieve it from secure storage.
    /// </summary>
    /// <param name="bridge">The bridge to connect to</param>
    /// <param name="appKey">Optional application key. If null, will try to use stored key.</param>
    /// <returns>True if connection was successful</returns>
    Task<bool> ConnectAsync(HueBridge bridge, string? appKey = null);

    /// <summary>
    /// Gets all lights available on the connected bridge
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of available lights</returns>
    Task<IReadOnlyList<HueLight>> GetLightsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Turns a light on or off
    /// </summary>
    /// <param name="lightId">ID of the light</param>
    /// <param name="isOn">True to turn on, false to turn off</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetLightOnOffAsync(string lightId, bool isOn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the brightness of a light
    /// </summary>
    /// <param name="lightId">ID of the light</param>
    /// <param name="brightness">Brightness level (0-254)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetLightBrightnessAsync(string lightId, byte brightness, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the color of a light
    /// </summary>
    /// <param name="lightId">ID of the light</param>
    /// <param name="color">RGB color to set</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetLightColorAsync(string lightId, HueColor color, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value indicating whether the service is connected to a bridge
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the currently connected bridge
    /// </summary>
    HueBridge? ConnectedBridge { get; }
}
