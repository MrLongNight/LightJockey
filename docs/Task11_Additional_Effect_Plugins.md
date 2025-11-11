# Task 11 - Additional Effect Plugins Documentation

## Overview

This document describes the 10 new effect plugins added to the LightJockey application as part of Task 11. These effects provide enhanced entertainment lighting capabilities using both slow HTTPS-based and fast DTLS-based communication protocols.

## Effect Plugins Summary

### Slow HTTPS-Based Effects (5)

These effects use the standard Philips Hue HTTPS API for communication, suitable for slower, smooth transitions:

1. **RainbowCycleEffect** - Smooth rainbow color cycling through the spectrum
2. **SmoothFadeEffect** - Gradual brightness fade in and out
3. **FFTLowFrequencyEffect** - Bass-reactive lighting using low frequency FFT data
4. **FFTMidFrequencyEffect** - Mid-range reactive lighting using mid frequency FFT data
5. **StrobeManualEffect** - Configurable strobe light effect

### Fast DTLS-Based Effects (5)

These effects use the Entertainment V2 DTLS/UDP protocol for high-performance, real-time lighting:

1. **FFTHighFrequencyEffect** - Treble-reactive lighting using high frequency FFT data
2. **RainbowFastEffect** - Fast rainbow color cycling
3. **PulseEffect** - Beat-synchronized pulsing effect
4. **ChaseEffect** - Sequential light chase pattern
5. **SparkleEffect** - Random twinkling sparkle effect

## Detailed Effect Descriptions

### 1. RainbowCycleEffect (HTTPS)

**Purpose**: Creates a smooth rainbow color transition across all lights.

**Behavior**:
- Cycles through the full color spectrum (0-360 degrees hue)
- Each light has an offset hue to create a rainbow spread
- Speed parameter controls the cycling rate
- Brightness and intensity parameters control the overall brightness

**Parameters**:
- `Speed` (0.1-5.0): Controls how fast the rainbow cycles
- `Brightness` (0.0-1.0): Overall brightness level
- `Intensity` (0.0-1.0): Effect intensity multiplier

**Audio Reactive**: No

**Use Cases**:
- Ambient background lighting
- Party atmosphere
- Decorative lighting displays

---

### 2. SmoothFadeEffect (HTTPS)

**Purpose**: Creates a gentle breathing effect by gradually fading lights in and out.

**Behavior**:
- Smoothly transitions from 0% to 100% brightness and back
- Uses white color for a clean fade effect
- Speed parameter controls the fade duration
- Can optionally react to audio energy when audio reactive mode is enabled

**Parameters**:
- `Speed` (0.1-5.0): Controls the fade speed
- `Brightness` (0.0-1.0): Maximum brightness level
- `Intensity` (0.0-1.0): Effect intensity
- `AudioReactive` (bool): Enable audio modulation

**Audio Reactive**: Optional (modulates fade speed based on audio energy)

**Use Cases**:
- Relaxation and meditation
- Sleep aid
- Ambient mood lighting

---

### 3. FFTLowFrequencyEffect (HTTPS)

**Purpose**: Reacts to bass frequencies in the audio spectrum.

**Behavior**:
- Analyzes low frequency band (20-250 Hz) from FFT data
- Brightness responds to bass energy
- Uses red/orange colors to represent low frequencies
- Flashes on beat detection when audio reactive

**Parameters**:
- `AudioReactive` (bool): Must be enabled
- `AudioSensitivity` (0.0-1.0): Sensitivity to bass energy
- `Brightness` (0.0-1.0): Maximum brightness
- `Intensity` (0.0-1.0): Effect intensity

**Audio Reactive**: Yes (required)

**Use Cases**:
- Music visualization
- Bass-heavy music (EDM, hip-hop, rock)
- DJ performances

---

### 4. FFTMidFrequencyEffect (HTTPS)

**Purpose**: Reacts to mid-range frequencies in the audio spectrum.

**Behavior**:
- Analyzes mid frequency band (250-2000 Hz) from FFT data
- Brightness responds to mid-range energy (vocals, guitars, keyboards)
- Uses green/yellow colors to represent mid frequencies
- Enhances brightness on beat detection

**Parameters**:
- `AudioReactive` (bool): Must be enabled
- `AudioSensitivity` (0.0-1.0): Sensitivity to mid-range energy
- `Brightness` (0.0-1.0): Maximum brightness
- `Intensity` (0.0-1.0): Effect intensity

**Audio Reactive**: Yes (required)

**Use Cases**:
- Vocal-heavy music
- Acoustic performances
- Classical music

---

