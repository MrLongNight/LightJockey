# UI Screenshots and Visual Reference

## Effect Selection in Main Window

The LightJockey main window now includes all 12 effect plugins (2 original + 10 new) available in the Effect Selection dropdown:

### Effect Dropdown Contents

When you open the "Effect Selection" dropdown, you'll see:

**Original Effects:**
1. SlowHttpsEffect
2. FastEntertainmentEffect

**New HTTPS Effects:**
3. RainbowCycleEffect
4. SmoothFadeEffect
5. FFTLowFrequencyEffect
6. FFTMidFrequencyEffect
7. StrobeManualEffect

**New DTLS Effects:**
8. FFTHighFrequencyEffect
9. RainbowFastEffect
10. PulseEffect
11. ChaseEffect
12. SparkleEffect

## Main Window Layout

```
┌────────────────────────────────────────────────────────────────┐
│ LightJockey                                    🌓 Toggle Theme │
├────────────────────────────────────────────────────────────────┤
│ ┌─ Audio Visualizer ──────────────────────────────────────┐    │
│ │  [====]  [==========]  [=====]                          │    │
│ │   Low       Mid        High                             │    │
│ │  Frequency Bands Visualization                          │    │
│ │                                                          │    │
│ │  BPM: 120.5                                             │    │
│ └──────────────────────────────────────────────────────────┘    │
├────────────────────────────────────────────────────────────────┤
│ ┌─ Audio Device ────┐  ┌─ Effect Parameters ──────────────┐   │
│ │ Select Device:    │  │ Intensity: 80% [========    ]    │   │
│ │ [Dropdown▼]       │  │                                   │   │
│ │                   │  │ Speed: 1.0x [=====          ]    │   │
│ │ [Refresh] [Start] │  │                                   │   │
│ │          [Stop]   │  │ Brightness: 80% [========    ]   │   │
│ └───────────────────┘  │                                   │   │
│                        │ Audio Sensitivity: 50%            │   │
│ ┌─ Philips Hue ─────┐  │ [=====              ]            │   │
│ │ Select Bridge:    │  │                                   │   │
│ │ [192.168.1.X ▼]   │  │ ☑ Audio Reactive                 │   │
│ │                   │  │ ☑ Smooth Transitions             │   │
│ │ [Discover][Connect│  └───────────────────────────────────┘   │
│ │ Connected: ✓      │                                          │
│ │ Lights: 5         │                                          │
│ └───────────────────┘                                          │
│                                                                 │
│ ┌─ Effect Selection ─┐                                         │
│ │ Select Effect:     │                                         │
│ │ [RainbowCycleEffect▼]                                        │
│ │                    │                                         │
│ │ [Start Effect]     │                                         │
│ │ [Stop Effect]      │                                         │
│ │ Running: ✓         │                                         │
│ └────────────────────┘                                         │
├────────────────────────────────────────────────────────────────┤
│ Status: Effect 'RainbowCycleEffect' started                    │
└────────────────────────────────────────────────────────────────┘
```

## Effect Visual Characteristics

### Color Schemes by Effect

| Effect | Primary Colors | Pattern | Motion |
|--------|---------------|---------|---------|
| **RainbowCycleEffect** | Full spectrum | Gradient | Slow rotation |
| **SmoothFadeEffect** | White | Solid | Fade in/out |
| **FFTLowFrequencyEffect** | Red, Orange | Solid | Reactive pulses |
| **FFTMidFrequencyEffect** | Green, Yellow | Solid | Reactive pulses |
| **StrobeManualEffect** | White | Flash | On/off |
| **FFTHighFrequencyEffect** | Blue, Purple, Cyan | Solid | Fast reactive |
| **RainbowFastEffect** | Full spectrum | Gradient | Fast rotation |
| **PulseEffect** | Magenta, Purple | Solid | Smooth wave |
| **ChaseEffect** | Orange | Sequential | Directional |
| **SparkleEffect** | White, Yellow | Random | Twinkle |

### Effect Speed Comparison

**Slow (HTTPS):**
- Update rate: ~5 Hz (200ms)
- RainbowCycleEffect at Speed 1.0x: ~30 seconds per full cycle
- SmoothFadeEffect at Speed 1.0x: ~10 seconds per fade cycle

