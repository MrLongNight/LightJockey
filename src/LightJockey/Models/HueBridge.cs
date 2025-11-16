namespace LightJockey.Models;

/// <summary>
/// Represents a Philips Hue Bridge discovered on the network
/// </summary>
public class HueBridge
{
    /// <summary>
    /// Gets or sets the IP address of the bridge
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique bridge identifier
    /// </summary>
    public string BridgeId { get; set; } = string.Empty;

    /// <summary>
    /// Legacy Id property for test compatibility
    /// </summary>
    public string Id
    {
        get => BridgeId;
        set => BridgeId = value;
    }

    /// <summary>
    /// Gets or sets the friendly name of the bridge (if available)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the model ID of the bridge
    /// </summary>
    public string? ModelId { get; set; }
}
