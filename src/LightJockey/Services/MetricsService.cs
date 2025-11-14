using LightJockey.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace LightJockey.Services
{
    public class MetricsService : IMetricsService
    {
        private readonly ILogger<MetricsService> _logger;
        private readonly List<PerformanceMetrics> _metricsHistory = new();
        private const int MaxHistorySize = 100;

        public MetricsService(ILogger<MetricsService> logger)
        {
            _logger = logger;
        }

        public void RecordMetrics(PerformanceMetrics metrics)
        {
            _metricsHistory.Add(metrics);
            if (_metricsHistory.Count > MaxHistorySize)
            {
                _metricsHistory.RemoveAt(0);
            }
            _logger.LogInformation("Metrics recorded at {Timestamp}", metrics.Timestamp);
        }

        public IEnumerable<PerformanceMetrics> GetMetricsHistory()
        {
            return _metricsHistory.ToList();
        }
    }
}
