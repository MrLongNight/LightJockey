using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace LightJockey.Tests.Services
{
    public class EffectEngineTests
    {
        private readonly Mock<ILogger<EffectEngine>> _mockLogger;
        private readonly Mock<IAudioService> _mockAudioService;
        private readonly Mock<ISpectralAnalyzer> _mockSpectralAnalyzer;
        private readonly Mock<IBeatDetector> _mockBeatDetector;
        private readonly EffectEngine _effectEngine;
        private readonly Mock<IEffectPlugin> _mockPlugin;

        public EffectEngineTests()
        {
            _mockLogger = new Mock<ILogger<EffectEngine>>();
            _mockAudioService = new Mock<IAudioService>();
            _mockSpectralAnalyzer = new Mock<ISpectralAnalyzer>();
            _mockBeatDetector = new Mock<IBeatDetector>();

            _effectEngine = new EffectEngine(
                _mockLogger.Object,
                _mockAudioService.Object,
                _mockSpectralAnalyzer.Object,
                _mockBeatDetector.Object);

            _mockPlugin = new Mock<IEffectPlugin>();
            _mockPlugin.Setup(p => p.Name).Returns("TestPlugin");
        }

        [Fact]
        public async Task UpdateActiveEffectConfig_WithActiveEffect_UpdatesConfig()
        {
            // Arrange
            var config1 = new EffectConfig { Intensity = 0.5 };
            var config2 = new EffectConfig { Intensity = 0.8 };
            _mockPlugin.Setup(p => p.InitializeAsync(config1)).ReturnsAsync(true);
            _effectEngine.RegisterPlugin(_mockPlugin.Object);
            await _effectEngine.SetActiveEffectAsync("TestPlugin", config1);

            // Act
            _effectEngine.UpdateActiveEffectConfig(config2);

            // Assert
            _mockPlugin.Verify(p => p.UpdateConfig(config2), Times.Once);
        }

        [Fact]
        public async Task SpectralDataEvent_ForwardsToActiveEffect()
        {
            // Arrange
            var config = new EffectConfig();
            _mockPlugin.Setup(p => p.State).Returns(EffectState.Running);
            _mockPlugin.Setup(p => p.InitializeAsync(config)).ReturnsAsync(true);
            _effectEngine.RegisterPlugin(_mockPlugin.Object);
            await _effectEngine.SetActiveEffectAsync("TestPlugin", config);

            var spectralData = new SpectralDataEventArgs(0.5, 0.6, 0.7);

            // Act
            _mockSpectralAnalyzer.Raise(sa => sa.SpectralDataAvailable += null, _mockSpectralAnalyzer.Object, spectralData);

            // Assert
            _mockPlugin.Verify(p => p.OnSpectralData(spectralData), Times.Once);
        }

        [Fact]
        public async Task BeatDetectedEvent_ForwardsToActiveEffect()
        {
            // Arrange
            var config = new EffectConfig();
            _mockPlugin.Setup(p => p.State).Returns(EffectState.Running);
            _mockPlugin.Setup(p => p.InitializeAsync(config)).ReturnsAsync(true);
            _effectEngine.RegisterPlugin(_mockPlugin.Object);
            await _effectEngine.SetActiveEffectAsync("TestPlugin", config);

            var beatData = new BeatDetectedEventArgs(0.8, 120, 0.9);

            // Act
            _mockBeatDetector.Raise(bd => bd.BeatDetected += null, _mockBeatDetector.Object, beatData);

            // Assert
            _mockPlugin.Verify(p => p.OnBeatDetected(beatData), Times.Once);
        }

        [Fact]
        public async Task Dispose_StopsActiveEffectAndDisposesPlugins()
        {
            // Arrange
            var config = new EffectConfig();
            _mockPlugin.Setup(p => p.InitializeAsync(config)).ReturnsAsync(true);
            _effectEngine.RegisterPlugin(_mockPlugin.Object);
            await _effectEngine.SetActiveEffectAsync("TestPlugin", config);

            // Act
            _effectEngine.Dispose();

            // Assert
            _mockPlugin.Verify(p => p.StopAsync(), Times.Once);
            _mockPlugin.Verify(p => p.Dispose(), Times.Once);
        }
    }
}
