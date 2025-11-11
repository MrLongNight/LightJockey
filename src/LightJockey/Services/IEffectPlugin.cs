using LightJockey.Models;

namespace LightJockey.Services;

/// <summary>
/// Interface for effect plugins in the LightJockey EffectEngine
/// </summary>
public interface IEffectPlugin : IDisposable
{
    /// <summary>
    /// Gets the unique name of this effect plugin
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a description of what this effect does
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the current state of the effect
    /// </summary>
    EffectState State { get; }

    /// <summary>
    /// Event raised when the effect state changes
    /// </summary>
    event EventHandler<EffectState>? StateChanged;

    /// <summary>
    /// Initializes the effect with the given configuration
    /// </summary>
    /// <param name="config">Effect configuration</param>
    /// <returns>True if initialization was successful</returns>
    Task<bool> InitializeAsync(EffectConfig config);

    /// <summary>
    /// Starts the effect
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the effect
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Updates the effect configuration
    /// </summary>
    /// <param name="config">New configuration</param>
    void UpdateConfig(EffectConfig config);

    /// <summary>
    /// Handles spectral data from audio analysis
    /// </summary>
    /// <param name="spectralData">Spectral data event arguments</param>
    void OnSpectralData(SpectralDataEventArgs spectralData);

    /// <summary>
    /// Handles beat detection events
    /// </summary>
    /// <param name="beatData">Beat detected event arguments</param>
    void OnBeatDetected(BeatDetectedEventArgs beatData);
}
