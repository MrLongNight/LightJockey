using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services;

/// <summary>
/// Unit tests for HueService demonstrating bridge discovery, authentication, and light control
/// </summary>
public class HueServiceTests : IDisposable
{
    private readonly Mock<ILogger<HueService>> _mockLogger;
    private readonly Mock<IConfigurationService> _mockConfigurationService;
    private readonly HueService _service;

    public HueServiceTests()
    {
        _mockLogger = new Mock<ILogger<HueService>>();
        _mockConfigurationService = new Mock<IConfigurationService>();
        _service = new HueService(_mockLogger.Object, _mockConfigurationService.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new HueService(null!, _mockConfigurationService.Object));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullConfigurationService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new HueService(_mockLogger.Object, null!));
        Assert.Equal("configurationService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidLogger_LogsDebugMessage()
    {
        // Arrange & Act
        var logger = new Mock<ILogger<HueService>>();
        using var service = new HueService(logger.Object, _mockConfigurationService.Object);

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
    public void IsConnected_InitiallyFalse()
    {
        // Assert
        Assert.False(_service.IsConnected);
    }

    [Fact]
    public void ConnectedBridge_InitiallyNull()
    {
        // Assert
        Assert.Null(_service.ConnectedBridge);
    }

    [Fact]
    public async Task DiscoverBridgesAsync_ReturnsListOfBridges()
    {
        // Act
        var bridges = await _service.DiscoverBridgesAsync();

        // Assert
        Assert.NotNull(bridges);
        Assert.IsAssignableFrom<IReadOnlyList<HueBridge>>(bridges);
        
        // Note: Actual bridge count depends on network availability
        // The test verifies the method executes without errors
        foreach (var bridge in bridges)
        {
            Assert.NotNull(bridge.IpAddress);
            Assert.NotNull(bridge.BridgeId);
        }
    }

    [Fact]
    public async Task RegisterAsync_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.RegisterAsync(null!, "TestApp", "TestDevice"));
        Assert.Equal("bridge", exception.ParamName);
    }

    [Fact]
    public async Task RegisterAsync_WithNullAppName_ThrowsArgumentException()
    {
        // Arrange
        var bridge = new HueBridge { IpAddress = "192.168.1.1", BridgeId = "test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.RegisterAsync(bridge, null!, "TestDevice"));
    }

    [Fact]
    public async Task RegisterAsync_WithNullDeviceName_ThrowsArgumentException()
    {
        // Arrange
        var bridge = new HueBridge { IpAddress = "192.168.1.1", BridgeId = "test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.RegisterAsync(bridge, "TestApp", null!));
    }

    [Fact]
    public async Task ConnectAsync_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.ConnectAsync(null!, "test-key"));
        Assert.Equal("bridge", exception.ParamName);
    }

    [Fact]
    public async Task ConnectAsync_WithNoAppKeyAndNoStoredKey_ReturnsFalse()
    {
        // Arrange
        var bridge = new HueBridge { IpAddress = "192.168.1.1", BridgeId = "test" };
        _mockConfigurationService
            .Setup(s => s.GetSecureValueAsync(It.IsAny<string>()))
            .ReturnsAsync((string)null);

        // Act
        var result = await _service.ConnectAsync(bridge);

        // Assert
        Assert.False(result);
        _mockConfigurationService.Verify(s => s.GetSecureValueAsync("HueAppKey"), Times.Once);
    }

