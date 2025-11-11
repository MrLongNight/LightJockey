using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services;

/// <summary>
/// Service for managing effect plugins and coordinating with audio services
/// </summary>
public class EffectEngine : IEffectEngine
{
    private readonly ILogger<EffectEngine> _logger;
    private readonly IAudioService _audioService;
    private readonly ISpectralAnalyzer _spectralAnalyzer;
    private readonly IBeatDetector _beatDetector;
    private readonly Dictionary<string, IEffectPlugin> _plugins = new();
    private IEffectPlugin? _activeEffect;
    private bool _disposed;

    /// <summary>
    /// Event raised when the active effect changes
    /// </summary>
    public event EventHandler<string?>? ActiveEffectChanged;

    /// <summary>
    /// Event raised when an effect error occurs
    /// </summary>
    public event EventHandler<string>? EffectError;

    /// <summary>
    /// Initializes a new instance of the EffectEngine class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="audioService">Audio service for audio events</param>
    /// <param name="spectralAnalyzer">Spectral analyzer for frequency analysis</param>
    /// <param name="beatDetector">Beat detector for beat events</param>
    public EffectEngine(
        ILogger<EffectEngine> logger,
        IAudioService audioService,
        ISpectralAnalyzer spectralAnalyzer,
        IBeatDetector beatDetector)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _spectralAnalyzer = spectralAnalyzer ?? throw new ArgumentNullException(nameof(spectralAnalyzer));
        _beatDetector = beatDetector ?? throw new ArgumentNullException(nameof(beatDetector));

        // Subscribe to audio analysis events
        _spectralAnalyzer.SpectralDataAvailable += OnSpectralDataAvailable;
        _beatDetector.BeatDetected += OnBeatDetected;