### 5. StrobeManualEffect (HTTPS)

**Purpose**: Creates a classic strobe light effect with configurable timing.

**Behavior**:
- Rapidly toggles lights on and off
- Speed parameter controls strobe frequency
- Uses white color for maximum strobe effect
- Can synchronize with beats when audio reactive

**Parameters**:
- `Speed` (0.1-5.0): Controls strobe frequency (maps to 500ms-50ms intervals)
- `Brightness` (0.0-1.0): Strobe brightness
- `Intensity` (0.0-1.0): Effect intensity
- `AudioReactive` (bool): Enable beat synchronization

**Audio Reactive**: Optional (synchronizes with beats)

**Use Cases**:
- Party lighting
- Dance performances
- Special effects
- ⚠️ **Warning**: May trigger photosensitive epilepsy

---

### 6. FFTHighFrequencyEffect (DTLS)

**Purpose**: Reacts to treble frequencies in the audio spectrum with high-speed updates.

**Behavior**:
- Analyzes high frequency band (2000-20000 Hz) from FFT data
- Uses blue/purple colors to represent high frequencies
- Flashes cyan on beat detection
- Updates at ~60 FPS using DTLS protocol

**Parameters**:
- `AudioReactive` (bool): Must be enabled
- `AudioSensitivity` (0.0-1.0): Sensitivity to treble energy
- `Brightness` (0.0-1.0): Maximum brightness
- `Intensity` (0.0-1.0): Effect intensity

**Audio Reactive**: Yes (required)

**Use Cases**:
- High-energy electronic music
- Cymbal and hi-hat emphasis
- Percussive music

---

### 7. RainbowFastEffect (DTLS)

**Purpose**: Creates rapid rainbow color cycling for high-energy environments.

**Behavior**:
- Similar to RainbowCycleEffect but much faster
- Updates at ~60 FPS for smooth, rapid color transitions
- Each light offset creates a flowing rainbow pattern
- Speed can be accelerated on beat detection when audio reactive

**Parameters**:
- `Speed` (0.1-5.0): Controls cycling speed
- `Brightness` (0.0-1.0): Overall brightness
- `Intensity` (0.0-1.0): Effect intensity
- `AudioReactive` (bool): Enable beat acceleration

**Audio Reactive**: Optional (accelerates on beats)

**Use Cases**:
- High-energy parties
- Dance clubs
- Fast-paced music

---

### 8. PulseEffect (DTLS)

**Purpose**: Creates a smooth pulsing effect synchronized with music beats.

**Behavior**:
- Uses sine wave for smooth pulsing motion
- Pulses are synchronized with beat detection
- Uses magenta/purple color scheme
- Pulse intensity modulated by total audio energy

**Parameters**:
- `Speed` (0.1-5.0): Controls pulse frequency
- `Brightness` (0.0-1.0): Maximum brightness
- `Intensity` (0.0-1.0): Effect intensity
- `AudioReactive` (bool): Enable audio synchronization

**Audio Reactive**: Optional (synchronizes and modulates with audio)

**Use Cases**:
- Rhythmic music
- Trance and ambient music
- Meditation with music

---

### 9. ChaseEffect (DTLS)

**Purpose**: Creates a sequential light chase pattern.

**Behavior**:
- Lights up one channel at a time in sequence
- Creates a trailing effect behind the active light
- Speed controls how fast the chase moves
- Can advance on beat when audio reactive

**Parameters**:
- `Speed` (0.1-5.0): Controls chase speed (maps to 500ms-50ms intervals)
- `Brightness` (0.0-1.0): Brightness of active lights
- `Intensity` (0.0-1.0): Effect intensity
- `AudioReactive` (bool): Enable beat-synchronized advancement

**Audio Reactive**: Optional (advances on beats)

**Use Cases**:
- Running lights effect
- Directional lighting
- Stage lighting

---

### 10. SparkleEffect (DTLS)

**Purpose**: Creates random twinkling lights like stars or fireflies.

**Behavior**:
- Randomly selects channels to "sparkle"
- Sparkles fade out gradually
- Higher speeds create more frequent sparkles
- Audio energy and beats trigger additional sparkles

**Parameters**:
- `Speed` (0.1-5.0): Controls sparkle frequency
- `Brightness` (0.0-1.0): Maximum sparkle brightness
- `Intensity` (0.0-1.0): Effect intensity
- `AudioReactive` (bool): Enable audio-triggered sparkles

**Audio Reactive**: Optional (triggers sparkles on energy/beats)

**Use Cases**:
- Ambient magical effects
- Holiday lighting
- Decorative displays