    [Fact]
    public async Task GetLightsAsync_WhenNotConnected_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetLightsAsync());
        Assert.Contains("not connected", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SetLightOnOffAsync_WithNullLightId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.SetLightOnOffAsync(null!, true));
    }

    [Fact]
    public async Task SetLightOnOffAsync_WhenNotConnected_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SetLightOnOffAsync("test-id", true));
        Assert.Contains("not connected", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SetLightBrightnessAsync_WithNullLightId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.SetLightBrightnessAsync(null!, 128));
    }

    [Fact]
    public async Task SetLightBrightnessAsync_WhenNotConnected_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SetLightBrightnessAsync("test-id", 128));
        Assert.Contains("not connected", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SetLightColorAsync_WithNullLightId_ThrowsArgumentException()
    {
        // Arrange
        var color = new HueColor(255, 0, 0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.SetLightColorAsync(null!, color));
    }

    [Fact]
    public async Task SetLightColorAsync_WithNullColor_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.SetLightColorAsync("test-id", null!));
        Assert.Equal("color", exception.ParamName);
    }

    [Fact]
    public async Task SetLightColorAsync_WhenNotConnected_ThrowsInvalidOperationException()
    {
        // Arrange
        var color = new HueColor(255, 0, 0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SetLightColorAsync("test-id", color));
        Assert.Contains("not connected", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ConnectAsync_WithStoredAppKey_UsesStoredKey()
    {
        // Arrange
        var bridge = new HueBridge { IpAddress = "192.168.1.1", BridgeId = "test" };
        const string storedKey = "my-stored-app-key";
        _mockConfigurationService
            .Setup(s => s.GetSecureValueAsync("HueAppKey"))
            .ReturnsAsync(storedKey);

        // This test will fail to connect because we are not mocking the Hue API,
        // but we can verify that it attempted to connect by checking the log.
        // A more advanced test would mock the LocalHueApi.

        // Act
        await _service.ConnectAsync(bridge);

        // Assert
        _mockConfigurationService.Verify(s => s.GetSecureValueAsync("HueAppKey"), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrieved AppKey from secure storage")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var service = new HueService(_mockLogger.Object, _mockConfigurationService.Object);

        // Act & Assert - should not throw
        service.Dispose();
        service.Dispose();
    }

    public void Dispose()
    {
        _service?.Dispose();
    }
}

/// <summary>
/// Unit tests for HueColor model
/// </summary>
public class HueColorTests
{
    [Fact]
    public void Constructor_Default_InitializesWithZeroValues()
    {
        // Act
        var color = new HueColor();

        // Assert
        Assert.Equal(0, color.Red);
        Assert.Equal(0, color.Green);
        Assert.Equal(0, color.Blue);
    }

    [Fact]
    public void Constructor_WithRgbValues_SetsProperties()
    {
        // Act
        var color = new HueColor(255, 128, 64);

        // Assert
        Assert.Equal(255, color.Red);
        Assert.Equal(128, color.Green);
        Assert.Equal(64, color.Blue);
    }

    [Fact]
    public void ToHexString_ConvertsCorrectly()
    {
        // Arrange
        var color = new HueColor(255, 0, 0);

        // Act
        var hex = color.ToHexString();

        // Assert
        Assert.Equal("FF0000", hex);
    }

    [Theory]
    [InlineData(0, 0, 0, "000000")]
    [InlineData(255, 255, 255, "FFFFFF")]
    [InlineData(255, 128, 0, "FF8000")]
    [InlineData(128, 64, 32, "804020")]
    public void ToHexString_VariousColors_ConvertsCorrectly(byte r, byte g, byte b, string expectedHex)
    {
        // Arrange
        var color = new HueColor(r, g, b);

        // Act
        var hex = color.ToHexString();

        // Assert
        Assert.Equal(expectedHex, hex);
    }

    [Fact]
    public void FromHexString_WithValidHex_CreatesColor()
    {
        // Act
        var color = HueColor.FromHexString("FF0000");

        // Assert
        Assert.Equal(255, color.Red);
        Assert.Equal(0, color.Green);
        Assert.Equal(0, color.Blue);
    }

    [Fact]
    public void FromHexString_WithHashPrefix_CreatesColor()
    {
        // Act
        var color = HueColor.FromHexString("#00FF00");

        // Assert
        Assert.Equal(0, color.Red);
        Assert.Equal(255, color.Green);
        Assert.Equal(0, color.Blue);
    }

    [Theory]
    [InlineData("000000", 0, 0, 0)]
    [InlineData("FFFFFF", 255, 255, 255)]
    [InlineData("#FF8000", 255, 128, 0)]
    [InlineData("804020", 128, 64, 32)]
    public void FromHexString_VariousHexValues_CreatesCorrectColor(string hex, byte expectedR, byte expectedG, byte expectedB)
    {
        // Act
        var color = HueColor.FromHexString(hex);

        // Assert
        Assert.Equal(expectedR, color.Red);
        Assert.Equal(expectedG, color.Green);
        Assert.Equal(expectedB, color.Blue);
    }

    [Fact]
    public void FromHexString_WithInvalidLength_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => HueColor.FromHexString("FFF"));
        Assert.Equal("hex", exception.ParamName);
    }

    [Fact]
    public void FromHexString_WithInvalidCharacters_ThrowsFormatException()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => HueColor.FromHexString("GGGGGG"));
    }

    [Fact]
    public void ToHexString_AndFromHexString_RoundTrip()
    {
        // Arrange
        var original = new HueColor(128, 64, 192);

        // Act
        var hex = original.ToHexString();
        var restored = HueColor.FromHexString(hex);

        // Assert
        Assert.Equal(original.Red, restored.Red);
        Assert.Equal(original.Green, restored.Green);
        Assert.Equal(original.Blue, restored.Blue);
    }
}

