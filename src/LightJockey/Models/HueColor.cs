namespace LightJockey.Models;

/// <summary>
/// Represents a color for Hue lights in RGB format
/// </summary>
public class HueColor
{
    /// <summary>
    /// Gets or sets the red component (0-255)
    /// </summary>
    public byte Red { get; set; }

    /// <summary>
    /// Gets or sets the green component (0-255)
    /// </summary>
    public byte Green { get; set; }

    /// <summary>
    /// Gets or sets the blue component (0-255)
    /// </summary>
    public byte Blue { get; set; }

    /// <summary>
    /// Initializes a new instance of the HueColor class
    /// </summary>
    public HueColor()
    {
    }

    /// <summary>
    /// Initializes a new instance of the HueColor class with specified RGB values
    /// </summary>
    /// <param name="red">Red component (0-255)</param>
    /// <param name="green">Green component (0-255)</param>
    /// <param name="blue">Blue component (0-255)</param>
    public HueColor(byte red, byte green, byte blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    /// <summary>
    /// Converts the color to a hex string (e.g., "FF0000" for red)
    /// </summary>
    public string ToHexString()
    {
        return $"{Red:X2}{Green:X2}{Blue:X2}";
    }

    /// <summary>
    /// Creates a HueColor from a hex string (e.g., "FF0000" or "#FF0000")
    /// </summary>
    public static HueColor FromHexString(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length != 6)
            throw new ArgumentException("Hex string must be 6 characters long (without #)", nameof(hex));

        return new HueColor(
            Convert.ToByte(hex.Substring(0, 2), 16),
            Convert.ToByte(hex.Substring(2, 2), 16),
            Convert.ToByte(hex.Substring(4, 2), 16)
        );
    }
}
