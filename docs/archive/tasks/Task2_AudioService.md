# Task 2 — AudioService

**Status**: ✅ Completed  
**PR**: Task2_AudioService  
**Date**: 2025-11-11

## Objective

Implement an audio service that enumerates audio devices, enables device selection through UI, provides stream loopback for effect calculation, and outputs FFT-compatible audio data events.

## Implementation

### 1. NuGet Packages Added

The following NAudio package was added to the main project for audio device management and streaming:

```xml
<PackageReference Include="NAudio" Version="2.2.1" />
```

NAudio provides:
- Audio device enumeration (input and output devices)
- WASAPI loopback capture for system audio
- WaveIn capture for microphone/line-in
- PCM audio data processing

### 2. Models Created

#### AudioDevice (`Models/AudioDevice.cs`)

Represents an audio device available for input or output.

**Properties:**
- `Id` (string): Unique device identifier
- `Name` (string): Friendly device name
- `DeviceType` (AudioDeviceType): Input or Output
- `IsDefault` (bool): Whether this is the default device

**AudioDeviceType Enum:**
- `Input`: Audio input device (microphone, line-in)
- `Output`: Audio output device (speakers, headphones)

#### AudioDataEventArgs (`Models/AudioDataEventArgs.cs`)

Event arguments for FFT-compatible audio data events.

**Properties:**
- `Samples` (float[]): Raw PCM audio samples (normalized to -1.0 to 1.0)
- `SampleRate` (int): Sample rate in Hz (e.g., 44100)
- `Channels` (int): Number of channels (1 for mono, 2 for stereo)
- `Timestamp` (DateTime): UTC timestamp when data was captured

The audio samples are provided as float arrays, which is the standard format for FFT processing libraries.

### 3. Audio Service Interface

#### IAudioService (`Services/IAudioService.cs`)

Service interface for managing audio devices and capturing audio streams.

**Events:**
```csharp
event EventHandler<AudioDataEventArgs>? AudioDataAvailable;
```
- Raised when new audio data is available for FFT processing
- Provides normalized float samples ready for spectral analysis

**Methods:**

```csharp
IReadOnlyList<AudioDevice> GetInputDevices();
```
- Returns all available audio input devices
- Uses NAudio's WaveIn enumeration

```csharp
IReadOnlyList<AudioDevice> GetOutputDevices();
```
- Returns all available audio output devices (for loopback)
- Uses NAudio's MMDeviceEnumerator (WASAPI)

```csharp
void SelectDevice(AudioDevice device);
```
- Selects an audio device for capture
- Must be called before StartCapture()

```csharp
void StartCapture();
```
- Starts audio capture from the selected device
- Automatically uses loopback for output devices
- Uses WaveIn for input devices

```csharp
void StopCapture();
```
- Stops audio capture and releases resources

**Properties:**
- `SelectedDevice`: Currently selected audio device (nullable)
- `IsCapturing`: Whether audio capture is currently active

### 4. Audio Service Implementation

#### AudioService (`Services/AudioService.cs`)

Complete implementation of the audio service using NAudio.

##### Device Enumeration

**Input Devices:**
- Uses `WaveIn.DeviceCount` and `WaveIn.GetCapabilities()`
- Enumerates microphones, line-in, and other input devices
- First device is marked as default

**Output Devices:**
- Uses `MMDeviceEnumerator` with WASAPI
- Enumerates active render endpoints
- Identifies the system default device

##### Audio Capture Modes

**Loopback Capture (Output Devices):**
- Uses `WasapiLoopbackCapture` for system audio
- Captures "what you hear" from speakers/headphones
- Perfect for music visualization from media players
- Automatically converts stereo to mono by averaging channels

**Input Capture (Input Devices):**
- Uses `WaveInEvent` for microphone/line-in
- Configurable format (defaults to 44.1kHz mono)
- Direct capture from hardware input

##### Audio Data Processing

**Format Conversion:**
- Converts byte buffers to normalized float samples
- Handles IEEE Float and 16-bit PCM formats
- Normalizes 16-bit samples to -1.0 to 1.0 range

**Stereo to Mono Conversion:**
- Averages multiple channels for mono output
- Essential for simplified FFT processing
- Maintains audio characteristics while reducing complexity

**Event Dispatching:**
- Raises `AudioDataAvailable` event with processed samples
- Includes sample rate and channel information
- Provides timestamp for synchronization

##### Error Handling

- Comprehensive try-catch blocks with logging
- Graceful handling of device errors
- Clean resource disposal on errors
- Detailed error messages for debugging

