using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services;

/// <summary>
/// Unit tests for AudioService demonstrating device enumeration, selection, and streaming
/// </summary>
public class AudioServiceTests : IDisposable
{
    private readonly Mock<ILogger<AudioService>> _mockLogger;
    private readonly AudioService _service;

    public AudioServiceTests()
    {
        _mockLogger = new Mock<ILogger<AudioService>>();
        _service = new AudioService(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new AudioService(null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidLogger_LogsDebugMessage()
    {
        // Arrange & Act
        var logger = new Mock<ILogger<AudioService>>();
        using var service = new AudioService(logger.Object);

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("initialized")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetInputDevices_ReturnsDeviceList()
    {
        // Act
        var devices = _service.GetInputDevices();

        // Assert
        Assert.NotNull(devices);
        // Note: Device count may be 0 on systems without audio input devices
        Assert.IsAssignableFrom<IReadOnlyList<AudioDevice>>(devices);
        
        foreach (var device in devices)
        {
            Assert.NotNull(device.Id);
            Assert.NotNull(device.Name);
            Assert.Equal(AudioDeviceType.Input, device.DeviceType);
        }
    }

    [Fact]
    public void GetOutputDevices_ReturnsDeviceList()
    {
        // Act
        var devices = _service.GetOutputDevices();

        // Assert
        Assert.NotNull(devices);
        // Note: Device count may be 0 on systems without audio output devices
        Assert.IsAssignableFrom<IReadOnlyList<AudioDevice>>(devices);
        
        foreach (var device in devices)
        {
            Assert.NotNull(device.Id);
            Assert.NotNull(device.Name);
            Assert.Equal(AudioDeviceType.Output, device.DeviceType);
        }
    }

    [Fact]
    public void SelectDevice_WithNullDevice_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => _service.SelectDevice(null!));
        Assert.Equal("device", exception.ParamName);
    }

    [Fact]
    public void SelectDevice_WithValidDevice_SetsSelectedDevice()
    {
        // Arrange
        var device = new AudioDevice
        {
            Id = "test-device",
            Name = "Test Audio Device",
            DeviceType = AudioDeviceType.Input,
            IsDefault = false
        };

        // Act
        _service.SelectDevice(device);

        // Assert
        Assert.NotNull(_service.SelectedDevice);
        Assert.Equal(device.Id, _service.SelectedDevice.Id);
        Assert.Equal(device.Name, _service.SelectedDevice.Name);
    }

    [Fact]
    public void SelectDevice_WithValidDevice_LogsInformation()
    {
        // Arrange
        var device = new AudioDevice
        {
            Id = "test-device",
            Name = "Test Audio Device",
            DeviceType = AudioDeviceType.Input,
            IsDefault = false
        };

        // Act
        _service.SelectDevice(device);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Selected audio device")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void StartCapture_WithoutSelectingDevice_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _service.StartCapture());
        Assert.Contains("No device selected", exception.Message);
    }

    [Fact]
    public void IsCapturing_InitiallyFalse()
    {
        // Assert
        Assert.False(_service.IsCapturing);
    }

    [Fact]
    public void SelectedDevice_InitiallyNull()
    {
        // Assert
        Assert.Null(_service.SelectedDevice);
    }

    [Fact]
    public void StopCapture_WhenNotCapturing_DoesNotThrow()
    {
        // Act & Assert - should not throw
        _service.StopCapture();
        
        // Verify it's still not capturing
        Assert.False(_service.IsCapturing);
    }

    [Fact]
    public void AudioDevice_ToString_ReturnsName()
    {
        // Arrange
        var device = new AudioDevice
        {
            Id = "test-id",
            Name = "Test Device Name",
            DeviceType = AudioDeviceType.Input,
            IsDefault = false
        };

        // Act
        var result = device.ToString();

        // Assert
        Assert.Equal("Test Device Name", result);
    }

    [Fact]
    public void AudioDataEventArgs_Constructor_SetsProperties()
    {
        // Arrange
        var samples = new float[] { 0.1f, 0.2f, 0.3f };
        var sampleRate = 44100;
        var channels = 1;

        // Act
        var eventArgs = new AudioDataEventArgs(samples, sampleRate, channels);

        // Assert
        Assert.Same(samples, eventArgs.Samples);
        Assert.Equal(sampleRate, eventArgs.SampleRate);
        Assert.Equal(channels, eventArgs.Channels);
        Assert.True((DateTime.UtcNow - eventArgs.Timestamp).TotalSeconds < 1);
    }

    [Fact]
    public void AudioDataEventArgs_Constructor_WithNullSamples_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new AudioDataEventArgs(null!, 44100, 1));
        Assert.Equal("samples", exception.ParamName);
    }

    [Fact]
    public void Dispose_StopsCapture()
    {
        // Act
        _service.Dispose();

        // Assert - verify not capturing after dispose
        Assert.False(_service.IsCapturing);
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        // Act & Assert - should not throw on multiple dispose calls
        _service.Dispose();
        _service.Dispose();
    }

    [Fact]
    public void StartCapture_WithInvalidInputDeviceId_ThrowsInvalidOperationException()
    {
        // Arrange
        var device = new AudioDevice
        {
            Id = "invalid-id",
            Name = "Test Device",
            DeviceType = AudioDeviceType.Input,
            IsDefault = false
        };

        // Act
        _service.SelectDevice(device);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _service.StartCapture());
        Assert.Contains("Invalid device ID", exception.Message);
    }

    public void Dispose()
    {
        _service?.Dispose();
    }
}
