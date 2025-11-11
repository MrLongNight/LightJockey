# Task 8 — Preset/Configuration Service

**Status**: ✅ Completed  
**PR**: Task8_PresetService  
**Date**: 2025-11-11

## Objective

Implement a comprehensive preset and configuration service that:
- Automatically saves application state
- Provides manual import/export of presets in JSON format
- Integrates with EffectEngine and UI bindings
- Includes example presets and documentation
- Has comprehensive unit tests for save/load functionality

## Implementation

### 1. Core Models

#### Preset (`Models/Preset.cs`)

Represents a named preset containing complete application configuration.

**Properties:**
- `Id` (string): Unique identifier for the preset (auto-generated GUID)
- `Name` (string): Display name for the preset
- `Description` (string?): Optional description of the preset
- `ActiveEffectName` (string?): Name of the active effect
- `EffectConfig` (EffectConfig?): Effect configuration settings
- `AudioDeviceId` (string?): Selected audio device ID
- `HueBridgeIp` (string?): Hue bridge IP address
- `EntertainmentAreaId` (string?): Entertainment area ID (for Entertainment V2)
- `CreatedAt` (DateTime): Timestamp when the preset was created
- `ModifiedAt` (DateTime): Timestamp when the preset was last modified
- `CustomSettings` (Dictionary<string, object>?): Additional custom settings

**Usage:**
```csharp
var preset = new Preset
{
    Name = "High Energy Party",
    Description = "Fast, responsive lighting for parties",
    ActiveEffectName = "FastEntertainmentEffect",
    EffectConfig = new EffectConfig
    {
        Intensity = 1.0,
        Speed = 2.5,
        AudioSensitivity = 0.8
    }
};
```

#### PresetCollection (`Models/PresetCollection.cs`)

Collection of presets with metadata for persistence.

**Properties:**
- `Version` (string): Version of the preset collection format (default: "1.0")
- `ActivePresetId` (string?): ID of the currently active preset
- `Presets` (List<Preset>): List of all presets
- `LastSavedAt` (DateTime): Timestamp when the collection was last saved

### 2. Service Interface

#### IPresetService (`Services/IPresetService.cs`)

Service interface for managing presets and application configuration persistence.

**Events:**
- `PresetSaved`: Raised when a preset is saved
- `PresetLoaded`: Raised when a preset is loaded
- `PresetDeleted`: Raised when a preset is deleted
- `AutoSaveCompleted`: Raised when auto-save occurs

**Key Methods:**

```csharp
// Preset Management
IReadOnlyList<Preset> GetAllPresets();
Preset? GetPreset(string presetId);
Preset? GetActivePreset();
Task<Preset> SavePresetAsync(Preset preset);
Task<Preset> CreatePresetFromCurrentStateAsync(string name, string? description = null);
Task<bool> LoadPresetAsync(string presetId);
Task<bool> DeletePresetAsync(string presetId);

// Import/Export
Task ExportPresetAsync(string presetId, string filePath);
Task ExportAllPresetsAsync(string filePath);
Task<Preset> ImportPresetAsync(string filePath);
Task<IReadOnlyList<Preset>> ImportPresetsAsync(string filePath);

// Auto-Save
void SetAutoSave(bool enabled);
bool IsAutoSaveEnabled { get; }
Task TriggerAutoSaveAsync();
```

### 3. Service Implementation

#### PresetService (`Services/PresetService.cs`)

Complete implementation of preset management with the following features:

**Key Features:**

1. **Automatic Persistence**
   - Saves to `%APPDATA%\LightJockey\Presets\presets.json`
   - Automatically creates directory structure
   - JSON serialization with proper formatting

2. **Auto-Save**
   - Enabled by default
   - Triggers on effect changes
   - Saves to `autosave.json` in presets directory
   - Can be enabled/disabled via `SetAutoSave(bool)`

3. **Import/Export**
   - Single preset export/import
   - Bulk preset export/import
   - Generates new IDs on import to avoid conflicts
   - Pretty-printed JSON with camelCase naming

4. **EffectEngine Integration**
   - Subscribes to `ActiveEffectChanged` event
   - Captures current effect configuration
   - Applies preset settings on load

5. **Event-Based Architecture**
   - Raises events on save, load, delete operations
   - Enables UI binding and reactive updates

**Storage Location:**
```
%APPDATA%\LightJockey\Presets\
├── presets.json        # Main preset collection
└── autosave.json       # Auto-saved state
```

**Usage Example:**