##### Resource Management

- Implements `IDisposable` pattern
- Properly unsubscribes from events
- Stops recording before disposal
- Releases NAudio resources correctly

### 5. Dependency Injection Registration

Updated `App.xaml.cs` to register the AudioService:

```csharp
services.AddSingleton<Services.IAudioService, Services.AudioService>();
```

The service is registered as a singleton to maintain state across the application lifecycle.

### 6. Unit Tests

Created comprehensive unit tests in `AudioServiceTests.cs`:

#### Constructor Tests
1. **Constructor_WithNullLogger_ThrowsArgumentNullException**
   - Verifies null checking for dependencies

2. **Constructor_WithValidLogger_LogsDebugMessage**
   - Verifies initialization logging

#### Device Enumeration Tests
3. **GetInputDevices_ReturnsDeviceList**
   - Verifies input device enumeration
   - Checks device properties and types

4. **GetOutputDevices_ReturnsDeviceList**
   - Verifies output device enumeration
   - Validates loopback-capable devices

#### Device Selection Tests
5. **SelectDevice_WithNullDevice_ThrowsArgumentNullException**
   - Verifies null checking

6. **SelectDevice_WithValidDevice_SetsSelectedDevice**
   - Verifies device selection logic

7. **SelectDevice_WithValidDevice_LogsInformation**
   - Verifies logging of device selection

#### Capture Tests
8. **StartCapture_WithoutSelectingDevice_ThrowsInvalidOperationException**
   - Verifies device must be selected first

9. **IsCapturing_InitiallyFalse**
   - Verifies initial state

10. **SelectedDevice_InitiallyNull**
    - Verifies initial state

11. **StopCapture_WhenNotCapturing_DoesNotThrow**
    - Verifies graceful handling

#### Model Tests
12. **AudioDevice_ToString_ReturnsName**
    - Verifies friendly string representation

13. **AudioDataEventArgs_Constructor_SetsProperties**
    - Verifies event args construction

14. **AudioDataEventArgs_Constructor_WithNullSamples_ThrowsArgumentNullException**
    - Verifies null checking

#### Disposal Tests
15. **Dispose_StopsCapture**
    - Verifies cleanup behavior

16. **Dispose_MultipleCalls_DoesNotThrow**
    - Verifies idempotent disposal

**Test Coverage:**
- Constructor validation
- Device enumeration
- Device selection
- Capture lifecycle
- Error handling
- Resource disposal

### 7. Usage Example

Here's how to use the AudioService in application code:

```csharp
// Inject the service
public class MyViewModel
{
    private readonly IAudioService _audioService;
    
    public MyViewModel(IAudioService audioService)
    {
        _audioService = audioService;
        _audioService.AudioDataAvailable += OnAudioDataAvailable;
    }
    
    public void EnumerateDevices()
    {
        // Get all output devices for loopback capture
        var outputDevices = _audioService.GetOutputDevices();
        
        // Get all input devices (microphones)
        var inputDevices = _audioService.GetInputDevices();
        
        // Display to user for selection...
    }
    
    public void StartListening(AudioDevice device)
    {
        // Select the device
        _audioService.SelectDevice(device);
        
        // Start capturing audio
        _audioService.StartCapture();
    }
    
    private void OnAudioDataAvailable(object? sender, AudioDataEventArgs e)
    {
        // e.Samples contains normalized float samples (-1.0 to 1.0)
        // e.SampleRate is the sample rate (e.g., 44100 Hz)
        // e.Channels is 1 for mono
        
        // Pass to FFT processor (Task 3)
        ProcessAudioForFFT(e.Samples, e.SampleRate);
    }
    
    public void StopListening()
    {
        _audioService.StopCapture();
    }
}
```

## Technical Details

### Audio Format

- **Sample Rate**: Typically 44100 Hz (CD quality) or 48000 Hz
- **Sample Format**: 32-bit IEEE Float (normalized -1.0 to 1.0)
- **Channels**: Mono (1 channel) for FFT processing
- **Buffer Size**: Determined by NAudio (typically ~100ms worth of samples)

### WASAPI Loopback

WASAPI (Windows Audio Session API) loopback capture is used for output devices:
- Captures the audio being played by the system
- Mix of all audio sources (media players, games, etc.)
- Zero latency impact on playback
- Same quality as the audio being rendered

### FFT Compatibility

The audio data format is optimized for FFT processing:
- Float samples are the standard format for FFT libraries
- Mono audio simplifies frequency analysis
- Sample rate is preserved for accurate frequency calculation
- Continuous streaming enables real-time analysis

