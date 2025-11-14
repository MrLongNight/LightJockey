using LightJockey.Models;
using LightJockey.Services;
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
            var metrics = new PerformanceMetrics { FrameCount = 1 };

            // Act
            _metricsService.RecordMetrics(metrics);
            var history = _metricsService.GetMetricsHistory();

            // Assert
            Assert.Single(history);
            Assert.Equal(1, history.First().FrameCount);
        }

        [Fact]
        public void RecordMetrics_HistorySizeIsLimited()
        {
            // Arrange
            for (int i = 0; i < 110; i++)
            {
                _metricsService.RecordMetrics(new PerformanceMetrics { FrameCount = i });
            }

            // Act
            var history = _metricsService.GetMetricsHistory();

            // Assert
            Assert.Equal(100, history.Count());
        }

        [Fact]
        public void GetMetricsHistory_ReturnsCopyOfHistory()
        {
            // Arrange
            _metricsService.RecordMetrics(new PerformanceMetrics { FrameCount = 1 });

            // Act
            var history1 = _metricsService.GetMetricsHistory();
            _metricsService.RecordMetrics(new PerformanceMetrics { FrameCount = 2 });
            var history2 = _metricsService.GetMetricsHistory();

            // Assert
            Assert.Single(history1);
            Assert.Equal(2, history2.Count());
        }
    }
}
