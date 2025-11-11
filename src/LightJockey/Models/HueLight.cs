namespace LightJockey.Models;

/// <summary>
/// Represents a Philips Hue light bulb or fixture
/// </summary>
public class HueLight
{
    /// <summary>
    /// Gets or sets the unique identifier of the light
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the friendly name of the light
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the light is on
    /// </summary>
    public bool IsOn { get; set; }

    /// <summary>
    /// Gets or sets the brightness level (0-254)
    /// </summary>
    public byte Brightness { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the light supports color
    /// </summary>
    public bool SupportsColor { get; set; }

    /// <summary>
    /// Gets or sets the type/archetype of the light
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the light is reachable
    /// </summary>
    public bool IsReachable { get; set; }
}
