# Task 3 — FFTProcessor & BeatDetector

**Status**: ✅ Completed  
**PR**: Task3_FFT_BeatDetector  
**Date**: 2025-11-11

## Objective

Implement audio analysis capabilities including FFT (Fast Fourier Transform) processing, spectral analysis across frequency bands, and beat detection with BPM estimation.

## Implementation

### 1. NuGet Packages Added

The following package was added to the main project for FFT processing:

```xml
<PackageReference Include="MathNet.Numerics" Version="5.0.0" />
```

MathNet.Numerics provides:
- Fast Fourier Transform (FFT) algorithms
- Complex number support
- Windowing functions (Hann, Hamming, etc.)
- Signal processing utilities

### 2. Models Created

#### FFTResultEventArgs (`Models/FFTResultEventArgs.cs`)

Event arguments containing FFT analysis results.

**Properties:**
- `Spectrum` (double[]): FFT magnitude spectrum (absolute values)
- `FrequencyResolution` (double): Frequency resolution in Hz per bin
- `SampleRate` (int): Sample rate used for FFT
- `FFTSize` (int): FFT size (number of bins)
- `Timestamp` (DateTime): UTC timestamp when FFT was computed

The spectrum contains only the first half of the FFT output (positive frequencies) as the second half is symmetric for real-valued input.

#### SpectralDataEventArgs (`Models/SpectralDataEventArgs.cs`)

Event arguments containing spectral analysis data for frequency bands.

**Properties:**
- `LowFrequencyEnergy` (double): Energy in low frequency band (20-250 Hz)
- `MidFrequencyEnergy` (double): Energy in mid frequency band (250-2000 Hz)
- `HighFrequencyEnergy` (double): Energy in high frequency band (2000-20000 Hz)
- `TotalEnergy` (double): Total spectral energy across all bands
- `Timestamp` (DateTime): UTC timestamp when analysis was computed

Energy values are calculated as the average of squared magnitudes within each frequency band.

#### BeatDetectedEventArgs (`Models/BeatDetectedEventArgs.cs`)

Event arguments for beat detection events.

**Properties:**
- `Energy` (double): Detected energy level at the beat
- `BPM` (double): Current estimated BPM (beats per minute)
- `Confidence` (double): Confidence level of beat detection (0.0 to 1.0)
- `Timestamp` (DateTime): UTC timestamp when beat was detected

### 3. FFT Processor Service

#### IFFTProcessor (`Services/IFFTProcessor.cs`)

Service interface for performing FFT on audio data.

**Events:**
```csharp
event EventHandler<FFTResultEventArgs>? FFTResultAvailable;
```
- Raised when FFT results are computed
- Provides magnitude spectrum ready for analysis

**Methods:**

```csharp
void ProcessAudio(float[] samples, int sampleRate);
```
- Processes audio samples and computes FFT
- Applies Hann windowing to reduce spectral leakage
- Raises `FFTResultAvailable` event with results

```csharp
int GetBinIndex(double frequency, int sampleRate);
```
- Converts a frequency in Hz to the corresponding FFT bin index

```csharp
double GetFrequency(int binIndex, int sampleRate);
```
- Converts an FFT bin index to its corresponding frequency in Hz

**Properties:**
- `FFTSize` (int): The FFT size (number of samples processed)

#### FFTProcessor Implementation

**Key Features:**
- Configurable FFT size (must be power of 2, default 2048)
- Hann windowing to reduce spectral leakage
- Efficient FFT using MathNet.Numerics
- Magnitude spectrum calculation
- Thread-safe event handling

**Algorithm:**
1. Validates input samples (requires at least FFTSize samples)
2. Applies Hann window to samples
3. Converts to complex numbers
4. Performs FFT using MathNet.Numerics.IntegralTransforms.Fourier
5. Calculates magnitude spectrum
6. Raises event with results

