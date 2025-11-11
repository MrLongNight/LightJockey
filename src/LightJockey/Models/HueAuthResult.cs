namespace LightJockey.Models;

/// <summary>
/// Represents the result of a Hue Bridge authentication/registration attempt
/// </summary>
public class HueAuthResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the authentication was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the application key (username) received from the bridge
    /// </summary>
    public string? AppKey { get; set; }

    /// <summary>
    /// Gets or sets the error message if authentication failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the link button needs to be pressed
    /// </summary>
    public bool RequiresLinkButton { get; set; }
}
