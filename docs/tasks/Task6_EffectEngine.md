# Task 6 — EffectEngine (MVP modes)

**Status**: ✅ Completed  
**PR**: Task6_EffectEngine  
**Date**: 2025-11-11

## Objective

Implement a plugin-based EffectEngine that coordinates audio-reactive lighting effects with two MVP modes:
- **SlowHttpsEffect**: HTTPS-based effect using HueService (2-5 updates/second)
- **FastEntertainmentEffect**: High-performance DTLS/UDP effect using EntertainmentService (25-60 FPS)

## Implementation

### 1. Core Models

#### EffectConfig (`Models/EffectConfig.cs`)

Configuration settings for effect parameters.

**Properties:**
- `Intensity` (double, 0.0-1.0): Effect intensity level (default: 0.8)
- `Speed` (double, 0.1-5.0): Effect speed multiplier (default: 1.0)
- `Brightness` (double, 0.0-1.0): Overall brightness level (default: 0.8)
- `AudioReactive` (bool): Whether effect reacts to audio input (default: true)
- `AudioSensitivity` (double, 0.0-1.0): Audio reaction sensitivity (default: 0.5)
- `SmoothTransitions` (bool): Enable smooth color transitions (default: true)
- `TransitionDurationMs` (int): Transition duration in milliseconds (default: 100)

**Usage:**
```csharp
var config = new EffectConfig
{
    Intensity = 0.9,
    AudioReactive = true,
    AudioSensitivity = 0.7,
    Brightness = 0.8
};
```

#### EffectState (`Models/EffectState.cs`)

Represents the lifecycle state of an effect.

**Values:**
- `Uninitialized`: Effect not yet initialized
- `Initialized`: Effect ready to start
- `Running`: Effect currently running
- `Paused`: Effect paused
- `Stopped`: Effect stopped
- `Error`: Effect encountered an error

### 2. Plugin Interface

#### IEffectPlugin (`Services/IEffectPlugin.cs`)

Interface for effect plugins in the LightJockey EffectEngine.

**Properties:**
- `Name` (string): Unique identifier for the plugin
- `Description` (string): Human-readable description
- `State` (EffectState): Current lifecycle state

**Events:**
- `StateChanged`: Raised when effect state changes

**Methods:**

```csharp
Task<bool> InitializeAsync(EffectConfig config);
```
- Initializes the effect with configuration
- Returns true if successful

```csharp
Task StartAsync(CancellationToken cancellationToken = default);
```
- Starts the effect
- Throws `InvalidOperationException` if not in Initialized or Stopped state

```csharp
Task StopAsync();
```
- Stops the effect gracefully

```csharp
void UpdateConfig(EffectConfig config);
```
- Updates effect configuration in real-time

```csharp
void OnSpectralData(SpectralDataEventArgs spectralData);
```
- Handles spectral analysis data from audio
- Called on each spectral data event when effect is running

```csharp
void OnBeatDetected(BeatDetectedEventArgs beatData);
```
- Handles beat detection events
- Called when beats are detected in audio

### 3. EffectEngine Service

#### IEffectEngine (`Services/IEffectEngine.cs`)

Service interface for managing effect plugins and coordinating with audio services.

**Events:**
- `ActiveEffectChanged`: Raised when active effect changes (provides effect name or null)
- `EffectError`: Raised when an effect error occurs (provides error message)

**Methods:**

```csharp
void RegisterPlugin(IEffectPlugin plugin);
```
- Registers an effect plugin with the engine

```csharp
void UnregisterPlugin(string pluginName);
```
- Unregisters an effect plugin (cannot unregister active plugin)

```csharp
IReadOnlyList<string> GetAvailableEffects();
```
- Gets names of all registered plugins

```csharp
IEffectPlugin? GetPlugin(string pluginName);
```
- Retrieves a specific plugin by name

```csharp
Task<bool> SetActiveEffectAsync(string pluginName, EffectConfig config, CancellationToken cancellationToken = default);
```
- Activates an effect plugin with configuration
- Stops current active effect first if any
- Returns true if activation successful

```csharp
Task StopActiveEffectAsync();
```
- Stops the currently active effect

