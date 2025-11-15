using Xunit;
using LightJockey.Services;
using LightJockey.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services
{
    public class MetricsServiceTests
    {
        private readonly Mock<ILogger<MetricsService>> _loggerMock;
        private readonly MetricsService _metricsService;

        public MetricsServiceTests()
        {
            _loggerMock = new Mock<ILogger<MetricsService>>();
            _metricsService = new MetricsService(_loggerMock.Object);
        }

        [Fact]
        public void RecordMetrics_AddsMetricsToHistory()
        {
            // Arrange
            var metrics = new PerformanceMetrics { StreamingFPS = 60, TotalLatencyMs = 16 };

            // Act
            _metricsService.RecordMetrics(metrics);

            // Assert
            Assert.Single(_metricsService.MetricsHistory);
            Assert.Equal(60, _metricsService.MetricsHistory[0].StreamingFPS);
        }

        [Fact]
        public void RecordMetrics_HistorySizeLimited()
        {
            // Arrange
            for (int i = 0; i < 350; i++)
            {
                _metricsService.RecordMetrics(new PerformanceMetrics { StreamingFPS = i });
            }

            // Assert
            Assert.Equal(300, _metricsService.MetricsHistory.Count);
            Assert.Equal(50, _metricsService.MetricsHistory[0].StreamingFPS);
        }
    }
}