**Example Usage:**
```csharp
var fftProcessor = serviceProvider.GetRequiredService<IFFTProcessor>();
fftProcessor.FFTResultAvailable += (s, e) => 
{
    Console.WriteLine($"FFT computed: {e.Spectrum.Length} bins, resolution: {e.FrequencyResolution} Hz/bin");
};

// Process audio samples
fftProcessor.ProcessAudio(audioSamples, 44100);
```

### 4. Spectral Analyzer Service

#### ISpectralAnalyzer (`Services/ISpectralAnalyzer.cs`)

Service interface for analyzing spectral content across frequency bands.

**Events:**
```csharp
event EventHandler<SpectralDataEventArgs>? SpectralDataAvailable;
```
- Raised when spectral analysis is complete
- Provides energy levels for low/mid/high frequency bands

**Methods:**

```csharp
void AnalyzeSpectrum(double[] spectrum, int sampleRate);
```
- Analyzes FFT spectrum and computes energy across frequency bands
- Can be called manually or automatically via FFT processor events

#### SpectralAnalyzer Implementation

**Frequency Bands:**
- **Low**: 20-250 Hz (bass, kick drum, bass guitar)
- **Mid**: 250-2000 Hz (vocals, guitars, piano)
- **High**: 2000-20000 Hz (cymbals, hi-hats, brightness)

**Key Features:**
- Automatically subscribes to FFT processor events
- Calculates average energy per frequency band
- Normalizes energy by number of bins
- Thread-safe event handling

**Algorithm:**
1. Determines bin indices for each frequency band
2. Sums squared magnitudes within each band
3. Normalizes by bin count to get average energy
4. Raises event with band energies

**Example Usage:**
```csharp
var spectralAnalyzer = serviceProvider.GetRequiredService<ISpectralAnalyzer>();
spectralAnalyzer.SpectralDataAvailable += (s, e) => 
{
    Console.WriteLine($"Low: {e.LowFrequencyEnergy:F2}, Mid: {e.MidFrequencyEnergy:F2}, High: {e.HighFrequencyEnergy:F2}");
    Console.WriteLine($"Total Energy: {e.TotalEnergy:F2}");
};
```

### 5. Beat Detector Service

#### IBeatDetector (`Services/IBeatDetector.cs`)

Service interface for detecting beats and estimating BPM.

**Events:**
```csharp
event EventHandler<BeatDetectedEventArgs>? BeatDetected;
```
- Raised when a beat is detected
- Provides beat energy, BPM estimate, and confidence

**Methods:**

```csharp
void ProcessEnergy(double energy);
```
- Processes audio energy value for beat detection
- Can be called manually or automatically via spectral analyzer events

```csharp
void Reset();
```
- Resets beat detector state (clears history and BPM)

**Properties:**
- `CurrentBPM` (double): Current estimated BPM

#### BeatDetector Implementation

**Detection Algorithm:**
- Energy-based onset detection
- Uses low frequency energy (bass) for beat detection
- Maintains energy history for dynamic threshold calculation
- Prevents false positives with minimum beat interval

**Key Features:**
- Configurable history window size (default 43 frames ~1 second at 43 FPS)
- Dynamic threshold: beat must be 1.5x average energy
- Minimum beat interval: 300ms (prevents detection above 200 BPM)
- BPM calculation from last 8 beat intervals
- Confidence score based on energy ratio

**Algorithm:**
1. Maintains rolling window of energy history
2. Calculates average energy from history
3. Detects beat when current energy > threshold * average
4. Ensures minimum time interval between beats
5. Updates BPM from recent beat timestamps
6. Calculates confidence from energy ratio

**Example Usage:**
```csharp
var beatDetector = serviceProvider.GetRequiredService<IBeatDetector>();
beatDetector.BeatDetected += (s, e) => 
{
    Console.WriteLine($"Beat! Energy: {e.Energy:F2}, BPM: {e.BPM:F1}, Confidence: {e.Confidence:F2}");
};

// Reset when changing songs
beatDetector.Reset();
```

