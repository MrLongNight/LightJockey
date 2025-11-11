namespace LightJockey.Models;

/// <summary>
/// Represents a Philips Hue Entertainment Area configured for synchronized lighting
/// </summary>
public class EntertainmentArea
{
    /// <summary>
    /// Unique identifier for the entertainment area (GUID)
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Friendly name of the entertainment area
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Whether the entertainment area is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// List of light IDs included in this entertainment area
    /// </summary>
    public IReadOnlyList<Guid> LightIds { get; init; } = new List<Guid>();

    /// <summary>
    /// Number of channels (lights) in the entertainment area
    /// </summary>
    public int ChannelCount => LightIds.Count;
}
