namespace LightJockey.Models;

/// <summary>
/// Represents a named preset containing complete application configuration
/// </summary>
public record Preset
{
    /// <summary>
    /// Unique identifier for the preset
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name for the preset
    /// </summary>
    public string Name { get; init; } = "Untitled Preset";

    /// <summary>
    /// Optional description of the preset
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Name of the active effect
    /// </summary>
    public string? ActiveEffectName { get; init; }

    /// <summary>
    /// Effect configuration settings
    /// </summary>
    public EffectConfig? EffectConfig { get; init; }

    /// <summary>
    /// Selected audio device ID
    /// </summary>
    public string? AudioDeviceId { get; init; }

    /// <summary>
    /// Hue bridge IP address
    /// </summary>
    public string? HueBridgeIp { get; init; }

    /// <summary>
    /// Entertainment area ID (for Entertainment V2)
    /// </summary>
    public string? EntertainmentAreaId { get; init; }

    /// <summary>
    /// Timestamp when the preset was created
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the preset was last modified
    /// </summary>
    public DateTime ModifiedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Additional custom settings as key-value pairs
    /// </summary>
    public Dictionary<string, object>? CustomSettings { get; init; }
}