### 6. Service Integration

All services are registered in the DI container (`App.xaml.cs`):

```csharp
// Register audio analysis services
services.AddSingleton<Services.IFFTProcessor, Services.FFTProcessor>();
services.AddSingleton<Services.ISpectralAnalyzer, Services.SpectralAnalyzer>();
services.AddSingleton<Services.IBeatDetector, Services.BeatDetector>();
```

**Service Dependencies:**
- `FFTProcessor` → standalone
- `SpectralAnalyzer` → depends on `IFFTProcessor`
- `BeatDetector` → depends on `ISpectralAnalyzer`

Services are automatically wired together via events:
1. Audio data → `FFTProcessor.ProcessAudio()`
2. FFT results → `SpectralAnalyzer` (via `FFTResultAvailable` event)
3. Spectral data → `BeatDetector` (via `SpectralDataAvailable` event)
4. Beats detected → Effect engine (via `BeatDetected` event)

### 7. Architecture Diagram

```
AudioService
    │
    │ AudioDataAvailable event
    │ (float[] samples)
    ↓
FFTProcessor
    │
    │ FFTResultAvailable event
    │ (double[] spectrum)
    ↓
SpectralAnalyzer
    │
    │ SpectralDataAvailable event
    │ (Low/Mid/High energy)
    ↓
BeatDetector
    │
    │ BeatDetected event
    │ (BPM, energy, confidence)
    ↓
EffectEngine (Task 6)
```

### 8. Signal Processing Flow

```
1. Raw Audio (PCM samples)
   ↓
2. Hann Window Application
   ↓
3. FFT Transform
   ↓
4. Magnitude Spectrum Calculation
   ↓
5. Frequency Band Analysis
   ↓
6. Energy-Based Beat Detection
   ↓
7. BPM Estimation
```

### 9. Unit Tests

Comprehensive test coverage with 41 total tests:

#### FFTProcessorTests (16 tests)
- Constructor validation (null parameters, invalid FFT sizes)
- FFT size property validation
- Audio processing with various inputs
- Sine wave frequency detection
- Bin index and frequency conversion
- Disposal behavior

#### SpectralAnalyzerTests (11 tests)
- Constructor validation
- Event subscription/unsubscription
- Frequency band energy calculation
- Different frequency content detection (low/mid/high)
- Total energy calculation
- Disposal behavior

#### BeatDetectorTests (14 tests)
- Constructor validation
- Beat detection with energy spikes
- BPM calculation from multiple beats
- Confidence level validation
- Minimum beat interval enforcement
- Silence false positive prevention
- Integration test (FFT → Spectral → Beat)
- Reset functionality
- Disposal behavior

**Test Coverage:**
- Edge cases (null, empty, invalid inputs)
- Core functionality (FFT accuracy, band detection, beat detection)
- Integration scenarios (end-to-end audio to beat)
- Resource management (disposal, event unsubscription)

### 10. Performance Considerations

**FFT Processing:**
- FFT size of 2048 samples @ 44100 Hz = ~46 ms of audio
- Processing time: < 5 ms on modern hardware
- Can process 43+ frames per second (real-time capable)

**Memory Usage:**
- FFT: ~16 KB per frame (2048 samples × 8 bytes)
- Spectrum: ~8 KB per frame (1024 bins × 8 bytes)
- Beat detector history: < 1 KB

**Optimization Strategies:**
- Services are singletons (created once)
- Reuses buffers where possible
- Event-based architecture (no polling)
- Efficient algorithms (O(n log n) FFT)

### 11. Integration with EffectEngine (Task 6)

The audio analysis services provide events that the EffectEngine can subscribe to:

**BeatDetected Event:**
- Triggers flash/strobe effects
- Synchronizes color changes
- Modulates intensity

