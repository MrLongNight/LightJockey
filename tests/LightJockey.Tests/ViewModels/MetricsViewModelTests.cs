using System;
using System.Collections.Generic;
using System.Linq;
using LightJockey.Models;
using LightJockey.Services;
using LightJockey.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
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
        public void Constructor_InitializesSeriesAndLabels()
        {
            Assert.NotNull(_viewModel.Series);
            Assert.Equal(2, _viewModel.Series.Count);
            Assert.NotNull(_viewModel.Labels);
        }

        [Fact]
        public void UpdateChart_UpdatesSeriesAndLabelsFromMetricsHistory()
        {
            // Arrange
            var metrics = new List<PerformanceMetrics>
            {
                new PerformanceMetrics { StreamingFPS = 60, TotalLatencyMs = 15, Timestamp = DateTime.Now },
                new PerformanceMetrics { StreamingFPS = 58, TotalLatencyMs = 18, Timestamp = DateTime.Now.AddSeconds(1) }
            };
            _metricsServiceMock.Setup(s => s.MetricsHistory).Returns(metrics.AsReadOnly());

            // Act
            // The timer will trigger the UpdateChart method, but for testing, we call it directly.
            // In a real scenario, you might need to use a mock dispatcher timer.
            _viewModel.GetType().GetMethod("UpdateChart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(_viewModel, null);

            // Assert
            Assert.Equal(2, ((ICollection<double>)_viewModel.Series[0].Values).Count);
            Assert.Equal(60, ((ICollection<double>)_viewModel.Series[0].Values).First());
            Assert.Equal(58, ((ICollection<double>)_viewModel.Series[0].Values).Last());

            Assert.Equal(2, ((ICollection<double>)_viewModel.Series[1].Values).Count);
            Assert.Equal(15, ((ICollection<double>)_viewModel.Series[1].Values).First());
            Assert.Equal(18, ((ICollection<double>)_viewModel.Series[1].Values).Last());

            Assert.Equal(2, _viewModel.Labels.Count);
        }
    }
}