```csharp
// Inject the service
public class MainViewModel
{
    private readonly IPresetService _presetService;
    
    public MainViewModel(IPresetService presetService)
    {
        _presetService = presetService;
        
        // Subscribe to events
        _presetService.PresetSaved += OnPresetSaved;
        _presetService.PresetLoaded += OnPresetLoaded;
    }
    
    // Create preset from current state
    public async Task SaveCurrentState()
    {
        var preset = await _presetService.CreatePresetFromCurrentStateAsync(
            "My Preset",
            "Custom lighting configuration");
    }
    
    // Load a preset
    public async Task LoadPreset(string presetId)
    {
        await _presetService.LoadPresetAsync(presetId);
    }
    
    // Export preset
    public async Task ExportPreset(string presetId, string filePath)
    {
        await _presetService.ExportPresetAsync(presetId, filePath);
    }
    
    // Import preset
    public async Task ImportPreset(string filePath)
    {
        var preset = await _presetService.ImportPresetAsync(filePath);
    }
}
```

### 4. Dependency Injection

The PresetService is registered in `App.xaml.cs`:

```csharp
services.AddSingleton<Services.IPresetService, Services.PresetService>();
```

This registration happens after EffectEngine registration since PresetService depends on it.

## Example Presets

Four example presets are provided in `docs/examples/presets/`:

### 1. High Energy Party (`high-energy-party.json`)
- **Effect**: FastEntertainmentEffect
- **Intensity**: 1.0
- **Speed**: 2.5
- **Audio Sensitivity**: 0.8
- **Use Case**: High-energy music, parties, dancing

### 2. Ambient Relaxation (`ambient-relaxation.json`)
- **Effect**: SlowHttpsEffect
- **Intensity**: 0.5
- **Speed**: 0.3
- **Audio Sensitivity**: 0.3
- **Use Case**: Relaxation, ambient music, background mood

### 3. Balanced Default (`balanced-default.json`)
- **Effect**: FastEntertainmentEffect
- **Intensity**: 0.8
- **Speed**: 1.0
- **Audio Sensitivity**: 0.5
- **Use Case**: General purpose, most music types

### 4. Cinema Mode (`cinema-mode.json`)
- **Effect**: SlowHttpsEffect
- **Intensity**: 0.3
- **Speed**: 0.5
- **Audio Sensitivity**: 0.4
- **Use Case**: Movies, TV shows with background music

**Import Example Presets:**
```csharp
await _presetService.ImportPresetAsync("docs/examples/presets/balanced-default.json");
```

## Unit Tests

Comprehensive unit tests in `tests/LightJockey.Tests/Services/PresetServiceTests.cs`:

### Test Coverage

**Constructor Tests:**
- ✅ Null logger throws ArgumentNullException
- ✅ Null effect engine throws ArgumentNullException

**Basic Operations:**
- ✅ GetAllPresets initially returns empty list
- ✅ SavePresetAsync saves preset successfully
- ✅ SavePresetAsync with null throws ArgumentNullException
- ✅ SavePresetAsync raises PresetSaved event
- ✅ GetPreset retrieves saved preset
- ✅ GetPreset with null/invalid ID returns null
- ✅ SavePresetAsync updates existing preset
- ✅ SavePresetAsync persists to disk

**Create from State:**
- ✅ CreatePresetFromCurrentStateAsync creates preset
- ✅ CreatePresetFromCurrentStateAsync with null name throws
- ✅ CreatePresetFromCurrentStateAsync with empty name throws

**Load Operations:**
- ✅ LoadPresetAsync loads preset successfully
- ✅ LoadPresetAsync raises PresetLoaded event
- ✅ LoadPresetAsync with null/invalid ID returns false
- ✅ GetActivePreset returns active preset after load
- ✅ GetActivePreset returns null when none active

**Delete Operations:**
- ✅ DeletePresetAsync deletes successfully
- ✅ DeletePresetAsync raises PresetDeleted event
- ✅ DeletePresetAsync with null ID returns false

**Import/Export:**
- ✅ ExportPresetAsync exports to file
- ✅ ExportPresetAsync with invalid ID throws
- ✅ ImportPresetAsync imports successfully
- ✅ ExportAllPresetsAsync exports multiple presets
- ✅ ImportPresetsAsync imports multiple presets

**Auto-Save:**
- ✅ SetAutoSave enables/disables auto-save
- ✅ TriggerAutoSaveAsync completes when enabled
- ✅ TriggerAutoSaveAsync skips when disabled
- ✅ AutoSaveCompleted event raised

**Total Tests:** 33 tests covering all functionality

### Running Tests

```bash
# Build the project
dotnet build

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Architecture Decisions

### 1. Record Types for Immutability

Preset and PresetCollection are implemented as records to support:
- Immutable data structures
- Easy cloning with `with` expressions
- Value-based equality

```csharp
var updatedPreset = preset with { Name = "New Name", ModifiedAt = DateTime.UtcNow };
```

### 2. JSON Serialization

Using `System.Text.Json` with:
- Pretty-printing for human readability
- camelCase naming convention
- Null value omission for compact files

### 3. Auto-Save Strategy

- Auto-save is enabled by default
- Separate `autosave.json` file to avoid polluting preset collection
- Triggered on effect changes via event subscription
- Can be disabled for performance-critical scenarios

### 4. Event-Driven Updates

Events enable:
- UI reactive updates
- Decoupled components
- Extensibility for future features (e.g., cloud sync)

### 5. Storage Location

Using `%APPDATA%\LightJockey\Presets\` because:
- Standard Windows application data location
- User-specific (no admin rights needed)
- Persists across application updates
- Easy to backup/restore

## Integration Points

### EffectEngine Integration

```csharp
// PresetService subscribes to effect changes
_effectEngine.ActiveEffectChanged += OnActiveEffectChanged;

