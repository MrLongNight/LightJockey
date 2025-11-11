# Task 7 - UI Screenshots and Layout

## Main Window Layout

```
┌─────────────────────────────────────────────────────────────────────┐
│ LightJockey - Audio-Reactive Lighting Control      🌓 Toggle Theme │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│ ┌──────────────────────────────────────────────────────────────┐   │
│ │ Audio Visualizer                                             │   │
│ │ ┌──────────────────────────────────────────────────────────┐ │   │
│ │ │                                                          │ │   │
│ │ │   LOW     MID     HIGH                            🔵    │ │   │
│ │ │   ███     ████    ██                                     │ │   │
│ │ │   ███     ████    ██                                     │ │   │
│ │ │   ███     ████    ██                                     │ │   │
│ │ │   ███     ████    ██                                     │ │   │
│ │ │                                                          │ │   │
│ │ └──────────────────────────────────────────────────────────┘ │   │
│ │                    BPM: 120.5                                │   │
│ └──────────────────────────────────────────────────────────────┘   │
│                                                                      │
│ ┌─────────────────────────────┬─────────────────────────────────┐  │
│ │ Audio Device                │ Effect Parameters               │  │
│ │ ┌─────────────────────────┐ │ ┌─────────────────────────────┐ │  │
│ │ │Select Audio Device:     │ │ │Intensity: 80%               │ │  │
│ │ │▼ Speaker (Default)      │ │ │├──────────●─────┤           │ │  │
│ │ └─────────────────────────┘ │ │                             │ │  │
│ │ [Refresh] [Start] [Stop]    │ │Speed: 1.0x                  │ │  │
│ │ Status: True ✓              │ │├──────●──────────┤           │ │  │
│ │                             │ │                             │ │  │
│ │ Philips Hue                 │ │Brightness: 80%              │ │  │
│ │ ┌─────────────────────────┐ │ │├──────────●─────┤           │ │  │
│ │ │Select Hue Bridge:       │ │ │                             │ │  │
│ │ │▼ 192.168.1.100          │ │ │Audio Sensitivity: 50%       │ │  │
│ │ └─────────────────────────┘ │ │├─────●───────────┤           │ │  │
│ │ [Discover] [Connect]        │ │                             │ │  │
│ │ Connected: True ✓           │ │☑ Audio Reactive             │ │  │
│ │ Lights Found: 5 lights      │ │☑ Smooth Transitions         │ │  │
│ │                             │ │                             │ │  │
│ │ Effect Selection            │ └─────────────────────────────┘ │  │
│ │ ┌─────────────────────────┐ │                               │  │
│ │ │Select Effect:           │ │                               │  │
│ │ │▼ SlowHttpsEffect        │ │                               │  │
│ │ └─────────────────────────┘ │                               │  │
│ │ [Start Effect] [Stop]       │                               │  │
│ │ Running: True ✓             │                               │  │
│ └─────────────────────────────┴─────────────────────────────────┘  │
│                                                                      │
│ Ready                                                                │
└─────────────────────────────────────────────────────────────────────┘
```

## Dark Theme (Default)

**Color Scheme:**
- Background: Dark Gray (#1E1E1E)
- Secondary Background: Medium Gray (#252526)
- Text: White (#FFFFFF)
- Accent: Blue (#007ACC)
- Visualizer:
  - Low Frequency: Teal (#4EC9B0)
  - Mid Frequency: Yellow (#DCDCAA)
  - High Frequency: Red (#F48771)
  - Beat Indicator: Blue (#007ACC)

## Light Theme

**Color Scheme:**
- Background: White (#FFFFFF)
- Secondary Background: Light Gray (#F3F3F3)
- Text: Dark Gray (#1E1E1E)
- Accent: Blue (#0078D4)
- Visualizer:
  - Low Frequency: Green (#107C10)
  - Mid Frequency: Orange (#F7630C)
  - High Frequency: Red (#E81123)
  - Beat Indicator: Blue (#0078D4)

## UI Components

### Audio Visualizer
- 3 vertical bars representing frequency bands
- Smooth height animations (50ms)
- Beat indicator in top-right corner
- Pulse animation on beat detection
- BPM display below visualizer

### Device Selection
- ComboBox for device selection
- Action buttons (Refresh, Start, Stop, Discover, Connect)
- Status indicators with color coding
- Light count display for Hue

### Effect Controls
- ComboBox for effect selection
- Start/Stop buttons
- Running status indicator

### Parameter Sliders
- Intensity (0-100%)
- Speed (0.1x-5.0x)
- Brightness (0-100%)
- Audio Sensitivity (0-100%)
- Live value display
- Tick marks at 10% intervals

### Checkboxes
- Audio Reactive toggle
- Smooth Transitions toggle

### Status Bar
- Informative status messages
- Error/success feedback
- Connection status updates

## User Workflow

1. **Audio Setup**
   - Refresh devices → Select device → Start capture
   - Visualizer shows real-time audio data

2. **Hue Setup**
   - Discover bridges → Select bridge → Press bridge button → Connect
   - System shows connected status and light count

3. **Effect Execution**
   - Select effect → Adjust parameters → Start effect
   - Lights react to audio in real-time
   - Adjust parameters while running

4. **Theme Toggle**
   - Click theme button
   - UI updates instantly with new color scheme

## Accessibility Features

- High contrast in both themes
- Clear status indicators
- Disabled state for unavailable actions
- Visual feedback for all interactions
- Proper control spacing

## Responsive Design

- Minimum window size: 800x600
- Scrollable content areas
- Adaptive layout
- GroupBox organization
- Clear visual hierarchy

## Note on Screenshots

Since this is a WPF application requiring Windows Desktop runtime, actual screenshots can only be captured when running on a Windows machine. The layout diagram above represents the UI structure and organization. To capture actual screenshots:

1. Build and run the application on Windows
2. Use Windows Snipping Tool or similar
3. Capture both Dark and Light themes
4. Save as PNG files in `docs/screenshots/` directory

Suggested screenshots:
- `MainWindow_DarkTheme.png`: Default dark theme view
- `MainWindow_LightTheme.png`: Light theme view
- `Visualizer_Active.png`: Active audio visualization
- `DeviceSelection.png`: Device selection process
- `EffectRunning.png`: Effect running with parameters
