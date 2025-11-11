using LightJockey.Models;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace LightJockey.Services;

/// <summary>
/// Implementation of audio device management and capture service using NAudio
/// </summary>
public class AudioService : IAudioService
{
    private readonly ILogger<AudioService> _logger;
    private AudioDevice? _selectedDevice;
    private IWaveIn? _waveIn;
    private WasapiLoopbackCapture? _loopbackCapture;
    private bool _isCapturing;
    private bool _disposed;

    /// <inheritdoc/>
    public event EventHandler<AudioDataEventArgs>? AudioDataAvailable;

    /// <inheritdoc/>
    public AudioDevice? SelectedDevice => _selectedDevice;

    /// <inheritdoc/>
    public bool IsCapturing => _isCapturing;

    /// <summary>
    /// Initializes a new instance of the AudioService class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public AudioService(ILogger<AudioService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("AudioService initialized");
    }

    /// <inheritdoc/>
    public IReadOnlyList<AudioDevice> GetInputDevices()
    {
        try
        {
            _logger.LogDebug("Enumerating audio input devices");
            var devices = new List<AudioDevice>();

            // Use WaveIn to enumerate input devices
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var capabilities = WaveIn.GetCapabilities(i);
                devices.Add(new AudioDevice
                {
                    Id = i.ToString(),
                    Name = capabilities.ProductName,
                    DeviceType = AudioDeviceType.Input,
                    IsDefault = i == 0
                });
            }

            _logger.LogInformation("Found {DeviceCount} audio input devices", devices.Count);
            return devices.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enumerating audio input devices");
            throw;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<AudioDevice> GetOutputDevices()
    {
        try
        {
            _logger.LogDebug("Enumerating audio output devices");
            var devices = new List<AudioDevice>();

            using (var enumerator = new MMDeviceEnumerator())
            {
                var outputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                foreach (var device in outputDevices)
                {
                    devices.Add(new AudioDevice
                    {
                        Id = device.ID,
                        Name = device.FriendlyName,
                        DeviceType = AudioDeviceType.Output,
                        IsDefault = device.ID == defaultDevice.ID
                    });
                }
            }

            _logger.LogInformation("Found {DeviceCount} audio output devices", devices.Count);
            return devices.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enumerating audio output devices");
            throw;
        }
    }

    /// <inheritdoc/>
    public void SelectDevice(AudioDevice device)
    {
        if (device == null)
        {
            throw new ArgumentNullException(nameof(device));
        }

        if (_isCapturing)
        {
            _logger.LogWarning("Cannot select device while capture is active");
            throw new InvalidOperationException("Cannot select device while capture is active. Stop capture first.");
        }

        _selectedDevice = device;
        _logger.LogInformation("Selected audio device: {DeviceName} (Type: {DeviceType})", 
            device.Name, device.DeviceType);
    }

    /// <inheritdoc/>
    public void StartCapture()
    {
        if (_selectedDevice == null)
        {
            _logger.LogWarning("Attempted to start capture without selecting a device");
            throw new InvalidOperationException("No device selected. Call SelectDevice first.");
        }

        if (_isCapturing)
        {
            _logger.LogWarning("Capture already active");
            return;
        }

        try
        {
            if (_selectedDevice.DeviceType == AudioDeviceType.Output)
            {
                StartLoopbackCapture();
            }
            else
            {
                StartInputCapture();
            }

            _isCapturing = true;
            _logger.LogInformation("Audio capture started for device: {DeviceName}", _selectedDevice.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting audio capture");
            CleanupCaptureResources();
            throw;
        }
    }

    private void StartLoopbackCapture()
    {
        using (var enumerator = new MMDeviceEnumerator())
        {
            var device = enumerator.GetDevice(_selectedDevice!.Id);
            _loopbackCapture = new WasapiLoopbackCapture(device);
            _loopbackCapture.DataAvailable += OnLoopbackDataAvailable;
            _loopbackCapture.RecordingStopped += OnRecordingStopped;
            _loopbackCapture.StartRecording();
        }
    }

    private void StartInputCapture()
    {
        if (!int.TryParse(_selectedDevice!.Id, out var deviceNumber))
        {
            _logger.LogError("Invalid device ID: {DeviceId}", _selectedDevice.Id);
            throw new InvalidOperationException($"Invalid device ID: {_selectedDevice.Id}");
        }

        _waveIn = new WaveInEvent
        {
            DeviceNumber = deviceNumber,
            WaveFormat = new WaveFormat(44100, 1) // 44.1kHz, mono
        };
        _waveIn.DataAvailable += OnWaveInDataAvailable;
        _waveIn.RecordingStopped += OnRecordingStopped;
        _waveIn.StartRecording();
    }

    private void OnLoopbackDataAvailable(object? sender, WaveInEventArgs e)
    {
        try
        {
            var capture = sender as WasapiLoopbackCapture;
            if (capture == null) return;

            var waveFormat = capture.WaveFormat;
            var samples = ConvertBytesToFloatSamples(e.Buffer, e.BytesRecorded, waveFormat);

            // Convert stereo to mono if needed by averaging channels
            var monoSamples = waveFormat.Channels > 1 
                ? ConvertToMono(samples, waveFormat.Channels) 
                : samples;

            var eventArgs = new AudioDataEventArgs(monoSamples, waveFormat.SampleRate, 1);
            AudioDataAvailable?.Invoke(this, eventArgs);
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex, "Unsupported audio format in loopback capture");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing loopback audio data");
        }
    }

    private void OnWaveInDataAvailable(object? sender, WaveInEventArgs e)
    {
        try
        {
            var waveIn = sender as IWaveIn;
            if (waveIn == null) return;

            var waveFormat = waveIn.WaveFormat;
            var samples = ConvertBytesToFloatSamples(e.Buffer, e.BytesRecorded, waveFormat);

            var eventArgs = new AudioDataEventArgs(samples, waveFormat.SampleRate, waveFormat.Channels);
            AudioDataAvailable?.Invoke(this, eventArgs);
        }
        catch (NotSupportedException ex)
        {
            _logger.LogError(ex, "Unsupported audio format in input capture");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing input audio data");
        }
    }

    private float[] ConvertBytesToFloatSamples(byte[] buffer, int bytesRecorded, WaveFormat waveFormat)
    {
        var sampleCount = bytesRecorded / (waveFormat.BitsPerSample / 8);
        var samples = new float[sampleCount];

        if (waveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = BitConverter.ToSingle(buffer, i * 4);
            }
        }
        else if (waveFormat.BitsPerSample == 16)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                short sample = BitConverter.ToInt16(buffer, i * 2);
                samples[i] = sample / 32768f;
            }
        }
        else
        {
            _logger.LogWarning("Unsupported audio format: Encoding={Encoding}, BitsPerSample={BitsPerSample}", 
                waveFormat.Encoding, waveFormat.BitsPerSample);
            throw new NotSupportedException($"Unsupported audio format: Encoding={waveFormat.Encoding}, BitsPerSample={waveFormat.BitsPerSample}");
        }

        return samples;
    }

    private float[] ConvertToMono(float[] samples, int channels)
    {
        var monoSamples = new float[samples.Length / channels];
        for (int i = 0; i < monoSamples.Length; i++)
        {
            float sum = 0;
            for (int c = 0; c < channels; c++)
            {
                sum += samples[i * channels + c];
            }
            monoSamples[i] = sum / channels;
        }
        return monoSamples;
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            _logger.LogError(e.Exception, "Recording stopped due to error");
        }
        else
        {
            _logger.LogDebug("Recording stopped normally");
        }
    }

    /// <inheritdoc/>
    public void StopCapture()
    {
        if (!_isCapturing)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Stopping audio capture");
            CleanupCaptureResources();
            _isCapturing = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping audio capture");
            throw;
        }
    }

    private void CleanupCaptureResources()
    {
        if (_loopbackCapture != null)
        {
            _loopbackCapture.DataAvailable -= OnLoopbackDataAvailable;
            _loopbackCapture.RecordingStopped -= OnRecordingStopped;
            _loopbackCapture.StopRecording();
            _loopbackCapture.Dispose();
            _loopbackCapture = null;
        }

        if (_waveIn != null)
        {
            _waveIn.DataAvailable -= OnWaveInDataAvailable;
            _waveIn.RecordingStopped -= OnRecordingStopped;
            _waveIn.StopRecording();
            _waveIn.Dispose();
            _waveIn = null;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            StopCapture();
            _disposed = true;
            _logger.LogDebug("AudioService disposed");
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogWarning(ex, "Object already disposed during AudioService.Dispose()");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation during AudioService.Dispose()");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error disposing AudioService");
        }

        GC.SuppressFinalize(this);
    }
}