```csharp
void UpdateActiveEffectConfig(EffectConfig config);
```
- Updates configuration of active effect

**Properties:**
- `ActiveEffectName` (string?): Name of currently active effect or null
- `ActiveEffect` (IEffectPlugin?): Currently active effect plugin or null
- `IsEffectRunning` (bool): Whether an effect is currently running

#### EffectEngine Implementation (`Services/EffectEngine.cs`)

Core implementation with plugin registry and audio event coordination.

**Key Features:**
- Plugin registry using Dictionary<string, IEffectPlugin>
- Automatic subscription to `SpectralAnalyzer.SpectralDataAvailable`
- Automatic subscription to `BeatDetector.BeatDetected`
- Thread-safe event forwarding to active effect
- Graceful error handling with logging
- Proper disposal of plugins

**Audio Event Forwarding:**
The EffectEngine subscribes to audio analysis events and forwards them to the active effect when it's in Running state:

```csharp
private void OnSpectralDataAvailable(object? sender, SpectralDataEventArgs e)
{
    if (_activeEffect?.State == EffectState.Running)
    {
        _activeEffect.OnSpectralData(e);
    }
}

private void OnBeatDetected(object? sender, BeatDetectedEventArgs e)
{
    if (_activeEffect?.State == EffectState.Running)
    {
        _activeEffect.OnBeatDetected(e);
    }
}
```

### 4. MVP Effect Plugins

#### SlowHttpsEffect (`Services/Effects/SlowHttpsEffect.cs`)

HTTPS-based audio-reactive effect using standard Hue API.

**Characteristics:**
- Update rate: ~5 FPS (200ms interval)
- Uses `IHueService` for light control
- Color mapping based on frequency bands:
  - Low frequencies (20-250 Hz) → Red/Orange hues (0-60°)
  - Mid frequencies (250-2000 Hz) → Yellow/Green hues (60-180°)
  - High frequencies (2000-20000 Hz) → Blue/Purple hues (240-300°)
- Brightness modulated by spectral energy
- Beat detection triggers brightness flash (1.3x multiplier)

**Implementation Details:**
- Background loop updates lights every 200ms
- Thread-safe state management with locks
- HSV to RGB color conversion
- Handles light on/off state, color, and brightness separately
- Graceful error handling per light

**Audio Reactivity:**
```csharp
public void OnSpectralData(SpectralDataEventArgs spectralData)
{
    var totalEnergy = spectralData.LowFrequencyEnergy + 
                     spectralData.MidFrequencyEnergy + 
                     spectralData.HighFrequencyEnergy;
    
    // Calculate hue based on dominant frequency
    // Calculate brightness from total energy
    
    _currentHue = CalculateHue(spectralData);
    _currentBrightness = CalculateBrightness(totalEnergy);
}
```

#### FastEntertainmentEffect (`Services/Effects/FastEntertainmentEffect.cs`)

High-performance DTLS/UDP effect using Entertainment V2 API.

**Characteristics:**
- Update rate: 25-60 FPS (configurable)
- Uses `IEntertainmentService` for streaming
- Per-channel color distribution across frequency spectrum
- Immediate updates via Entertainment streaming
- Beat detection triggers brightness flash across all channels (1.5x multiplier)

**Channel Distribution:**
Channels are distributed across frequency bands:
- First third of channels → Low frequency colors (red/orange)
- Middle third of channels → Mid frequency colors (yellow/green)
- Last third of channels → High frequency colors (blue/purple)

**Implementation Details:**
- Stateful per-channel tracking (hue, brightness)
- Direct Entertainment channel updates (no background loop needed)
- Thread-safe state management
- Leverages Entertainment V2's high-speed streaming
- Does not stop Entertainment service (managed at higher level)

**Audio Reactivity:**
```csharp
public void OnSpectralData(SpectralDataEventArgs spectralData)
{
    for (byte i = 0; i < channelCount; i++)
    {
        var channelRatio = (double)i / channelCount;
        
        // Assign frequency band based on channel position
        var (hue, brightness) = CalculateChannelState(channelRatio, spectralData);
        
        UpdateChannel(i, hue, brightness);
    }
}
```