/// <summary>
/// Unit tests for HueBridge model
/// </summary>
public class HueBridgeTests
{
    [Fact]
    public void HueBridge_RequiredProperties_CanBeSet()
    {
        // Act
        var bridge = new HueBridge
        {
            IpAddress = "192.168.1.100",
            BridgeId = "001788FFFE123456"
        };

        // Assert
        Assert.Equal("192.168.1.100", bridge.IpAddress);
        Assert.Equal("001788FFFE123456", bridge.BridgeId);
    }

    [Fact]
    public void HueBridge_OptionalProperties_CanBeSet()
    {
        // Act
        var bridge = new HueBridge
        {
            IpAddress = "192.168.1.100",
            BridgeId = "001788FFFE123456",
            Name = "Philips Hue",
            ModelId = "BSB002"
        };

        // Assert
        Assert.Equal("Philips Hue", bridge.Name);
        Assert.Equal("BSB002", bridge.ModelId);
    }
}

/// <summary>
/// Unit tests for HueLight model
/// </summary>
public class HueLightTests
{
    [Fact]
    public void HueLight_RequiredProperties_CanBeSet()
    {
        // Act
        var light = new HueLight
        {
            Id = "00000000-0000-0000-0000-000000000001",
            Name = "Living Room Light"
        };

        // Assert
        Assert.Equal("00000000-0000-0000-0000-000000000001", light.Id);
        Assert.Equal("Living Room Light", light.Name);
    }

    [Fact]
    public void HueLight_AllProperties_CanBeSet()
    {
        // Act
        var light = new HueLight
        {
            Id = "00000000-0000-0000-0000-000000000001",
            Name = "Living Room Light",
            IsOn = true,
            Brightness = 200,
            SupportsColor = true,
            Type = "Extended color light",
            IsReachable = true
        };

        // Assert
        Assert.True(light.IsOn);
        Assert.Equal(200, light.Brightness);
        Assert.True(light.SupportsColor);
        Assert.Equal("Extended color light", light.Type);
        Assert.True(light.IsReachable);
    }
}

/// <summary>
/// Unit tests for HueAuthResult model
/// </summary>
public class HueAuthResultTests
{
    [Fact]
    public void HueAuthResult_SuccessfulAuth_HasAppKey()
    {
        // Act
        var result = new HueAuthResult
        {
            IsSuccess = true,
            AppKey = "1234567890abcdef",
            RequiresLinkButton = false
        };

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("1234567890abcdef", result.AppKey);
        Assert.False(result.RequiresLinkButton);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void HueAuthResult_LinkButtonRequired_HasFlag()
    {
        // Act
        var result = new HueAuthResult
        {
            IsSuccess = false,
            RequiresLinkButton = true,
            ErrorMessage = "Link button not pressed"
        };

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.RequiresLinkButton);
        Assert.Equal("Link button not pressed", result.ErrorMessage);
        Assert.Null(result.AppKey);
    }

    [Fact]
    public void HueAuthResult_FailedAuth_HasErrorMessage()
    {
        // Act
        var result = new HueAuthResult
        {
            IsSuccess = false,
            ErrorMessage = "Connection timeout",
            RequiresLinkButton = false
        };

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Connection timeout", result.ErrorMessage);
        Assert.False(result.RequiresLinkButton);
        Assert.Null(result.AppKey);
    }
}
