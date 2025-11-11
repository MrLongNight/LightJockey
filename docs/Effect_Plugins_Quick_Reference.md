# Effect Plugins Quick Reference

## Effect Selection Guide

### Choose Your Effect Based on Your Needs

#### 🎨 **For Ambient/Decorative Lighting**
- **RainbowCycleEffect** (HTTPS) - Smooth rainbow transitions
- **SmoothFadeEffect** (HTTPS) - Gentle breathing effect
- **SparkleEffect** (DTLS) - Twinkling stars

#### 🎵 **For Music Visualization**
- **FFTLowFrequencyEffect** (HTTPS) - Best for bass-heavy music (EDM, hip-hop)
- **FFTMidFrequencyEffect** (HTTPS) - Best for vocals and melodies
- **FFTHighFrequencyEffect** (DTLS) - Best for high-energy electronic music
- **PulseEffect** (DTLS) - Beat-synchronized pulsing

#### 🎉 **For Parties/High Energy**
- **RainbowFastEffect** (DTLS) - Rapid rainbow cycling
- **ChaseEffect** (DTLS) - Running lights
- **StrobeManualEffect** (HTTPS) - Classic strobe ⚠️

## Quick Setup

### Step 1: Connect to Hue Bridge
1. Click "Discover Bridges"
2. Select your bridge
3. Press the physical button on your Hue Bridge
4. Click "Connect"

### Step 2: Start Audio Capture
1. Select your audio device from dropdown
2. Click "Start Capture"

### Step 3: Choose and Start Effect
1. Select effect from "Effect Selection" dropdown
2. Adjust parameters (optional):
   - **Intensity**: How strong the effect is
   - **Speed**: How fast the effect runs
   - **Brightness**: Overall brightness level
   - **Audio Sensitivity**: How much audio influences the effect
3. Enable "Audio Reactive" if desired
4. Click "Start Effect"

## Parameter Guide

| Parameter | Range | Description | Tip |
|-----------|-------|-------------|-----|
| **Intensity** | 0-100% | Effect strength | Start at 80% |
| **Speed** | 0.1x-5.0x | Effect speed | 1.0x is normal |
| **Brightness** | 0-100% | Light brightness | Start at 80% |
| **Audio Sensitivity** | 0-100% | Audio reactivity | Start at 50% |

## Effect Comparison Matrix

| Effect | Type | Speed | Audio Required | Best For |
|--------|------|-------|----------------|----------|
| RainbowCycleEffect | HTTPS | Slow | No | Ambient |
| SmoothFadeEffect | HTTPS | Slow | No | Relaxation |
| FFTLowFrequencyEffect | HTTPS | Slow | Yes | Bass music |
| FFTMidFrequencyEffect | HTTPS | Slow | Yes | Vocals |
| StrobeManualEffect | HTTPS | Medium | No | Parties ⚠️ |
| FFTHighFrequencyEffect | DTLS | Fast | Yes | Electronic |
| RainbowFastEffect | DTLS | Fast | No | High energy |
| PulseEffect | DTLS | Fast | Yes* | Rhythmic |
| ChaseEffect | DTLS | Fast | No | Dynamic |
| SparkleEffect | DTLS | Fast | No | Decorative |

*Optional but recommended

## Recommended Settings by Music Genre

### 🎸 Rock/Metal
- **Effect**: FFTLowFrequencyEffect
- **Intensity**: 90%
- **Speed**: 1.5x
- **Audio Sensitivity**: 60%

### 🎹 Electronic/EDM
- **Effect**: FFTHighFrequencyEffect or RainbowFastEffect
- **Intensity**: 100%
- **Speed**: 2.0x
- **Audio Sensitivity**: 70%

### 🎤 Pop/Vocals
- **Effect**: FFTMidFrequencyEffect
- **Intensity**: 80%
- **Speed**: 1.0x
- **Audio Sensitivity**: 50%

### 🎺 Jazz/Classical
- **Effect**: PulseEffect or SmoothFadeEffect
- **Intensity**: 70%
- **Speed**: 0.8x
- **Audio Sensitivity**: 40%

### 💃 Dance/Party
- **Effect**: ChaseEffect or StrobeManualEffect
- **Intensity**: 100%
- **Speed**: 3.0x
- **Audio Reactive**: Enabled

## Troubleshooting

### Effect Not Working
- ✅ Check Hue Bridge connection
- ✅ Verify lights are powered on
- ✅ Ensure effect is started (not just selected)

### No Audio Reaction
- ✅ Enable "Audio Reactive" checkbox
- ✅ Start audio capture
- ✅ Increase Audio Sensitivity
- ✅ Check audio input is receiving signal (watch the visualizer)

### Lights Too Dim
- ✅ Increase Brightness slider
- ✅ Increase Intensity slider
- ✅ Check individual light settings in Hue app

### Lights Too Flashy
- ✅ Decrease Speed slider
- ✅ Decrease Intensity slider
- ✅ Disable Audio Reactive
- ✅ Enable Smooth Transitions

### Strobe Warning
⚠️ **IMPORTANT**: Strobe and rapid flashing effects may trigger seizures in people with photosensitive epilepsy. Use with extreme caution and never use around individuals with known photosensitivity.

## Advanced Tips

### Creating the Perfect Party Atmosphere
1. Start with **RainbowFastEffect** for arrivals
2. Switch to **FFTLowFrequencyEffect** when music starts
3. Use **PulseEffect** during peak energy
4. End with **SmoothFadeEffect** for wind-down

### Multi-Effect Rotation
While you can't run multiple effects simultaneously, you can:
1. Stop current effect
2. Select new effect
3. Start new effect

This allows you to change the mood throughout an event.

### Power User Settings

For **maximum reactivity**:
- Speed: 2.5x
- Audio Sensitivity: 80%
- Audio Reactive: Enabled
- Smooth Transitions: Disabled

For **smooth ambiance**:
- Speed: 0.5x
- Audio Sensitivity: 30%
- Audio Reactive: Disabled
- Smooth Transitions: Enabled

## Keyboard Shortcuts

Currently, all controls are via UI only. Keyboard shortcuts may be added in future versions.

## Performance Tips

### If experiencing lag:
1. Reduce number of active lights
2. Lower Speed setting
3. Use HTTPS effects instead of DTLS
4. Close other applications

### For best results:
1. Use wired Ethernet connection to Hue Bridge
2. Keep lights within good WiFi range
3. Update Hue Bridge firmware
4. Use Entertainment Areas (for DTLS effects)

## FAQ

**Q: Can I use multiple effects at once?**
A: No, only one effect can be active at a time.

**Q: Why are DTLS effects faster?**
A: DTLS uses UDP protocol with direct streaming, while HTTPS uses REST API calls.

**Q: Do I need Entertainment V2 for all effects?**
A: No, only for DTLS-based effects. HTTPS effects work with standard Hue setup.

**Q: Can I create custom effects?**
A: Not through the UI, but the plugin architecture allows developers to create new effects.

**Q: Will this work with other smart lights?**
A: Currently only Philips Hue is supported.

---

**Enjoy your new lighting effects! 🎨✨**