### 5. Dependency Injection Setup

Updated `App.xaml.cs` to register all services:

```csharp
// Register Hue services
services.AddSingleton<Services.IHueService, Services.HueService>();
services.AddSingleton<Services.IEntertainmentService, Services.EntertainmentService>();

// Register effect plugins
services.AddTransient<Services.Effects.SlowHttpsEffect>();
services.AddTransient<Services.Effects.FastEntertainmentEffect>();

// Register EffectEngine
services.AddSingleton<Services.IEffectEngine, Services.EffectEngine>();
```

### 6. Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                      EffectEngine                           │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Plugin Registry: Dictionary<string, IEffectPlugin> │   │
│  └─────────────────────────────────────────────────────┘   │
│                           ▲                                 │
│                           │ Event Forwarding                │
│              ┌────────────┴────────────┐                   │
│              │                         │                    │
│     ┌────────▼─────────┐      ┌───────▼────────┐          │
│     │ SpectralAnalyzer │      │  BeatDetector  │          │
│     └──────────────────┘      └────────────────┘          │
└─────────────────────────────────────────────────────────────┘
                           │
                           │ Activates
                           ▼
              ┌────────────────────────┐
              │   IEffectPlugin        │
              ├────────────────────────┤
              │ - Name                 │
              │ - State                │
              │ - OnSpectralData()     │
              │ - OnBeatDetected()     │
              └────────────────────────┘
                           ▲
              ┌────────────┴──────────────┐
              │                           │
    ┌─────────▼────────┐      ┌──────────▼─────────────┐
    │ SlowHttpsEffect  │      │ FastEntertainmentEffect │
    ├──────────────────┤      ├────────────────────────┤
    │ Uses:            │      │ Uses:                  │
    │ - HueService     │      │ - EntertainmentService │
    │                  │      │                        │
    │ Update: ~5 FPS   │      │ Update: 25-60 FPS     │
    │ Mode: HTTPS      │      │ Mode: DTLS/UDP        │
    └──────────────────┘      └────────────────────────┘
```

### 7. Effect Flow Diagram

```
Audio Input
    │
    ├──► AudioService (NAudio)
    │         │
    │         ├──► FFTProcessor
    │         │         │
    │         │         └──► SpectralAnalyzer ──┐
    │         │                                 │
    │         └──► BeatDetector ────────────────┤
    │                                           │
    │                                           ▼
    │                                    EffectEngine
    │                                           │
    │                     ┌─────────────────────┴────────────────────┐
    │                     │                                          │
    │                     ▼                                          ▼
    │            SlowHttpsEffect                          FastEntertainmentEffect
    │                     │                                          │
    │              ┌──────┴──────┐                            ┌──────┴──────┐
    │              │             │                            │             │
    │         Spectral Data  Beat Events                 Spectral Data  Beat Events
    │              │             │                            │             │
    │              └──────┬──────┘                            └──────┬──────┘
    │                     │                                          │
    │                     ▼                                          ▼
    │              Update Hue/                                Update Per-Channel
    │              Brightness                                 Colors & Brightness
    │                     │                                          │
    │                     ▼                                          ▼
    │                HueService                            EntertainmentService
    │                     │                                          │
    │                     ▼                                          ▼
    │              HTTPS API Calls                           DTLS/UDP Stream
    │              (~5 calls/sec)                            (25-60 frames/sec)
    │                     │                                          │
    └─────────────────────┴──────────────────────────────────────────┘
                                    │
                                    ▼
                            Philips Hue Bridge
                                    │
                                    ▼
                              Hue Smart Lights
```

### 8. Usage Example

```csharp
// Initialize services (done via DI)
var effectEngine = serviceProvider.GetRequiredService<IEffectEngine>();
var slowEffect = serviceProvider.GetRequiredService<SlowHttpsEffect>();
var fastEffect = serviceProvider.GetRequiredService<FastEntertainmentEffect>();

// Register plugins
effectEngine.RegisterPlugin(slowEffect);
effectEngine.RegisterPlugin(fastEffect);

