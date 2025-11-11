using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services;

/// <summary>
/// Unit tests for EntertainmentService demonstrating Entertainment V2 streaming functionality
/// </summary>
public class EntertainmentServiceTests : IDisposable
{
    private readonly Mock<ILogger<EntertainmentService>> _mockLogger;
    private readonly Mock<IAudioService> _mockAudioService;
    private readonly EntertainmentService _service;

    public EntertainmentServiceTests()
    {
        _mockLogger = new Mock<ILogger<EntertainmentService>>();
        _mockAudioService = new Mock<IAudioService>();
        _service = new EntertainmentService(_mockLogger.Object, _mockAudioService.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var audioService = new Mock<IAudioService>().Object;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EntertainmentService(null!, audioService));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullAudioService_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = new Mock<ILogger<EntertainmentService>>().Object;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EntertainmentService(logger, null!));
        Assert.Equal("audioService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidArguments_LogsDebugMessage()
    {
        // Arrange & Act
        var logger = new Mock<ILogger<EntertainmentService>>();
        var audioService = new Mock<IAudioService>();
        using var service = new EntertainmentService(logger.Object, audioService.Object);

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
    public void IsStreaming_InitiallyFalse()
    {
        // Assert
        Assert.False(_service.IsStreaming);
    }

    [Fact]
    public void ActiveArea_InitiallyNull()
    {
        // Assert
        Assert.Null(_service.ActiveArea);
    }

    [Fact]
    public void Configuration_InitiallyNull()
    {
        // Assert
        Assert.Null(_service.Configuration);
    }

    [Fact]
    public void CurrentFrameRate_InitiallyZero()
    {
        // Assert
        Assert.Equal(0.0, _service.CurrentFrameRate);
    }

    [Fact]
    public async Task StartStreamingAsync_WithoutInitialization_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new LightJockeyEntertainmentConfig();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.StartStreamingAsync(config));
        Assert.Contains("not initialized", exception.Message);
    }

    [Fact]
    public async Task StopStreamingAsync_WhenNotStreaming_DoesNotThrow()
    {
        // Act
        await _service.StopStreamingAsync();

        // Assert - Should complete without exception
        Assert.False(_service.IsStreaming);
    }

    [Fact]
    public void UpdateChannel_WithoutInitialization_DoesNotThrow()
    {
        // Arrange
        var color = new HueColor(255, 0, 0);

        // Act - Should not throw even when not initialized
        _service.UpdateChannel(0, color, 1.0);

        // Assert - No exception expected
        Assert.False(_service.IsStreaming);
    }

    [Fact]
    public void UpdateChannels_WithoutInitialization_DoesNotThrow()
    {
        // Arrange
        var channels = new List<LightChannel>
        {
            new LightChannel { Index = 0, LightId = Guid.NewGuid(), Color = new HueColor(255, 0, 0), Brightness = 1.0 }
        };

        // Act - Should not throw even when not initialized
        _service.UpdateChannels(channels);

        // Assert - No exception expected
        Assert.False(_service.IsStreaming);
    }

    [Fact]
    public void StreamingStarted_Event_CanBeSubscribed()
    {
        // Arrange
        var eventRaised = false;
        _service.StreamingStarted += (sender, args) => eventRaised = true;

        // Act
        // Event will be raised when streaming actually starts
        // This test just verifies the event can be subscribed

        // Assert
        Assert.False(eventRaised); // Not raised yet
    }

    [Fact]
    public void StreamingStopped_Event_CanBeSubscribed()
    {
        // Arrange
        var eventRaised = false;
        _service.StreamingStopped += (sender, args) => eventRaised = true;

        // Act
        // Event will be raised when streaming actually stops
        // This test just verifies the event can be subscribed

        // Assert
        Assert.False(eventRaised); // Not raised yet
    }

    [Fact]
    public void StreamingError_Event_CanBeSubscribed()
    {
        // Arrange
        var eventRaised = false;
        string? errorMessage = null;
        _service.StreamingError += (sender, msg) =>
        {
            eventRaised = true;
            errorMessage = msg;
        };

        // Act
        // Event will be raised when an error occurs
        // This test just verifies the event can be subscribed

        // Assert
        Assert.False(eventRaised); // Not raised yet
        Assert.Null(errorMessage);
    }

