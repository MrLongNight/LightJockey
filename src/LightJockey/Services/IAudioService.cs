using LightJockey.Models;

namespace LightJockey.Services;

/// <summary>
/// Service for managing audio devices and capturing audio streams
/// </summary>
public interface IAudioService : IDisposable
{
    /// <summary>
    /// Event raised when new audio data is available for processing (FFT-compatible)
    /// </summary>
    event EventHandler<AudioDataEventArgs>? AudioDataAvailable;

    /// <summary>
    /// Gets all available audio input devices
    /// </summary>
    /// <returns>Collection of available input devices</returns>
    IReadOnlyList<AudioDevice> GetInputDevices();

    /// <summary>
    /// Gets all available audio output devices (for loopback capture)
    /// </summary>
    /// <returns>Collection of available output devices</returns>
    IReadOnlyList<AudioDevice> GetOutputDevices();

    /// <summary>
    /// Gets the currently selected audio device
    /// </summary>
    AudioDevice? SelectedDevice { get; }

    /// <summary>
    /// Selects an audio device for capture
    /// </summary>
    /// <param name="device">The device to select</param>
    void SelectDevice(AudioDevice device);

    /// <summary>
    /// Starts audio capture from the selected device
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no device is selected</exception>
    void StartCapture();

    /// <summary>
    /// Stops audio capture
    /// </summary>
    void StopCapture();

    /// <summary>
    /// Gets a value indicating whether audio capture is currently active
    /// </summary>
    bool IsCapturing { get; }
}
