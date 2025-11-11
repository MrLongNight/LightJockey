using System.IO;
using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services;

/// <summary>
/// Unit tests for PresetService
/// </summary>
public class PresetServiceTests : IDisposable
{
    private readonly Mock<ILogger<PresetService>> _mockLogger;
    private readonly Mock<IEffectEngine> _mockEffectEngine;
    private readonly PresetService _service;
    private readonly string _testPresetsDirectory;

    public PresetServiceTests()
    {
        _mockLogger = new Mock<ILogger<PresetService>>();
        _mockEffectEngine = new Mock<IEffectEngine>();

        // Create a temporary directory for test presets
        _testPresetsDirectory = Path.Combine(Path.GetTempPath(), $"LightJockeyTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testPresetsDirectory);

        // Override the presets directory using reflection (since it's set in constructor)
        _service = new PresetService(_mockLogger.Object, _mockEffectEngine.Object);

        // Replace the presets directory with our test directory
        var presetsDirectoryField = typeof(PresetService).GetField("_presetsDirectory", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        presetsDirectoryField?.SetValue(_service, _testPresetsDirectory);
    }

    public void Dispose()
    {
        _service.Dispose();
        
        // Clean up test directory
        if (Directory.Exists(_testPresetsDirectory))
        {
            Directory.Delete(_testPresetsDirectory, true);
        }
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new PresetService(null!, _mockEffectEngine.Object));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullEffectEngine_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new PresetService(_mockLogger.Object, null!));
        Assert.Equal("effectEngine", exception.ParamName);
    }

    [Fact]
    public void GetAllPresets_Initially_ReturnsEmptyList()
    {
        // Act
        var presets = _service.GetAllPresets();

        // Assert
        Assert.NotNull(presets);
        Assert.Empty(presets);
    }

    [Fact]
    public async Task SavePresetAsync_WithValidPreset_SavesSuccessfully()
    {
        // Arrange
        var preset = new Preset
        {
            Name = "Test Preset",
            Description = "A test preset",
            ActiveEffectName = "TestEffect"
        };

        // Act
        var savedPreset = await _service.SavePresetAsync(preset);

        // Assert
        Assert.NotNull(savedPreset);
        Assert.Equal(preset.Name, savedPreset.Name);
        Assert.Equal(preset.Description, savedPreset.Description);
        Assert.Equal(preset.ActiveEffectName, savedPreset.ActiveEffectName);
        Assert.True(savedPreset.ModifiedAt >= preset.ModifiedAt);
    }

