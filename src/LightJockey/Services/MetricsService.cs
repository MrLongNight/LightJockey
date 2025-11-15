using System.Collections.Generic;
using System.Linq;
using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services
{
    /// <inheritdoc cref="IMetricsService"/>
    public class MetricsService : IMetricsService
    {
        private readonly ILogger<MetricsService> _logger;
        private readonly List<PerformanceMetrics> _metricsHistory = new();
        private const int MaxHistorySize = 300; // Keep the last 300 metrics

        /// <inheritdoc/>
        public IReadOnlyList<PerformanceMetrics> MetricsHistory => _metricsHistory.AsReadOnly();

        /// <summary>
        * Initializes a new instance of the <see cref="MetricsService"/> class.
        /// </summary>
        * <param name="logger">The logger for the service.</param>
        public MetricsService(ILogger<MetricsService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public void RecordMetrics(PerformanceMetrics metrics)
        {
            lock (_metricsHistory)
            {
                _metricsHistory.Add(metrics);
                if (_metricsHistory.Count > MaxHistorySize)
                {
                    _metricsHistory.RemoveAt(0);
                }
            }
            _logger.LogDebug("Recorded new performance metrics: FPS={FPS}, Latency={Latency}ms", metrics.StreamingFPS, metrics.TotalLatencyMs);
        }
    }
}