// On preset load, effect is applied
await _effectEngine.SetActiveEffectAsync(
    preset.ActiveEffectName,
    preset.EffectConfig,
    cancellationToken);
```

### Future UI Integration

The service is designed for easy UI binding:

```xml
<!-- Preset List -->
<ListBox ItemsSource="{Binding Presets}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}" />
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>

<!-- Load Button -->
<Button Command="{Binding LoadPresetCommand}" 
        CommandParameter="{Binding SelectedPreset.Id}" />

<!-- Export Button -->
<Button Command="{Binding ExportPresetCommand}" />
```

## Performance Considerations

1. **Disk I/O**: Async file operations to avoid blocking UI
2. **Memory**: Presets loaded in memory for fast access
3. **Auto-Save**: Debouncing could be added if effect changes are too frequent
4. **JSON**: Efficient serialization with `System.Text.Json`

## Future Enhancements

Potential improvements for future tasks:

1. **Preset Categories/Tags**: Organize presets by genre, mood, etc.
2. **Cloud Sync**: Sync presets across devices
3. **Preset Sharing**: Share presets with other users
4. **Preset Preview**: Preview effect before applying
5. **Preset Scheduling**: Automatically switch presets based on time/playlist
6. **Import from URL**: Load presets from web URLs
7. **Preset Validation**: Validate presets before import
8. **Backup/Restore**: Automated backup of preset collections
9. **Version Migration**: Handle preset format changes across versions
10. **Encryption**: Encrypt sensitive settings in presets

## API Reference

### PresetService Methods

| Method | Parameters | Returns | Description |
|--------|-----------|---------|-------------|
| `GetAllPresets()` | None | `IReadOnlyList<Preset>` | Gets all presets |
| `GetPreset(id)` | `string` | `Preset?` | Gets preset by ID |
| `GetActivePreset()` | None | `Preset?` | Gets active preset |
| `SavePresetAsync(preset)` | `Preset` | `Task<Preset>` | Saves preset |
| `CreatePresetFromCurrentStateAsync(name, desc)` | `string, string?` | `Task<Preset>` | Creates from state |
| `LoadPresetAsync(id)` | `string` | `Task<bool>` | Loads preset |
| `DeletePresetAsync(id)` | `string` | `Task<bool>` | Deletes preset |
| `ExportPresetAsync(id, path)` | `string, string` | `Task` | Exports preset |
| `ExportAllPresetsAsync(path)` | `string` | `Task` | Exports all |
| `ImportPresetAsync(path)` | `string` | `Task<Preset>` | Imports preset |
| `ImportPresetsAsync(path)` | `string` | `Task<IReadOnlyList<Preset>>` | Imports multiple |
| `SetAutoSave(enabled)` | `bool` | `void` | Enable/disable auto-save |
| `TriggerAutoSaveAsync()` | None | `Task` | Manual auto-save trigger |

## Validation & Testing

### Manual Testing Steps

1. **Create Preset**
   ```csharp
   var preset = await service.CreatePresetFromCurrentStateAsync("Test", "Description");
   ```
   - Verify preset is saved
   - Check file exists in `%APPDATA%\LightJockey\Presets\presets.json`

2. **Load Preset**
   ```csharp
   await service.LoadPresetAsync(presetId);
   ```
   - Verify effect changes
   - Verify PresetLoaded event fires

3. **Export/Import**
   ```csharp
   await service.ExportPresetAsync(presetId, "export.json");
   var imported = await service.ImportPresetAsync("export.json");
   ```
   - Verify exported JSON is valid
   - Verify imported preset has new ID

4. **Auto-Save**
   - Change active effect
   - Wait for auto-save
   - Check `autosave.json` exists and is updated

### Automated Test Results

All 33 unit tests pass successfully:
- ✅ Constructor validation
- ✅ CRUD operations
- ✅ Import/Export functionality
- ✅ Auto-save behavior
- ✅ Event handling
- ✅ Persistence verification

## Conclusion

The PresetService provides a complete solution for preset management in LightJockey:

✅ **Automatic Persistence**: Configuration saved automatically  
✅ **Import/Export**: Full JSON import/export support  
✅ **EffectEngine Integration**: Seamless effect configuration  
✅ **UI Ready**: Event-driven for reactive UI bindings  
✅ **Well Documented**: Complete documentation with examples  
✅ **Fully Tested**: 33 unit tests with comprehensive coverage  

The service is production-ready and provides a solid foundation for the UI implementation in Task 7 and future enhancements.
