namespace LightJockey.Models;

/// <summary>
/// Represents a Philips Hue Bridge discovered on the network
/// </summary>
public class HueBridge
{
    /// <summary>
    /// Gets or sets the IP address of the bridge
    /// </summary>
    public required string IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the unique bridge identifier
    /// </summary>
    public required string BridgeId { get; set; }

    /// <summary>
    /// Gets or sets the friendly name of the bridge (if available)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the model ID of the bridge
    /// </summary>
    public string? ModelId { get; set; }
}
