namespace LightJockey.Models;

/// <summary>
/// Represents a light channel in LightJockey Entertainment streaming
/// </summary>
public class LightChannel
{
    /// <summary>
    /// Channel index (0-based)
    /// </summary>
    public required byte Index { get; init; }

    /// <summary>
    /// Light ID associated with this channel
    /// </summary>
    public required Guid LightId { get; init; }

    /// <summary>
    /// Current RGB color for this channel
    /// </summary>
    public HueColor Color { get; set; } = new HueColor(0, 0, 0);

    /// <summary>
    /// Current brightness for this channel (0.0 - 1.0)
    /// </summary>
    public double Brightness { get; set; } = 1.0;
}