## Technical Implementation

### Effect Plugin Architecture

All effects implement the `IEffectPlugin` interface:

```csharp
public interface IEffectPlugin : IDisposable
{
    string Name { get; }
    string Description { get; }
    EffectState State { get; }
    event EventHandler<EffectState>? StateChanged;
    
    Task<bool> InitializeAsync(EffectConfig config);
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync();
    void UpdateConfig(EffectConfig config);
    void OnSpectralData(SpectralDataEventArgs spectralData);
    void OnBeatDetected(BeatDetectedEventArgs beatData);
}
```

### Effect Configuration

Effects are configured using the `EffectConfig` class:

```csharp
public class EffectConfig
{
    public double Intensity { get; init; } = 0.8;           // 0.0 - 1.0
    public double Speed { get; init; } = 1.0;               // 0.1 - 5.0
    public double Brightness { get; init; } = 0.8;          // 0.0 - 1.0
    public bool AudioReactive { get; init; } = true;
    public double AudioSensitivity { get; init; } = 0.5;    // 0.0 - 1.0
    public bool SmoothTransitions { get; init; } = true;
    public int TransitionDurationMs { get; init; } = 100;
}
```

### Registration

Effects are registered in `App.xaml.cs`:

```csharp
// Register effect plugins
services.AddTransient<Services.Effects.RainbowCycleEffect>();
services.AddTransient<Services.Effects.SmoothFadeEffect>();
// ... etc

// Register with EffectEngine
services.AddSingleton<Services.IEffectEngine>(sp =>
{
    var engine = new Services.EffectEngine(/* dependencies */);
    engine.RegisterPlugin(sp.GetRequiredService<Services.Effects.RainbowCycleEffect>());
    // ... etc
    return engine;
});
```

## User Interface

The UI provides:

1. **Effect Selection Dropdown**: Lists all available effects by name
2. **Effect Control Parameters**:
   - Intensity slider (0-100%)
   - Speed slider (0.1x - 5.0x)
   - Brightness slider (0-100%)
   - Audio Sensitivity slider (0-100%)
   - Audio Reactive checkbox
   - Smooth Transitions checkbox

3. **Start/Stop Controls**: Buttons to activate and deactivate effects

## Testing

Unit tests are provided for all effects covering:
- Constructor validation (null parameter checks)
- Name and description verification
- State management (initialization, running, stopped)
- Configuration updates
- Audio event handling (spectral data and beat detection)
- Proper cleanup and disposal

Test files:
- `RainbowCycleEffectTests.cs`
- `FFTLowFrequencyEffectTests.cs`
- `FFTHighFrequencyEffectTests.cs`
- `FastEffectsTests.cs` (PulseEffect, ChaseEffect, SparkleEffect)

## Performance Considerations

### HTTPS Effects
- Update rate: ~5 updates per second (200ms interval)
- Latency: 50-200ms depending on network
- Suitable for: Smooth, gradual effects

### DTLS Effects
- Update rate: ~60 FPS (16ms interval)
- Latency: <10ms
- Suitable for: Fast, reactive, real-time effects

## Future Enhancements

Potential improvements:
1. Custom color palettes for effects
2. Effect combining/layering
3. User-defined effect presets
4. Beat prediction for smoother synchronization
5. Frequency band customization
6. Effect scheduling and automation

## Warnings and Safety

⚠️ **Photosensitivity Warning**: StrobeManualEffect and other rapid flashing effects may trigger seizures in people with photosensitive epilepsy. Use with caution.

⚠️ **Light Longevity**: Rapid on/off switching (strobe effects) may reduce the lifespan of LED bulbs. Use intermittently.

## Support and Troubleshooting

### Common Issues

1. **Effects not appearing in dropdown**
   - Verify effect is registered in `App.xaml.cs`
   - Check that the effect plugin is properly instantiated

2. **DTLS effects not working**
   - Ensure Entertainment V2 is configured and streaming
   - Verify Entertainment Area is set up with lights

3. **Audio reactive effects not responding**
   - Enable "Audio Reactive" checkbox
   - Start audio capture
   - Adjust Audio Sensitivity slider
   - Verify audio input is receiving signal

4. **Performance issues**
   - Reduce number of active lights
   - Lower effect speed
   - Use HTTPS effects instead of DTLS for slower systems

## Conclusion

These 10 new effect plugins significantly enhance the LightJockey application's capabilities, providing users with a wide range of lighting effects suitable for entertainment, relaxation, and creative expression. The combination of HTTPS and DTLS-based effects ensures compatibility across different use cases while maximizing performance where possible.
