using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services;

/// <summary>
/// Service for managing presets and application configuration persistence
/// </summary>
public class PresetService : IPresetService
{
    private readonly ILogger<PresetService> _logger;
    private readonly IEffectEngine _effectEngine;
    private readonly string _presetsDirectory;
    private readonly string _autoSaveFileName = "autosave.json";
    private PresetCollection _presetCollection;
    private bool _autoSaveEnabled = true;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Event raised when a preset is saved
    /// </summary>
    public event EventHandler<Preset>? PresetSaved;

    /// <summary>
    /// Event raised when a preset is loaded
    /// </summary>
    public event EventHandler<Preset>? PresetLoaded;

    /// <summary>
    /// Event raised when a preset is deleted
    /// </summary>
    public event EventHandler<string>? PresetDeleted;

    /// <summary>
    /// Event raised when auto-save occurs
    /// </summary>
    public event EventHandler? AutoSaveCompleted;

    /// <summary>
    /// Gets a value indicating whether auto-save is enabled
    /// </summary>
    public bool IsAutoSaveEnabled => _autoSaveEnabled;

    /// <summary>
    /// Initializes a new instance of the PresetService class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="effectEngine">Effect engine for integration</param>
    public PresetService(ILogger<PresetService> logger, IEffectEngine effectEngine)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _effectEngine = effectEngine ?? throw new ArgumentNullException(nameof(effectEngine));

        // Set up presets directory in AppData
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _presetsDirectory = Path.Combine(appDataPath, "LightJockey", "Presets");
        Directory.CreateDirectory(_presetsDirectory);

        _logger.LogDebug("Presets directory: {PresetsDirectory}", _presetsDirectory);

        // Load existing presets
        _presetCollection = LoadPresetCollectionFromDisk();

        // Subscribe to effect changes for auto-save
        _effectEngine.ActiveEffectChanged += OnActiveEffectChanged;