**SpectralDataAvailable Event:**
- Maps frequency bands to colors
  - Low (bass) → Red
  - Mid (vocals) → Green
  - High (treble) → Blue
- Adjusts brightness based on energy

**Example Effect Engine Integration:**
```csharp
// In EffectEngine constructor
_beatDetector.BeatDetected += OnBeatDetected;
_spectralAnalyzer.SpectralDataAvailable += OnSpectralData;

private void OnBeatDetected(object? sender, BeatDetectedEventArgs e)
{
    // Flash lights on beat
    if (e.Confidence > 0.7)
    {
        TriggerBeatEffect(e.Energy, e.BPM);
    }
}

private void OnSpectralData(object? sender, SpectralDataEventArgs e)
{
    // Map frequency bands to RGB
    var red = (byte)(e.LowFrequencyEnergy * 255);
    var green = (byte)(e.MidFrequencyEnergy * 255);
    var blue = (byte)(e.HighFrequencyEnergy * 255);
    
    UpdateLightColor(red, green, blue);
}
```

## Usage Examples

### Basic Setup

```csharp
// Get services from DI container
var audioService = serviceProvider.GetRequiredService<IAudioService>();
var fftProcessor = serviceProvider.GetRequiredService<IFFTProcessor>();
var spectralAnalyzer = serviceProvider.GetRequiredService<ISpectralAnalyzer>();
var beatDetector = serviceProvider.GetRequiredService<IBeatDetector>();

// Subscribe to events
audioService.AudioDataAvailable += (s, e) => 
    fftProcessor.ProcessAudio(e.Samples, e.SampleRate);

beatDetector.BeatDetected += (s, e) => 
    Console.WriteLine($"Beat at {e.BPM:F1} BPM");

// Start capturing
audioService.SelectDevice(outputDevice);
audioService.StartCapture();
```

### Manual Processing

```csharp
// Process pre-recorded audio
var samples = LoadAudioFile("song.wav");
var fftProcessor = new FFTProcessor(logger, 2048);

fftProcessor.FFTResultAvailable += (s, e) =>
{
    // Find dominant frequency
    int peakBin = e.Spectrum
        .Select((value, index) => new { value, index })
        .OrderByDescending(x => x.value)
        .First().index;
    
    double dominantFreq = fftProcessor.GetFrequency(peakBin, 44100);
    Console.WriteLine($"Dominant frequency: {dominantFreq:F1} Hz");
};

fftProcessor.ProcessAudio(samples, 44100);
```

## Testing

Run unit tests:
```bash
dotnet test --filter FullyQualifiedName~FFTProcessor
dotnet test --filter FullyQualifiedName~SpectralAnalyzer
dotnet test --filter FullyQualifiedName~BeatDetector
```

All tests pass on Windows with .NET 9.0.

## Next Steps

- **Task 4**: Implement HueService for Philips Hue control
- **Task 6**: Create EffectEngine that uses beat detection and spectral data
- **Task 7**: Add audio visualizer UI component

## References

- [MathNet.Numerics Documentation](https://numerics.mathdotnet.com/)
- [FFT Windowing Functions](https://en.wikipedia.org/wiki/Window_function)
- [Beat Detection Algorithms](https://en.wikipedia.org/wiki/Beat_detection)
- Task 2 documentation: [Task2_AudioService.md](Task2_AudioService.md)

## Troubleshooting

**Issue**: No beats detected
- Check that audio is playing and has percussive content
- Verify low frequency energy is present (bass/kick drum)
- Adjust beat threshold if needed

**Issue**: Too many false beat detections
- Increase minimum beat interval
- Increase threshold multiplier in BeatDetector
- Ensure audio input has good signal-to-noise ratio

**Issue**: Incorrect BPM estimation
- Need more beats for accurate calculation (minimum 2-3 beats)
- BPM will stabilize after 8 beats
- Check for double-counting (halve the reported BPM)