        _logger.LogDebug("EffectEngine initialized");
    }

    /// <summary>
    /// Registers an effect plugin
    /// </summary>
    /// <param name="plugin">The plugin to register</param>
    public void RegisterPlugin(IEffectPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        if (_plugins.ContainsKey(plugin.Name))
        {
            _logger.LogWarning("Plugin '{PluginName}' is already registered", plugin.Name);
            return;
        }

        _plugins[plugin.Name] = plugin;
        _logger.LogInformation("Registered effect plugin: {PluginName}", plugin.Name);
    }

    /// <summary>
    /// Unregisters an effect plugin
    /// </summary>
    /// <param name="pluginName">Name of the plugin to unregister</param>
    public void UnregisterPlugin(string pluginName)
    {
        if (string.IsNullOrEmpty(pluginName))
        {
            throw new ArgumentException("Plugin name cannot be null or empty", nameof(pluginName));
        }

        if (_activeEffect?.Name == pluginName)
        {
            _logger.LogWarning("Cannot unregister active plugin '{PluginName}'", pluginName);
            return;
        }

        if (_plugins.Remove(pluginName))
        {
            _logger.LogInformation("Unregistered effect plugin: {PluginName}", pluginName);
        }
    }

    /// <summary>
    /// Gets all registered effect plugins
    /// </summary>
    /// <returns>Collection of registered plugin names</returns>
    public IReadOnlyList<string> GetAvailableEffects()
    {
        return _plugins.Keys.ToList();
    }

    /// <summary>
    /// Gets a specific effect plugin by name
    /// </summary>
    /// <param name="pluginName">Name of the plugin</param>
    /// <returns>The plugin or null if not found</returns>
    public IEffectPlugin? GetPlugin(string pluginName)
    {
        if (string.IsNullOrEmpty(pluginName))
        {
            return null;
        }

        _plugins.TryGetValue(pluginName, out var plugin);
        return plugin;
    }

    /// <summary>
    /// Sets the active effect plugin
    /// </summary>
    /// <param name="pluginName">Name of the plugin to activate</param>
    /// <param name="config">Configuration for the effect</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the effect was activated successfully</returns>
    public async Task<bool> SetActiveEffectAsync(string pluginName, EffectConfig config, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(pluginName))
        {
            throw new ArgumentException("Plugin name cannot be null or empty", nameof(pluginName));
        }

        ArgumentNullException.ThrowIfNull(config);

        // Stop current active effect if any
        if (_activeEffect != null)
        {
            await StopActiveEffectAsync();
        }

        // Get the requested plugin
        if (!_plugins.TryGetValue(pluginName, out var plugin))
        {
            _logger.LogError("Plugin '{PluginName}' not found", pluginName);
            return false;
        }

        try
        {
            _logger.LogInformation("Initializing effect: {PluginName}", pluginName);
            
            // Initialize the plugin
            var initialized = await plugin.InitializeAsync(config);
            if (!initialized)
            {
                _logger.LogError("Failed to initialize plugin '{PluginName}'", pluginName);
                return false;
            }

            // Start the plugin
            await plugin.StartAsync(cancellationToken);
            
            _activeEffect = plugin;
            _logger.LogInformation("Active effect set to: {PluginName}", pluginName);
            
            ActiveEffectChanged?.Invoke(this, pluginName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating effect '{PluginName}'", pluginName);
            EffectError?.Invoke(this, $"Failed to activate effect '{pluginName}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Stops the currently active effect
    /// </summary>
    public async Task StopActiveEffectAsync()
    {
        if (_activeEffect == null)
        {
            return;
        }

        var effectName = _activeEffect.Name;
        _logger.LogInformation("Stopping active effect: {EffectName}", effectName);

        try
        {
            await _activeEffect.StopAsync();
            _activeEffect = null;
            ActiveEffectChanged?.Invoke(this, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping effect '{EffectName}'", effectName);
            EffectError?.Invoke(this, $"Failed to stop effect '{effectName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the configuration of the active effect
    /// </summary>
    /// <param name="config">New configuration</param>
    public void UpdateActiveEffectConfig(EffectConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (_activeEffect == null)
        {
            _logger.LogWarning("No active effect to update");
            return;
        }

        try
        {
            _activeEffect.UpdateConfig(config);
            _logger.LogDebug("Updated configuration for active effect: {EffectName}", _activeEffect.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating effect configuration");
            EffectError?.Invoke(this, $"Failed to update effect configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the name of the currently active effect
    /// </summary>
    public string? ActiveEffectName => _activeEffect?.Name;

    /// <summary>
    /// Gets the currently active effect plugin
    /// </summary>
    public IEffectPlugin? ActiveEffect => _activeEffect;

    /// <summary>
    /// Gets a value indicating whether an effect is currently running
    /// </summary>
    public bool IsEffectRunning => _activeEffect?.State == EffectState.Running;

    private void OnSpectralDataAvailable(object? sender, SpectralDataEventArgs e)
    {
        if (_activeEffect?.State == EffectState.Running)
        {
            try
            {
                _activeEffect.OnSpectralData(e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in effect spectral data handler");
            }
        }
    }

    private void OnBeatDetected(object? sender, BeatDetectedEventArgs e)
    {
        if (_activeEffect?.State == EffectState.Running)
        {
            try
            {
                _activeEffect.OnBeatDetected(e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in effect beat detection handler");
            }
        }
    }

    /// <summary>
    /// Disposes the EffectEngine and all registered plugins
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _logger.LogDebug("Disposing EffectEngine");

        // Unsubscribe from events
        _spectralAnalyzer.SpectralDataAvailable -= OnSpectralDataAvailable;
        _beatDetector.BeatDetected -= OnBeatDetected;

        // Stop and dispose active effect
        if (_activeEffect != null)
        {
            try
            {
                _activeEffect.StopAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping active effect during disposal");
            }
        }

        // Dispose all plugins
        foreach (var plugin in _plugins.Values)
        {
            try
            {
                plugin.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing plugin '{PluginName}'", plugin.Name);
            }
        }

        _plugins.Clear();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