        _logger.LogInformation("PresetService initialized with {PresetCount} presets", _presetCollection.Presets.Count);
    }

    /// <summary>
    /// Gets all available presets
    /// </summary>
    /// <returns>Collection of all presets</returns>
    public IReadOnlyList<Preset> GetAllPresets()
    {
        return _presetCollection.Presets.AsReadOnly();
    }

    /// <summary>
    /// Gets a preset by ID
    /// </summary>
    /// <param name="presetId">ID of the preset</param>
    /// <returns>The preset or null if not found</returns>
    public Preset? GetPreset(string presetId)
    {
        if (string.IsNullOrEmpty(presetId))
        {
            return null;
        }

        return _presetCollection.Presets.FirstOrDefault(p => p.Id == presetId);
    }

    /// <summary>
    /// Gets the currently active preset
    /// </summary>
    /// <returns>The active preset or null if none is active</returns>
    public Preset? GetActivePreset()
    {
        if (string.IsNullOrEmpty(_presetCollection.ActivePresetId))
        {
            return null;
        }

        return GetPreset(_presetCollection.ActivePresetId);
    }

    /// <summary>
    /// Saves a preset
    /// </summary>
    /// <param name="preset">The preset to save</param>
    /// <returns>The saved preset with updated metadata</returns>
    public async Task<Preset> SavePresetAsync(Preset preset)
    {
        ArgumentNullException.ThrowIfNull(preset);

        try
        {
            // Update modified timestamp
            var updatedPreset = preset with { ModifiedAt = DateTime.UtcNow };

            // Find existing preset or add new one
            var existingIndex = _presetCollection.Presets.FindIndex(p => p.Id == updatedPreset.Id);
            if (existingIndex >= 0)
            {
                _presetCollection.Presets[existingIndex] = updatedPreset;
                _logger.LogInformation("Updated preset: {PresetName} ({PresetId})", updatedPreset.Name, updatedPreset.Id);
            }
            else
            {
                _presetCollection.Presets.Add(updatedPreset);
                _logger.LogInformation("Added new preset: {PresetName} ({PresetId})", updatedPreset.Name, updatedPreset.Id);
            }

            // Persist to disk
            await SavePresetCollectionToDiskAsync();

            PresetSaved?.Invoke(this, updatedPreset);
            return updatedPreset;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving preset: {PresetName}", preset.Name);
            throw;
        }
    }

    /// <summary>
    /// Creates a new preset from current application state
    /// </summary>
    /// <param name="name">Name for the preset</param>
    /// <param name="description">Optional description</param>
    /// <returns>The created preset</returns>
    public async Task<Preset> CreatePresetFromCurrentStateAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Preset name cannot be null or empty", nameof(name));
        }

        try
        {
            var preset = new Preset
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                ActiveEffectName = _effectEngine.ActiveEffectName,
                EffectConfig = CaptureCurrentEffectConfig(),
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Creating preset from current state: {PresetName}", name);
            return await SavePresetAsync(preset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating preset from current state: {PresetName}", name);
            throw;
        }
    }

    /// <summary>
    /// Loads a preset and applies it to the application
    /// </summary>
    /// <param name="presetId">ID of the preset to load</param>
    /// <returns>True if the preset was loaded successfully</returns>
    public async Task<bool> LoadPresetAsync(string presetId)
    {
        if (string.IsNullOrEmpty(presetId))
        {
            _logger.LogWarning("Cannot load preset: preset ID is null or empty");
            return false;
        }

        var preset = GetPreset(presetId);
        if (preset == null)
        {
            _logger.LogWarning("Preset not found: {PresetId}", presetId);
            return false;
        }

        try
        {
            _logger.LogInformation("Loading preset: {PresetName} ({PresetId})", preset.Name, preset.Id);

            // Apply effect if specified
            if (!string.IsNullOrEmpty(preset.ActiveEffectName) && preset.EffectConfig != null)
            {
                var success = await _effectEngine.SetActiveEffectAsync(
                    preset.ActiveEffectName,
                    preset.EffectConfig,
                    CancellationToken.None);

                if (!success)
                {
                    _logger.LogWarning("Failed to set active effect from preset: {EffectName}", preset.ActiveEffectName);
                    return false;
                }
            }

            // Update active preset ID
            _presetCollection = _presetCollection with { ActivePresetId = presetId };
            await SavePresetCollectionToDiskAsync();

            PresetLoaded?.Invoke(this, preset);
            _logger.LogInformation("Successfully loaded preset: {PresetName}", preset.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading preset: {PresetId}", presetId);
            return false;
        }
    }

    /// <summary>
    /// Deletes a preset
    /// </summary>
    /// <param name="presetId">ID of the preset to delete</param>
    /// <returns>True if the preset was deleted successfully</returns>
    public async Task<bool> DeletePresetAsync(string presetId)
    {
        if (string.IsNullOrEmpty(presetId))
        {
            return false;
        }

        var preset = GetPreset(presetId);
        if (preset == null)
        {
            return false;
        }

        try
        {
            _presetCollection.Presets.Remove(preset);

            // Clear active preset if it was the deleted one
            if (_presetCollection.ActivePresetId == presetId)
            {
                _presetCollection = _presetCollection with { ActivePresetId = null };
            }

            await SavePresetCollectionToDiskAsync();

            PresetDeleted?.Invoke(this, presetId);
            _logger.LogInformation("Deleted preset: {PresetName} ({PresetId})", preset.Name, presetId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting preset: {PresetId}", presetId);
            return false;
        }
    }

    /// <summary>
    /// Exports a preset to a JSON file
    /// </summary>
    /// <param name="presetId">ID of the preset to export</param>
    /// <param name="filePath">Path where to save the JSON file</param>
    public async Task ExportPresetAsync(string presetId, string filePath)
    {
        var preset = GetPreset(presetId);
        if (preset == null)
        {
            throw new InvalidOperationException($"Preset not found: {presetId}");
        }

        try
        {
            var json = JsonSerializer.Serialize(preset, JsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            _logger.LogInformation("Exported preset to {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting preset to {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Exports all presets to a JSON file
    /// </summary>
    /// <param name="filePath">Path where to save the JSON file</param>
    public async Task ExportAllPresetsAsync(string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(_presetCollection, JsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            _logger.LogInformation("Exported all presets to {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting all presets to {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Imports a preset from a JSON file
    /// </summary>
    /// <param name="filePath">Path to the JSON file</param>
    /// <returns>The imported preset</returns>
    public async Task<Preset> ImportPresetAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var preset = JsonSerializer.Deserialize<Preset>(json, JsonOptions);

            if (preset == null)
            {
                throw new InvalidOperationException("Failed to deserialize preset from JSON");
            }

            // Generate new ID to avoid conflicts
            var importedPreset = preset with
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            await SavePresetAsync(importedPreset);
            _logger.LogInformation("Imported preset from {FilePath}: {PresetName}", filePath, importedPreset.Name);
            return importedPreset;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing preset from {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Imports multiple presets from a JSON file
    /// </summary>
    /// <param name="filePath">Path to the JSON file</param>
    /// <returns>Collection of imported presets</returns>
    public async Task<IReadOnlyList<Preset>> ImportPresetsAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var collection = JsonSerializer.Deserialize<PresetCollection>(json, JsonOptions);

            if (collection == null || collection.Presets.Count == 0)
            {
                throw new InvalidOperationException("No presets found in the file");
            }

            var importedPresets = new List<Preset>();
            foreach (var preset in collection.Presets)
            {
                var importedPreset = preset with
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                await SavePresetAsync(importedPreset);
                importedPresets.Add(importedPreset);
            }

            _logger.LogInformation("Imported {Count} presets from {FilePath}", importedPresets.Count, filePath);
            return importedPresets.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing presets from {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Enables or disables automatic saving of configuration changes
    /// </summary>
    /// <param name="enabled">True to enable auto-save, false to disable</param>
    public void SetAutoSave(bool enabled)
    {
        _autoSaveEnabled = enabled;
        _logger.LogInformation("Auto-save {Status}", enabled ? "enabled" : "disabled");
    }

    /// <summary>
    /// Manually triggers an auto-save of the current state
    /// </summary>
    public async Task TriggerAutoSaveAsync()
    {
        if (!_autoSaveEnabled)
        {
            _logger.LogDebug("Auto-save is disabled, skipping");
            return;
        }

        try
        {
            var autoSavePreset = new Preset
            {
                Id = "autosave",
                Name = "Auto-Save",
                Description = "Automatically saved configuration",
                ActiveEffectName = _effectEngine.ActiveEffectName,
                EffectConfig = CaptureCurrentEffectConfig(),
                ModifiedAt = DateTime.UtcNow
            };

            var autoSavePath = Path.Combine(_presetsDirectory, _autoSaveFileName);
            var json = JsonSerializer.Serialize(autoSavePreset, JsonOptions);
            await File.WriteAllTextAsync(autoSavePath, json);

            AutoSaveCompleted?.Invoke(this, EventArgs.Empty);
            _logger.LogDebug("Auto-save completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auto-save");
        }
    }

    private EffectConfig? CaptureCurrentEffectConfig()
    {
        // For now, return a default config
        // In a real implementation, this would capture the current effect's configuration
        if (_effectEngine.ActiveEffect != null)
        {
            return new EffectConfig
            {
                Intensity = 0.8,
                Speed = 1.0,
                Brightness = 0.8,
                AudioReactive = true,
                AudioSensitivity = 0.5,
                SmoothTransitions = true,
                TransitionDurationMs = 100
            };
        }

        return null;
    }

    private PresetCollection LoadPresetCollectionFromDisk()
    {
        var collectionPath = Path.Combine(_presetsDirectory, "presets.json");

        if (!File.Exists(collectionPath))
        {
            _logger.LogDebug("No existing preset collection found, creating new one");
            return new PresetCollection();
        }

        try
        {
            var json = File.ReadAllText(collectionPath);
            var collection = JsonSerializer.Deserialize<PresetCollection>(json, JsonOptions);
            _logger.LogInformation("Loaded {Count} presets from disk", collection?.Presets.Count ?? 0);
            return collection ?? new PresetCollection();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading preset collection from disk, creating new one");
            return new PresetCollection();
        }
    }

    private async Task SavePresetCollectionToDiskAsync()
    {
        var collectionPath = Path.Combine(_presetsDirectory, "presets.json");

        try
        {
            var updatedCollection = _presetCollection with { LastSavedAt = DateTime.UtcNow };
            var json = JsonSerializer.Serialize(updatedCollection, JsonOptions);
            await File.WriteAllTextAsync(collectionPath, json);
            _presetCollection = updatedCollection;
            _logger.LogDebug("Saved preset collection to disk");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving preset collection to disk");
            throw;
        }
    }

    private async void OnActiveEffectChanged(object? sender, string? effectName)
    {
        if (_autoSaveEnabled)
        {
            _logger.LogDebug("Active effect changed to {EffectName}, triggering auto-save", effectName ?? "none");
            await TriggerAutoSaveAsync();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _effectEngine.ActiveEffectChanged -= OnActiveEffectChanged;
        _disposed = true;
        _logger.LogDebug("PresetService disposed");
    }
}
