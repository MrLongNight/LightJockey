using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services;

/// <summary>
/// Unit tests for ExampleService demonstrating error handling and logging
/// </summary>
public class ExampleServiceTests
{
    private readonly Mock<ILogger<ExampleService>> _mockLogger;
    private readonly ExampleService _service;

    public ExampleServiceTests()
    {
        _mockLogger = new Mock<ILogger<ExampleService>>();
        _service = new ExampleService(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ExampleService(null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidLogger_LogsDebugMessage()
    {
        // Arrange & Act
        var logger = new Mock<ILogger<ExampleService>>();
        _ = new ExampleService(logger.Object);

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
    public void PerformOperation_WithShouldThrowFalse_CompletesSuccessfully()
    {
        // Act
        _service.PerformOperation(shouldThrow: false);

        // Assert - verify Info log was called for starting and completing
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting example operation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void PerformOperation_WithShouldThrowTrue_LogsErrorAndThrows()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _service.PerformOperation(shouldThrow: true));
        Assert.Equal("Example error to demonstrate error handling", exception.Message);

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred")),
                It.Is<Exception>(ex => ex is InvalidOperationException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void PerformOperation_WithShouldThrowTrue_DoesNotLogSuccessMessage()
    {
        // Act
        try
        {
            _service.PerformOperation(shouldThrow: true);
        }
        catch (InvalidOperationException)
        {
            // Expected exception
        }

        // Assert - verify success message was NOT logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completed successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
