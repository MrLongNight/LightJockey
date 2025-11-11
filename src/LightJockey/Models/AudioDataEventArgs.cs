namespace LightJockey.Models;

/// <summary>
/// Event arguments for audio data events, containing raw PCM samples for FFT processing
/// </summary>
public class AudioDataEventArgs : EventArgs
{
    /// <summary>
    /// Gets the raw audio samples (PCM data)
    /// </summary>
    public float[] Samples { get; }

    /// <summary>
    /// Gets the sample rate in Hz
    /// </summary>
    public int SampleRate { get; }

    /// <summary>
    /// Gets the number of channels (1 for mono, 2 for stereo)
    /// </summary>
    public int Channels { get; }

    /// <summary>
    /// Gets the timestamp when the audio data was captured
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Initializes a new instance of the AudioDataEventArgs class
    /// </summary>
    /// <param name="samples">Raw audio samples (PCM data)</param>
    /// <param name="sampleRate">Sample rate in Hz</param>
    /// <param name="channels">Number of channels</param>
    public AudioDataEventArgs(float[] samples, int sampleRate, int channels)
    {
        Samples = samples ?? throw new ArgumentNullException(nameof(samples));
        SampleRate = sampleRate;
        Channels = channels;
        Timestamp = DateTime.UtcNow;
    }
}
