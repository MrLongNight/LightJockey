using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LightJockey.Tests.Services
{
    public class HueServiceIntegrationTests
    {
        private readonly string? _bridgeIp;
        private readonly string? _appKey;

        public HueServiceIntegrationTests()
        {
            // Liest die Secrets aus den Umgebungsvariablen, die von GitHub Actions gesetzt werden.
            _bridgeIp = Environment.GetEnvironmentVariable("HUE_BRIDGE_IP");
            _appKey = Environment.GetEnvironmentVariable("HUE_APP_KEY");
        }

        [Fact]
        public async Task ConnectAndGetLights_WithRealBridge_Succeeds()
        {
            // Diesen Test nur ausführen, wenn die Secrets in der CI-Umgebung vorhanden sind.
            if (string.IsNullOrEmpty(_bridgeIp) || string.IsNullOrEmpty(_appKey))
            {
                // In Xunit gibt es kein Assert.Skip, daher verlassen wir den Test einfach.
                // Alternativ kann man eine separate Testkonfiguration verwenden.
                return;
            }

            // Arrange
            var logger = new NullLogger<HueService>();
            var configServiceMock = new Moq.Mock<IConfigurationService>();
            var service = new HueService(logger, configServiceMock.Object);
            var bridge = new HueBridge { IpAddress = _bridgeIp };

            // Act
            var connected = await service.ConnectAsync(bridge, _appKey);
            var lights = await service.GetLightsAsync();

            // Assert
            Assert.True(connected, "Verbindung zur Hue Bridge fehlgeschlagen.");
            Assert.NotNull(lights);
            Assert.NotEmpty(lights); // Es sollte mindestens ein Licht gefunden werden.
        }
    }
}