    [Fact]
    public void Dispose_MultipleTimesSafe()
    {
        // Act - Call dispose multiple times
        _service.Dispose();
        _service.Dispose();

        // Assert - Should not throw
        Assert.False(_service.IsStreaming);
    }

    [Fact]
    public void Dispose_LogsDebugMessage()
    {
        // Act
        _service.Dispose();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("disposed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public void Dispose()
    {
        _service.Dispose();
    }
}

/// <summary>
/// Unit tests for Entertainment models
/// </summary>
public class EntertainmentModelsTests
{
    [Fact]
    public void EntertainmentArea_Properties_SetCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Area";
        var lightIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }.AsReadOnly();

        // Act
        var area = new EntertainmentArea
        {
            Id = id,
            Name = name,
            IsActive = true,
            LightIds = lightIds
        };

        // Assert
        Assert.Equal(id, area.Id);
        Assert.Equal(name, area.Name);
        Assert.True(area.IsActive);
        Assert.Equal(lightIds, area.LightIds);
        Assert.Equal(2, area.ChannelCount);
    }

    [Fact]
    public void EntertainmentArea_DefaultLightIds_EmptyList()
    {
        // Act
        var area = new EntertainmentArea
        {
            Id = Guid.NewGuid(),
            Name = "Test Area"
        };

        // Assert
        Assert.Empty(area.LightIds);
        Assert.Equal(0, area.ChannelCount);
    }

    [Fact]
    public void LightChannel_Properties_SetCorrectly()
    {
        // Arrange
        byte index = 5;
        var lightId = Guid.NewGuid();
        var color = new HueColor(255, 128, 64);
        double brightness = 0.75;

        // Act
        var channel = new LightChannel
        {
            Index = index,
            LightId = lightId,
            Color = color,
            Brightness = brightness
        };

        // Assert
        Assert.Equal(index, channel.Index);
        Assert.Equal(lightId, channel.LightId);
        Assert.Equal(color, channel.Color);
        Assert.Equal(brightness, channel.Brightness);
    }

    [Fact]
    public void LightChannel_DefaultColor_Black()
    {
        // Act
        var channel = new LightChannel
        {
            Index = 0,
            LightId = Guid.NewGuid()
        };

        // Assert
        Assert.Equal(0, channel.Color.Red);
        Assert.Equal(0, channel.Color.Green);
        Assert.Equal(0, channel.Color.Blue);
    }

    [Fact]
    public void LightChannel_DefaultBrightness_FullBrightness()
    {
        // Act
        var channel = new LightChannel
        {
            Index = 0,
            LightId = Guid.NewGuid()
        };

        // Assert
        Assert.Equal(1.0, channel.Brightness);
    }

    [Fact]
    public void LightJockeyEntertainmentConfig_DefaultValues_Correct()
    {
        // Act
        var config = new LightJockeyEntertainmentConfig();

        // Assert
        Assert.Equal(25, config.TargetFrameRate);
        Assert.False(config.UseColorLoop);
        Assert.Equal(0.5, config.ColorLoopSpeed);
        Assert.True(config.AudioReactive);
        Assert.Equal(0.5, config.AudioSensitivity);
        Assert.Equal(0.1, config.MinBrightness);
        Assert.Equal(1.0, config.MaxBrightness);
    }

    [Fact]
    public void LightJockeyEntertainmentConfig_CustomValues_SetCorrectly()
    {
        // Act
        var config = new LightJockeyEntertainmentConfig
        {
            TargetFrameRate = 60,
            UseColorLoop = true,
            ColorLoopSpeed = 0.8,
            AudioReactive = false,
            AudioSensitivity = 0.7,
            MinBrightness = 0.2,
            MaxBrightness = 0.9
        };

        // Assert
        Assert.Equal(60, config.TargetFrameRate);
        Assert.True(config.UseColorLoop);
        Assert.Equal(0.8, config.ColorLoopSpeed);
        Assert.False(config.AudioReactive);
        Assert.Equal(0.7, config.AudioSensitivity);
        Assert.Equal(0.2, config.MinBrightness);
        Assert.Equal(0.9, config.MaxBrightness);
    }
}