**Fast (DTLS):**
- Update rate: ~60 Hz (16ms)
- RainbowFastEffect at Speed 1.0x: ~10 seconds per full cycle
- PulseEffect at Speed 1.0x: ~2 seconds per pulse

### Audio Visualization Bars

The visualizer shows three frequency bands:
```
[====]        Low:  20-250 Hz    (Bass, kick drums)
[==========]  Mid:  250-2000 Hz  (Vocals, guitars)
[=====]       High: 2000-20000 Hz (Cymbals, hi-hats)
```

The height of each bar indicates the energy level in that frequency band.

### Effect Parameter Impact

**Intensity Slider:**
- 0%: No effect visible
- 50%: Moderate effect
- 100%: Maximum effect strength

**Speed Slider:**
- 0.1x: Very slow (10% of normal speed)
- 1.0x: Normal speed
- 5.0x: Very fast (500% of normal speed)

**Brightness Slider:**
- 0%: Lights off
- 50%: Half brightness
- 100%: Maximum brightness

**Audio Sensitivity:**
- 0%: No audio reaction
- 50%: Moderate reaction
- 100%: Maximum reaction (may be oversensitive)

## Dark Theme vs Light Theme

The application supports both themes:

**Dark Theme (default):**
- Dark background
- Light text
- Green success indicators
- Easier on eyes in low light

**Light Theme:**
- Light background
- Dark text
- Green success indicators
- Better for bright environments

Toggle between themes using the "🌓 Toggle Theme" button in the top right.

## Status Indicators

### Connection Status
- **Audio**: "Status: Capturing" (green) or "Status: Stopped" (default)
- **Hue**: "Connected: True" (green) or "Connected: False" (default)
- **Effect**: "Running: True" (green) or "Running: False" (default)

### Beat Detection
When a beat is detected, you'll see a brief visual flash in the beat indicator.

## Effect-Specific Visual Notes

### RainbowCycleEffect
- Lights will show different colors creating a rainbow spread
- Colors slowly rotate through the spectrum
- All lights stay on continuously

### SmoothFadeEffect
- All lights fade in unison
- Smooth transition from dim to bright to dim
- No color changes (stays white)

### FFT Effects
- Brightness changes rapidly with music
- FFTLowFrequencyEffect: Reacts to bass hits
- FFTMidFrequencyEffect: Reacts to vocals/melody
- FFTHighFrequencyEffect: Reacts to high-pitched sounds

### StrobeManualEffect
⚠️ **Warning**: Rapid flashing effect
- Lights turn completely on and off
- Flash rate controlled by Speed parameter
- Can be disorienting

### PulseEffect
- Smooth breathing effect
- Synchronized with music beats
- More gradual than strobe

### ChaseEffect
- One light at a time moves in sequence
- Creates a "running light" pattern
- Trailing lights show dimmed afterglow

### SparkleEffect
- Random lights twinkle on and off
- Like stars twinkling in the night sky
- Sparkle rate increases with Speed

## Recommendations for Screenshots

Since this is running in a headless environment, actual screenshots cannot be captured. However, when documenting this feature, consider capturing:

1. **Main window with effect dropdown open** - showing all 12 effects
2. **Effect running with audio visualizer active** - showing bars responding to music
3. **Parameter sliders** - demonstrating different settings
4. **Status bar** - showing successful effect activation
5. **Dark and Light themes** - showing both UI modes
6. **Individual effect demonstrations**:
   - Rainbow effects showing color gradients
   - FFT effects reacting to music
   - Chase effect showing sequential pattern
   - Sparkle effect showing random twinkling

## UI Accessibility

The interface is designed to be:
- **Clear**: Labels for all controls
- **Responsive**: Real-time feedback
- **Intuitive**: Familiar slider and checkbox controls
- **Informative**: Status messages at bottom
- **Themed**: Dark and light modes for different preferences

---

**Note**: For actual screenshots, please run the application on a Windows machine with a Hue Bridge connected and lights available. The effects are best observed with 3 or more lights in your Entertainment Area or Hue setup.
