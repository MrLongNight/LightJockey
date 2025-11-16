using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LightJockey.Tests.Services
{
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
        public async Task ConnectAsync_WithNoAppKeyAndNoStoredKey_ReturnsFalse()
        {
            // Arrange
            var bridge = new HueBridge { IpAddress = "192.168.1.1", BridgeId = "test" };
            _mockConfigurationService
                .Setup(s => s.GetSecureValueAsync(It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _service.ConnectAsync(bridge);

            // Assert
            Assert.False(result);
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

            // Act
            await _service.ConnectAsync(bridge);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieved AppKey from secure storage")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        public void Dispose()
        {
            _service?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
