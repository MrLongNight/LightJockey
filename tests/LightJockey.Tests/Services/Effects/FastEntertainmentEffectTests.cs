using LightJockey.Models;
using LightJockey.Services;
using LightJockey.Services.Effects;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LightJockey.Tests.Services.Effects
{
    public class FastEntertainmentEffectTests
    {
        private readonly Mock<ILogger<FastEntertainmentEffect>> _mockLogger;
        private readonly Mock<IEntertainmentService> _mockEntertainmentService;
        private readonly FastEntertainmentEffect _effect;

        public FastEntertainmentEffectTests()
        {
            _mockLogger = new Mock<ILogger<FastEntertainmentEffect>>();
            _mockEntertainmentService = new Mock<IEntertainmentService>();
            _effect = new FastEntertainmentEffect(_mockLogger.Object, _mockEntertainmentService.Object);
        }

        [Fact]
        public async Task StateChanged_EventRaisedOnStateChange()
        {
            // Arrange
            EffectState? newState = null;
            _effect.StateChanged += (sender, state) => newState = state;
            var config = new EffectConfig();

            // Act
            await _effect.InitializeAsync(config);

            // Assert
            Assert.Equal(EffectState.Initialized, newState);
        }
    }
}
