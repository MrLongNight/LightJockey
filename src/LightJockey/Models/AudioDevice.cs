namespace LightJockey.Models;

/// <summary>
/// Represents an audio device available for input or output
/// </summary>
public class AudioDevice
{
    /// <summary>
    /// Gets or sets the unique device identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the friendly name of the device
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the device type (Input or Output)
    /// </summary>
    public AudioDeviceType DeviceType { get; set; }

    /// <summary>
    /// Gets or sets whether this device is the default device
    /// </summary>
    public bool IsDefault { get; set; }

    public override string ToString() => Name;
}

/// <summary>
/// Represents the type of audio device
/// </summary>
public enum AudioDeviceType
{
    /// <summary>
    /// Audio input device (microphone, line-in, etc.)
    /// </summary>
    Input,

    /// <summary>
    /// Audio output device (speakers, headphones, etc.)
    /// </summary>
    Output
}
