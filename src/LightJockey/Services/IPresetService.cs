using LightJockey.Models;

namespace LightJockey.Services;

/// <summary>
/// Service for managing presets and application configuration persistence
/// </summary>
public interface IPresetService : IDisposable
{
    /// <summary>
    /// Event raised when a preset is saved
    /// </summary>
    event EventHandler<Preset>? PresetSaved;

    /// <summary>
    /// Event raised when a preset is loaded
    /// </summary>
    event EventHandler<Preset>? PresetLoaded;

    /// <summary>
    /// Event raised when a preset is deleted
    /// </summary>
    event EventHandler<string>? PresetDeleted;

    /// <summary>
    /// Event raised when auto-save occurs
    /// </summary>
    event EventHandler? AutoSaveCompleted;

    /// <summary>
    /// Gets all available presets
    /// </summary>
    /// <returns>Collection of all presets</returns>
    IReadOnlyList<Preset> GetAllPresets();

    /// <summary>
    /// Gets a preset by ID
    /// </summary>
    /// <param name="presetId">ID of the preset</param>
    /// <returns>The preset or null if not found</returns>
    Preset? GetPreset(string presetId);

    /// <summary>
    /// Gets the currently active preset
    /// </summary>
    /// <returns>The active preset or null if none is active</returns>
    Preset? GetActivePreset();

    /// <summary>
    /// Saves a preset
    /// </summary>
    /// <param name="preset">The preset to save</param>
    /// <returns>The saved preset with updated metadata</returns>
    Task<Preset> SavePresetAsync(Preset preset);

    /// <summary>
    /// Creates a new preset from current application state
    /// </summary>
    /// <param name="name">Name for the preset</param>
    /// <param name="description">Optional description</param>
    /// <returns>The created preset</returns>
    Task<Preset> CreatePresetFromCurrentStateAsync(string name, string? description = null);

    /// <summary>
    /// Loads a preset and applies it to the application
    /// </summary>
    /// <param name="presetId">ID of the preset to load</param>
    /// <returns>True if the preset was loaded successfully</returns>
    Task<bool> LoadPresetAsync(string presetId);

    /// <summary>
    /// Deletes a preset
    /// </summary>
    /// <param name="presetId">ID of the preset to delete</param>
    /// <returns>True if the preset was deleted successfully</returns>
    Task<bool> DeletePresetAsync(string presetId);

    /// <summary>
    /// Exports a preset to a JSON file
    /// </summary>
    /// <param name="presetId">ID of the preset to export</param>
    /// <param name="filePath">Path where to save the JSON file</param>
    Task ExportPresetAsync(string presetId, string filePath);

    /// <summary>
    /// Exports all presets to a JSON file
    /// </summary>
    /// <param name="filePath">Path where to save the JSON file</param>
    Task ExportAllPresetsAsync(string filePath);

    /// <summary>
    /// Imports a preset from a JSON file
    /// </summary>
    /// <param name="filePath">Path to the JSON file</param>
    /// <returns>The imported preset</returns>
    Task<Preset> ImportPresetAsync(string filePath);

    /// <summary>
    /// Imports multiple presets from a JSON file
    /// </summary>
    /// <param name="filePath">Path to the JSON file</param>
    /// <returns>Collection of imported presets</returns>
    Task<IReadOnlyList<Preset>> ImportPresetsAsync(string filePath);

    /// <summary>
    /// Enables or disables automatic saving of configuration changes
    /// </summary>
    /// <param name="enabled">True to enable auto-save, false to disable</param>
    void SetAutoSave(bool enabled);

    /// <summary>
    /// Gets a value indicating whether auto-save is enabled
    /// </summary>
    bool IsAutoSaveEnabled { get; }

    /// <summary>
    /// Manually triggers an auto-save of the current state
    /// </summary>
    Task TriggerAutoSaveAsync();
}
