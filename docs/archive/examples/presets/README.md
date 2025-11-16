# LightJockey Example Presets

This directory contains example preset configurations for LightJockey. These presets demonstrate different lighting configurations optimized for various use cases.

## Available Presets

### 1. High Energy Party
**File:** `high-energy-party.json`

Perfect for high-energy music, parties, and dancing.

- **Effect:** FastEntertainmentEffect
- **Intensity:** Maximum (1.0)
- **Speed:** Fast (2.5x)
- **Audio Sensitivity:** High (0.8)
- **Transitions:** Quick and responsive (50ms)
- **Best For:** EDM, electronic music, dance parties

### 2. Ambient Relaxation
**File:** `ambient-relaxation.json`

Smooth, subtle lighting for relaxation and ambient music.

- **Effect:** SlowHttpsEffect
- **Intensity:** Low (0.5)
- **Speed:** Slow (0.3x)
- **Audio Sensitivity:** Low (0.3)
- **Transitions:** Smooth and gradual (500ms)
- **Best For:** Chill music, meditation, relaxation, background ambiance

### 3. Balanced Default
**File:** `balanced-default.json`

Balanced settings suitable for most music types.

- **Effect:** FastEntertainmentEffect
- **Intensity:** Medium-High (0.8)
- **Speed:** Normal (1.0x)
- **Audio Sensitivity:** Medium (0.5)
- **Transitions:** Smooth (100ms)
- **Best For:** Pop, rock, general listening, default configuration

### 4. Cinema Mode
**File:** `cinema-mode.json`

Dim, responsive lighting for watching movies with background music.

- **Effect:** SlowHttpsEffect
- **Intensity:** Low (0.3)
- **Speed:** Medium-Slow (0.5x)
- **Audio Sensitivity:** Low-Medium (0.4)
- **Transitions:** Smooth (300ms)
- **Best For:** Movies, TV shows, gaming with soundtrack

## How to Import Presets

### Using the PresetService API

```csharp
// Import a single preset
var presetService = serviceProvider.GetRequiredService<IPresetService>();
var preset = await presetService.ImportPresetAsync("path/to/high-energy-party.json");

// Load the imported preset
await presetService.LoadPresetAsync(preset.Id);
```

### Using the UI (Future)

1. Open LightJockey
2. Navigate to Presets menu
3. Click "Import Preset"
4. Select the desired `.json` file
5. The preset will be added to your collection

## Creating Custom Presets

You can create your own presets by copying and modifying these examples:

### Preset Structure

```json
{
  "id": "unique-preset-id",
  "name": "Display Name",
  "description": "Description of the preset",
  "activeEffectName": "EffectName",
  "effectConfig": {
    "intensity": 0.8,
    "speed": 1.0,
    "brightness": 0.8,
    "audioReactive": true,
    "audioSensitivity": 0.5,
    "smoothTransitions": true,
    "transitionDurationMs": 100
  },
  "createdAt": "2025-11-11T00:00:00Z",
  "modifiedAt": "2025-11-11T00:00:00Z"
}
```

### Effect Configuration Parameters

| Parameter | Type | Range | Description |
|-----------|------|-------|-------------|
| `intensity` | double | 0.0 - 1.0 | Overall effect intensity |
| `speed` | double | 0.1 - 5.0 | Effect speed multiplier |
| `brightness` | double | 0.0 - 1.0 | Light brightness level |
| `audioReactive` | bool | true/false | Enable audio reactivity |
| `audioSensitivity` | double | 0.0 - 1.0 | How responsive to audio |
| `smoothTransitions` | bool | true/false | Enable smooth color transitions |
| `transitionDurationMs` | int | 0 - 1000 | Transition duration in milliseconds |

### Available Effects

- **FastEntertainmentEffect**: High-performance DTLS/UDP streaming (25-60 FPS)
- **SlowHttpsEffect**: HTTPS-based effect (2-5 updates/second)

## Tips for Custom Presets

1. **For Fast Music:** Use FastEntertainmentEffect with high speed and sensitivity
2. **For Slow Music:** Use SlowHttpsEffect with longer transitions
3. **For Parties:** Maximize intensity and disable smooth transitions for sharp changes
4. **For Relaxation:** Lower intensity and speed, enable smooth transitions
5. **Testing:** Import your preset and test with different music genres to fine-tune

## Sharing Presets

You can share your custom presets with others:

1. Export your preset using the PresetService
2. Share the generated `.json` file
3. Others can import it into their LightJockey installation

## Preset Locations

- **User Presets:** `%APPDATA%\LightJockey\Presets\presets.json`
- **Auto-Save:** `%APPDATA%\LightJockey\Presets\autosave.json`
- **Example Presets:** `docs/examples/presets/*.json`

## Troubleshooting

### Import Fails

- Ensure the JSON is valid (use a JSON validator)
- Check that all required fields are present
- Verify effect names match available effects

### Preset Doesn't Apply Correctly

- Check that the effect name is valid
- Ensure the Hue Bridge is connected
- Verify audio device is selected and streaming

### Auto-Save Not Working

- Check that auto-save is enabled: `presetService.IsAutoSaveEnabled`
- Verify write permissions to `%APPDATA%\LightJockey\Presets\`

## Contributing

Have a great preset to share? Consider submitting a pull request to add it to the example presets collection!

## License

These example presets are provided as part of LightJockey and are free to use, modify, and share.