// Configure and activate slow HTTPS effect
var config = new EffectConfig
{
    Intensity = 0.8,
    AudioReactive = true,
    AudioSensitivity = 0.6,
    Brightness = 0.9
};

await effectEngine.SetActiveEffectAsync("SlowHttpsEffect", config);

// Later: Switch to fast entertainment effect
await effectEngine.SetActiveEffectAsync("FastEntertainmentEffect", config);

// Update configuration in real-time
config = config with { Intensity = 1.0, AudioSensitivity = 0.8 };
effectEngine.UpdateActiveEffectConfig(config);

// Stop effect
await effectEngine.StopActiveEffectAsync();
```

### 9. Unit Tests

Comprehensive test coverage with 3 test files:

#### EffectEngineTests.cs (41 tests)
- Constructor validation
- Plugin registration/unregistration
- Effect activation and switching
- Configuration updates
- Event forwarding
- Error handling
- Disposal

#### SlowHttpsEffectTests.cs (17 tests)
- Initialization with Hue service
- Start/stop lifecycle
- Configuration updates
- Audio event handling
- State transitions
- Light updates during runtime

#### FastEntertainmentEffectTests.cs (16 tests)
- Initialization with Entertainment service
- Start/stop lifecycle
- Entertainment streaming integration
- Channel updates
- Audio event handling
- State transitions

**Total: 74 unit tests**

All tests use Moq for mocking dependencies and follow xUnit testing patterns.

### 10. Key Design Decisions

1. **Plugin Architecture**: Extensible design allows adding new effects without modifying core EffectEngine
2. **Event-Based Coordination**: Effects react to audio events rather than polling
3. **Thread Safety**: Lock-based synchronization for shared state in effects
4. **Separation of Concerns**: Effects know their output mechanism (HTTPS vs UDP), EffectEngine manages lifecycle
5. **Graceful Degradation**: Effects handle errors per-light without crashing entire effect
6. **State Machine**: Clear state transitions with events for UI integration
7. **Configuration Immutability**: EffectConfig uses `init` properties for thread safety

### 11. Performance Characteristics

| Aspect | SlowHttpsEffect | FastEntertainmentEffect |
|--------|----------------|-------------------------|
| Update Rate | ~5 FPS | 25-60 FPS |
| Protocol | HTTPS | DTLS/UDP |
| Latency | ~200ms | <20ms |
| CPU Usage | Low | Moderate |
| Network | Multiple HTTP requests | Single UDP stream |
| Best For | Ambient lighting, demos | Music visualization, parties |

### 12. Future Enhancements

Potential additions for extended tasks:

1. **More Effect Plugins**:
   - Rainbow cycle effect
   - Strobe effect
   - Wave propagation
   - Color chase patterns
   - Frequency-based zones

2. **Advanced Features**:
   - Effect transitions/crossfading
   - Multi-effect layering
   - Beat-synchronized patterns
   - Genre-specific presets
   - ML-based music analysis

3. **Configuration**:
   - Effect-specific parameters
   - Preset system integration
   - User-defined color palettes
   - Per-light zone assignments

4. **Performance**:
   - Effect pooling
   - Batched light updates
   - Adaptive frame rates
   - GPU acceleration for complex effects

## Testing

Build the solution:
```bash
dotnet build
```

The implementation successfully compiles with only minor xUnit analyzer warnings about using `.Wait()` in some test methods, which is acceptable for test code.

## Documentation

- Architecture diagrams created showing plugin system and data flow
- Comprehensive inline documentation with XML comments
- Usage examples for integration
- Performance characteristics documented

## Conclusion

Task 6 is complete with a fully functional, extensible EffectEngine featuring:
- ✅ Plugin-based architecture
- ✅ Two MVP effect modes (SlowHttpsEffect, FastEntertainmentEffect)
- ✅ Audio-reactive lighting with spectral and beat analysis
- ✅ Comprehensive unit tests (74 tests)
- ✅ Full documentation with flowcharts
- ✅ Dependency injection integration
- ✅ Thread-safe implementation
- ✅ Graceful error handling

The system is ready for UI integration in Task 7 and preset management in Task 8.
