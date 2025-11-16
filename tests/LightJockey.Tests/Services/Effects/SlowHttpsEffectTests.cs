using LightJockey.Models;
using LightJockey.Services;
using LightJockey.Services.Effects;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LightJockey.Tests.Services.Effects
{
    public class SlowHttpsEffectTests
    {
        private readonly Mock<ILogger<SlowHttpsEffect>> _mockLogger;
        private readonly Mock<IHueService> _mockHueService;
        private readonly SlowHttpsEffect _effect;

        public SlowHttpsEffectTests()
        {
            _mockLogger = new Mock<ILogger<SlowHttpsEffect>>();
            _mockHueService = new Mock<IHueService>();
            _effect = new SlowHttpsEffect(_mockLogger.Object, _mockHueService.Object);
        }

        [Fact]
        public async Task StateChanged_EventRaisedOnStateChange()
        {
            // Arrange
            EffectState? newState = null;
            _effect.StateChanged += (sender, state) => newState = state;
            _mockHueService.Setup(h => h.IsConnected).Returns(true);
            _mockHueService.Setup(h => h.GetLightsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HueLight>());
            var config = new EffectConfig();

            // Act
            await _effect.InitializeAsync(config);

            // Assert
            Assert.Equal(EffectState.Initialized, newState);
        }
    }
}
