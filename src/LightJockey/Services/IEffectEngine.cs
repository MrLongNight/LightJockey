using LightJockey.Models;

namespace LightJockey.Services;

/// <summary>
/// Service for managing effect plugins and coordinating with audio services
/// </summary>
public interface IEffectEngine : IDisposable
{
    /// <summary>
    /// Event raised when the active effect changes
    /// </summary>
    event EventHandler<string?>? ActiveEffectChanged;

    /// <summary>
    /// Event raised when an effect error occurs
    /// </summary>
    event EventHandler<string>? EffectError;

    /// <summary>
    /// Registers an effect plugin
    /// </summary>
    /// <param name="plugin">The plugin to register</param>
    void RegisterPlugin(IEffectPlugin plugin);

    /// <summary>
    /// Unregisters an effect plugin
    /// </summary>
    /// <param name="pluginName">Name of the plugin to unregister</param>
    void UnregisterPlugin(string pluginName);

    /// <summary>
    /// Gets all registered effect plugins
    /// </summary>
    /// <returns>Collection of registered plugin names</returns>
    IReadOnlyList<string> GetAvailableEffects();

    /// <summary>
    /// Gets a specific effect plugin by name
    /// </summary>
    /// <param name="pluginName">Name of the plugin</param>
    /// <returns>The plugin or null if not found</returns>
    IEffectPlugin? GetPlugin(string pluginName);

    /// <summary>
    /// Sets the active effect plugin
    /// </summary>
    /// <param name="pluginName">Name of the plugin to activate</param>
    /// <param name="config">Configuration for the effect</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the effect was activated successfully</returns>
    Task<bool> SetActiveEffectAsync(string pluginName, EffectConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the currently active effect
    /// </summary>
    Task StopActiveEffectAsync();

    /// <summary>
    /// Updates the configuration of the active effect
    /// </summary>
    /// <param name="config">New configuration</param>
    void UpdateActiveEffectConfig(EffectConfig config);

    /// <summary>
    /// Gets the name of the currently active effect
    /// </summary>
    string? ActiveEffectName { get; }

    /// <summary>
    /// Gets the currently active effect plugin
    /// </summary>
    IEffectPlugin? ActiveEffect { get; }

    /// <summary>
    /// Gets a value indicating whether an effect is currently running
    /// </summary>
    bool IsEffectRunning { get; }
}