## Architecture Decisions

### Why NAudio?

- **Windows Native**: Built specifically for Windows audio APIs
- **WASAPI Support**: Essential for loopback capture
- **Mature Library**: Well-tested and widely used
- **Flexible**: Supports multiple audio APIs (WaveIn, WASAPI, DirectSound)
- **Active Maintenance**: Regular updates and bug fixes

### Why Loopback Capture?

- **System Audio**: Captures music from any source (Spotify, YouTube, local files)
- **User Experience**: No need for virtual audio cables
- **Simplicity**: Works out-of-the-box on Windows
- **Quality**: Bit-perfect audio capture

### Why Float Samples?

- **FFT Standard**: Most FFT libraries expect float input
- **Precision**: Better than integer formats for signal processing
- **Normalization**: Standard -1.0 to 1.0 range simplifies processing
- **Efficiency**: No conversion needed before FFT

### Why Mono Conversion?

- **Simplicity**: Single FFT instead of stereo processing
- **Performance**: Half the processing required
- **Sufficient**: Frequency content is similar in both channels for music
- **Standard Practice**: Common in audio visualization applications

## Testing Notes

The unit tests build successfully but cannot run on Linux due to Windows-specific audio APIs. Tests must be run on Windows where:
- Audio devices are available
- WASAPI is supported
- WaveIn APIs function correctly

The GitHub Actions CI/CD pipeline runs on `windows-latest` and will execute all tests.

## Integration Points

### Task 3 - FFT Processor & Beat Detector

The AudioService provides the foundation for Task 3:
- `AudioDataAvailable` event supplies samples for FFT
- Float format is compatible with FFT libraries
- Sample rate information enables frequency calculation
- Real-time streaming enables live beat detection

### Task 7 - UI & Visualizer

The AudioService will integrate with the UI for:
- Device selection ComboBox/ListBox
- Start/Stop capture buttons
- Real-time audio level visualization
- Device status indicators

## Security & Privacy

### Microphone Access

When using input devices (microphones):
- Windows will show microphone permission dialogs
- User must grant permission for microphone access
- Privacy settings in Windows apply

### Loopback Capture

When using output devices (loopback):
- Captures system audio only
- No microphone involved
- No privacy prompts required
- User is aware audio is being visualized

## Performance Considerations

### CPU Usage

- Minimal overhead for audio capture
- NAudio uses efficient native APIs
- Event-based architecture prevents polling
- Proper resource disposal prevents leaks

### Memory Usage

- Audio buffers are reused
- Float arrays are temporary
- No long-term audio storage
- Events allow garbage collection

### Latency

- WASAPI provides low-latency capture (~10ms)
- Event delivery is near real-time
- Suitable for live music visualization
- No noticeable delay in reactive lighting

## Known Limitations

1. **Windows Only**: NAudio is Windows-specific (aligns with WPF requirement)
2. **Single Device**: Can only capture from one device at a time
3. **Mono Output**: Stereo information is lost (acceptable for visualization)
4. **No Recording**: Service doesn't save audio to disk (by design)

## Next Steps

This AudioService is now ready for:
- **Task 3**: FFT Processor will subscribe to AudioDataAvailable events
- **Task 3**: Beat Detector will analyze audio frequency data
- **Task 7**: UI will allow device selection and control
- **Task 7**: Visualizer will display real-time audio levels

## Files Created

### Main Project
- ✅ `src/LightJockey/Models/AudioDevice.cs` - Audio device model
- ✅ `src/LightJockey/Models/AudioDataEventArgs.cs` - Audio event arguments
- ✅ `src/LightJockey/Services/IAudioService.cs` - Audio service interface
- ✅ `src/LightJockey/Services/AudioService.cs` - Audio service implementation
- ✅ `src/LightJockey/App.xaml.cs` - Updated DI registration
- ✅ `src/LightJockey/LightJockey.csproj` - Added NAudio package

### Test Project
- ✅ `tests/LightJockey.Tests/Services/AudioServiceTests.cs` - Comprehensive unit tests

### Documentation
- ✅ `docs/tasks/Task2_AudioService.md` - This document

## References

- [NAudio Documentation](https://github.com/naudio/NAudio)
- [WASAPI Documentation](https://docs.microsoft.com/en-us/windows/win32/coreaudio/wasapi)
- [FFT Theory](https://en.wikipedia.org/wiki/Fast_Fourier_transform)
- [Audio Signal Processing](https://en.wikipedia.org/wiki/Audio_signal_processing)