    [Fact]
    public async Task SavePresetAsync_WithNullPreset_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.SavePresetAsync(null!));
    }

    [Fact]
    public async Task SavePresetAsync_RaisesPresetSavedEvent()
    {
        // Arrange
        var preset = new Preset { Name = "Test" };
        Preset? eventPreset = null;
        _service.PresetSaved += (sender, p) => eventPreset = p;

        // Act
        await _service.SavePresetAsync(preset);

        // Assert
        Assert.NotNull(eventPreset);
        Assert.Equal(preset.Name, eventPreset.Name);
    }

    [Fact]
    public async Task GetPreset_AfterSaving_ReturnsPreset()
    {
        // Arrange
        var preset = new Preset { Name = "Test Preset" };
        var savedPreset = await _service.SavePresetAsync(preset);

        // Act
        var retrievedPreset = _service.GetPreset(savedPreset.Id);

        // Assert
        Assert.NotNull(retrievedPreset);
        Assert.Equal(savedPreset.Id, retrievedPreset.Id);
        Assert.Equal(savedPreset.Name, retrievedPreset.Name);
    }

    [Fact]
    public void GetPreset_WithNullId_ReturnsNull()
    {
        // Act
        var preset = _service.GetPreset(null!);

        // Assert
        Assert.Null(preset);
    }

    [Fact]
    public void GetPreset_WithInvalidId_ReturnsNull()
    {
        // Act
        var preset = _service.GetPreset("invalid-id");

        // Assert
        Assert.Null(preset);
    }

    [Fact]
    public async Task CreatePresetFromCurrentStateAsync_WithValidName_CreatesPreset()
    {
        // Arrange
        _mockEffectEngine.Setup(e => e.ActiveEffectName).Returns("TestEffect");

        // Act
        var preset = await _service.CreatePresetFromCurrentStateAsync("Test Preset", "Test Description");

        // Assert
        Assert.NotNull(preset);
        Assert.Equal("Test Preset", preset.Name);
        Assert.Equal("Test Description", preset.Description);
        Assert.Equal("TestEffect", preset.ActiveEffectName);
    }

    [Fact]
    public async Task CreatePresetFromCurrentStateAsync_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CreatePresetFromCurrentStateAsync(null!));
    }

    [Fact]
    public async Task CreatePresetFromCurrentStateAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CreatePresetFromCurrentStateAsync(""));
    }

    [Fact]
    public async Task LoadPresetAsync_WithValidPreset_LoadsSuccessfully()
    {
        // Arrange
        var config = new EffectConfig { Intensity = 0.9 };
        var preset = new Preset
        {
            Name = "Test Preset",
            ActiveEffectName = "TestEffect",
            EffectConfig = config
        };
        var savedPreset = await _service.SavePresetAsync(preset);

        _mockEffectEngine
            .Setup(e => e.SetActiveEffectAsync("TestEffect", config, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.LoadPresetAsync(savedPreset.Id);

        // Assert
        Assert.True(result);
        _mockEffectEngine.Verify(e => e.SetActiveEffectAsync(
            "TestEffect", 
            It.IsAny<EffectConfig>(), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task LoadPresetAsync_RaisesPresetLoadedEvent()
    {
        // Arrange
        var preset = new Preset
        {
            Name = "Test",
            ActiveEffectName = "TestEffect",
            EffectConfig = new EffectConfig()
        };
        var savedPreset = await _service.SavePresetAsync(preset);

        _mockEffectEngine
            .Setup(e => e.SetActiveEffectAsync(It.IsAny<string>(), It.IsAny<EffectConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Preset? eventPreset = null;
        _service.PresetLoaded += (sender, p) => eventPreset = p;

        // Act
        await _service.LoadPresetAsync(savedPreset.Id);

        // Assert
        Assert.NotNull(eventPreset);
        Assert.Equal(savedPreset.Name, eventPreset.Name);
    }

    [Fact]
    public async Task LoadPresetAsync_WithNullId_ReturnsFalse()
    {
        // Act
        var result = await _service.LoadPresetAsync(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task LoadPresetAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _service.LoadPresetAsync("invalid-id");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeletePresetAsync_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var preset = new Preset { Name = "Test" };
        var savedPreset = await _service.SavePresetAsync(preset);

        // Act
        var result = await _service.DeletePresetAsync(savedPreset.Id);

        // Assert
        Assert.True(result);
        Assert.Null(_service.GetPreset(savedPreset.Id));
    }

    [Fact]
    public async Task DeletePresetAsync_RaisesPresetDeletedEvent()
    {
        // Arrange
        var preset = new Preset { Name = "Test" };
        var savedPreset = await _service.SavePresetAsync(preset);

        string? deletedId = null;
        _service.PresetDeleted += (sender, id) => deletedId = id;

        // Act
        await _service.DeletePresetAsync(savedPreset.Id);

        // Assert
        Assert.Equal(savedPreset.Id, deletedId);
    }

    [Fact]
    public async Task DeletePresetAsync_WithNullId_ReturnsFalse()
    {
        // Act
        var result = await _service.DeletePresetAsync(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExportPresetAsync_WithValidPreset_ExportsToFile()
    {
        // Arrange
        var preset = new Preset { Name = "Export Test" };
        var savedPreset = await _service.SavePresetAsync(preset);
        var exportPath = Path.Combine(_testPresetsDirectory, "export.json");

        // Act
        await _service.ExportPresetAsync(savedPreset.Id, exportPath);

        // Assert
        Assert.True(File.Exists(exportPath));
        var json = await File.ReadAllTextAsync(exportPath);
        Assert.Contains("Export Test", json);
    }

    [Fact]
    public async Task ExportPresetAsync_WithInvalidId_ThrowsInvalidOperationException()
    {
        // Arrange
        var exportPath = Path.Combine(_testPresetsDirectory, "export.json");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.ExportPresetAsync("invalid-id", exportPath));
    }

    [Fact]
    public async Task ImportPresetAsync_WithValidFile_ImportsSuccessfully()
    {
        // Arrange
        var originalPreset = new Preset { Name = "Import Test" };
        var savedPreset = await _service.SavePresetAsync(originalPreset);
        var exportPath = Path.Combine(_testPresetsDirectory, "import.json");
        await _service.ExportPresetAsync(savedPreset.Id, exportPath);

        // Delete the original preset
        await _service.DeletePresetAsync(savedPreset.Id);

        // Act
        var importedPreset = await _service.ImportPresetAsync(exportPath);

        // Assert
        Assert.NotNull(importedPreset);
        Assert.Equal("Import Test", importedPreset.Name);
        Assert.NotEqual(savedPreset.Id, importedPreset.Id); // New ID should be generated
    }

    [Fact]
    public async Task ExportAllPresetsAsync_WithMultiplePresets_ExportsAll()
    {
        // Arrange
        await _service.SavePresetAsync(new Preset { Name = "Preset 1" });
        await _service.SavePresetAsync(new Preset { Name = "Preset 2" });
        var exportPath = Path.Combine(_testPresetsDirectory, "all_presets.json");

        // Act
        await _service.ExportAllPresetsAsync(exportPath);

        // Assert
        Assert.True(File.Exists(exportPath));
        var json = await File.ReadAllTextAsync(exportPath);
        Assert.Contains("Preset 1", json);
        Assert.Contains("Preset 2", json);
    }

    [Fact]
    public async Task ImportPresetsAsync_WithValidFile_ImportsMultiple()
    {
        // Arrange
        await _service.SavePresetAsync(new Preset { Name = "Preset 1" });
        await _service.SavePresetAsync(new Preset { Name = "Preset 2" });
        var exportPath = Path.Combine(_testPresetsDirectory, "multiple.json");
        await _service.ExportAllPresetsAsync(exportPath);

        // Clear all presets
        var allPresets = _service.GetAllPresets().ToList();
        foreach (var preset in allPresets)
        {
            await _service.DeletePresetAsync(preset.Id);
        }

        // Act
        var importedPresets = await _service.ImportPresetsAsync(exportPath);

        // Assert
        Assert.NotNull(importedPresets);
        Assert.Equal(2, importedPresets.Count);
    }

    [Fact]
    public void SetAutoSave_ToTrue_EnablesAutoSave()
    {
        // Act
        _service.SetAutoSave(true);

        // Assert
        Assert.True(_service.IsAutoSaveEnabled);
    }

    [Fact]
    public void SetAutoSave_ToFalse_DisablesAutoSave()
    {
        // Act
        _service.SetAutoSave(false);

        // Assert
        Assert.False(_service.IsAutoSaveEnabled);
    }

    [Fact]
    public async Task TriggerAutoSaveAsync_WhenEnabled_CompletesSuccessfully()
    {
        // Arrange
        _service.SetAutoSave(true);
        _mockEffectEngine.Setup(e => e.ActiveEffectName).Returns("TestEffect");

        var autoSaveCompleted = false;
        _service.AutoSaveCompleted += (sender, args) => autoSaveCompleted = true;

        // Act
        await _service.TriggerAutoSaveAsync();

        // Assert
        Assert.True(autoSaveCompleted);
    }

    [Fact]
    public async Task TriggerAutoSaveAsync_WhenDisabled_SkipsAutoSave()
    {
        // Arrange
        _service.SetAutoSave(false);

        var autoSaveCompleted = false;
        _service.AutoSaveCompleted += (sender, args) => autoSaveCompleted = true;

        // Act
        await _service.TriggerAutoSaveAsync();

        // Assert
        Assert.False(autoSaveCompleted);
    }

    [Fact]
    public async Task GetActivePreset_AfterLoadingPreset_ReturnsActivePreset()
    {
        // Arrange
        var preset = new Preset
        {
            Name = "Active Test",
            ActiveEffectName = "TestEffect",
            EffectConfig = new EffectConfig()
        };
        var savedPreset = await _service.SavePresetAsync(preset);

        _mockEffectEngine
            .Setup(e => e.SetActiveEffectAsync(It.IsAny<string>(), It.IsAny<EffectConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _service.LoadPresetAsync(savedPreset.Id);
        var activePreset = _service.GetActivePreset();

        // Assert
        Assert.NotNull(activePreset);
        Assert.Equal(savedPreset.Id, activePreset.Id);
    }

    [Fact]
    public void GetActivePreset_WithNoActivePreset_ReturnsNull()
    {
        // Act
        var activePreset = _service.GetActivePreset();

        // Assert
        Assert.Null(activePreset);
    }

    [Fact]
    public async Task SavePresetAsync_UpdatesExistingPreset()
    {
        // Arrange
        var preset = new Preset { Name = "Original Name" };
        var savedPreset = await _service.SavePresetAsync(preset);

        // Act
        var updatedPreset = savedPreset with { Name = "Updated Name" };
        await _service.SavePresetAsync(updatedPreset);

        // Assert
        var retrievedPreset = _service.GetPreset(savedPreset.Id);
        Assert.NotNull(retrievedPreset);
        Assert.Equal("Updated Name", retrievedPreset.Name);
        Assert.Single(_service.GetAllPresets()); // Should still be only 1 preset
    }

    [Fact]
    public async Task SavePresetAsync_PersistsToDisk()
    {
        // Arrange
        var preset = new Preset { Name = "Persistence Test" };
        await _service.SavePresetAsync(preset);

        // Act - Create a new service instance to load from disk
        var newService = new PresetService(_mockLogger.Object, _mockEffectEngine.Object);
        var presetsDirectoryField = typeof(PresetService).GetField("_presetsDirectory",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        presetsDirectoryField?.SetValue(newService, _testPresetsDirectory);

        // Reload from disk manually by calling private method
        var loadMethod = typeof(PresetService).GetMethod("LoadPresetCollectionFromDisk",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var collection = loadMethod?.Invoke(newService, null) as PresetCollection;

        // Assert
        Assert.NotNull(collection);
        Assert.Single(collection.Presets);
        Assert.Equal("Persistence Test", collection.Presets[0].Name);
        
        newService.Dispose();
    }

    [Fact]
    public async Task SavePresetAsync_PersistsColorVariationParameters()
    {
        // Arrange
        var effectConfig = new EffectConfig
        {
            Intensity = 0.9,
            Speed = 2.5,
            Brightness = 0.7,
            AudioSensitivity = 0.6,
            HueVariation = 0.8,
            Saturation = 0.95,
            ColorTemperature = 0.3
        };
        
        var preset = new Preset
        {
            Name = "Color Variation Test",
            EffectConfig = effectConfig
        };

        // Act
        var savedPreset = await _service.SavePresetAsync(preset);

        // Assert
        Assert.NotNull(savedPreset.EffectConfig);
        Assert.Equal(0.8, savedPreset.EffectConfig.HueVariation);
        Assert.Equal(0.95, savedPreset.EffectConfig.Saturation);
        Assert.Equal(0.3, savedPreset.EffectConfig.ColorTemperature);
        
        // Verify persistence to disk
        var retrievedPreset = _service.GetPreset(savedPreset.Id);
        Assert.NotNull(retrievedPreset);
        Assert.NotNull(retrievedPreset.EffectConfig);
        Assert.Equal(0.8, retrievedPreset.EffectConfig.HueVariation);
        Assert.Equal(0.95, retrievedPreset.EffectConfig.Saturation);
        Assert.Equal(0.3, retrievedPreset.EffectConfig.ColorTemperature);
    }

    [Fact]
    public async Task ExportImportPreset_PreservesColorVariationParameters()
    {
        // Arrange
        var effectConfig = new EffectConfig
        {
            HueVariation = 0.75,
            Saturation = 0.85,
            ColorTemperature = 0.4
        };
        
        var preset = new Preset
        {
            Name = "Export Test",
            EffectConfig = effectConfig
        };
        
        var savedPreset = await _service.SavePresetAsync(preset);
        var exportPath = Path.Combine(_testPresetsDirectory, "export_test.json");

        // Act
        await _service.ExportPresetAsync(savedPreset.Id, exportPath);
        var importedPreset = await _service.ImportPresetAsync(exportPath);

        // Assert
        Assert.NotNull(importedPreset.EffectConfig);
        Assert.Equal(0.75, importedPreset.EffectConfig.HueVariation);
        Assert.Equal(0.85, importedPreset.EffectConfig.Saturation);
        Assert.Equal(0.4, importedPreset.EffectConfig.ColorTemperature);
    }
}
