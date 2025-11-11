namespace LightJockey.Models;

/// <summary>
/// Collection of presets with metadata
/// </summary>
public record PresetCollection
{
    /// <summary>
    /// Version of the preset collection format
    /// </summary>
    public string Version { get; init; } = "1.0";

    /// <summary>
    /// ID of the currently active preset
    /// </summary>
    public string? ActivePresetId { get; init; }

    /// <summary>
    /// List of all presets
    /// </summary>
    public List<Preset> Presets { get; init; } = new();

    /// <summary>
    /// Timestamp when the collection was last saved
    /// </summary>
    public DateTime LastSavedAt { get; init; } = DateTime.UtcNow;
}
