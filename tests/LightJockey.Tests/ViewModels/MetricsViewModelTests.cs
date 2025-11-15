using LightJockey.Services;
using LightJockey.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace LightJockey.Tests.ViewModels
{
    public class MetricsViewModelTests
    {
        private readonly Mock<IMetricsService> _metricsServiceMock;
        private readonly Mock<ILogger<MetricsViewModel>> _loggerMock;
        private readonly MetricsViewModel _viewModel;

        public MetricsViewModelTests()
        {
            _metricsServiceMock = new Mock<IMetricsService>();
            _loggerMock = new Mock<ILogger<MetricsViewModel>>();
            _viewModel = new MetricsViewModel(_metricsServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ExportMetricsCommand_CallsServiceAndUpdatesStatus()
        {
            // Arrange
            var expectedFilePath = "path/to/metrics.csv";
            _metricsServiceMock.Setup(s => s.ExportMetricsToCsvAsync()).ReturnsAsync(expectedFilePath);

            // Act
            await _viewModel.ExportMetricsCommand.ExecuteAsync(null);

            // Assert
            _metricsServiceMock.Verify(s => s.ExportMetricsToCsvAsync(), Times.Once);
            Assert.Equal($"Metrics exported to {expectedFilePath}", _viewModel.ExportStatusMessage);
        }

        [Fact]
        public async Task ExportMetricsCommand_HandlesExceptionAndUpdatesStatus()
        {
            // Arrange
            _metricsServiceMock.Setup(s => s.ExportMetricsToCsvAsync()).ThrowsAsync(new System.Exception("Test exception"));

            // Act
            await _viewModel.ExportMetricsCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Export failed. Check logs for details.", _viewModel.ExportStatusMessage);
        }
    }
}
