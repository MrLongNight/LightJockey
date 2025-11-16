using LightJockey.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (_metricsHistory.Count >= MaxHistorySize)
            {
                _metricsHistory.RemoveAt(0);
            }
            _metricsHistory.Add(metrics);
        }

        public void RecordCpuUsage(double cpuUsage)
        {
            UpdateLatestMetrics(m => m.CpuUsage = cpuUsage);
        }

        public void RecordMemoryUsage(long memoryUsage)
        {
            UpdateLatestMetrics(m => m.MemoryUsage = memoryUsage);
        }

        public void RecordFps(double fps)
        {
            UpdateLatestMetrics(m => m.StreamingFPS = fps);
        }

        private void UpdateLatestMetrics(Action<PerformanceMetrics> updateAction)
        {
            var now = DateTime.UtcNow;
            var latestMetrics = _metricsHistory.LastOrDefault();

            if (latestMetrics == null || (now - latestMetrics.Timestamp).TotalSeconds > 1)
            {
                latestMetrics = new PerformanceMetrics { Timestamp = now };
                _metricsHistory.Add(latestMetrics);
                if (_metricsHistory.Count > MaxHistorySize)
                {
                    _metricsHistory.RemoveAt(0);
                }
            }

            updateAction(latestMetrics);
            _logger.LogInformation("Metrics updated at {Timestamp}", latestMetrics.Timestamp);
        }

        public IEnumerable<PerformanceMetrics> GetMetricsHistory()
        {
            return _metricsHistory.ToList();
        }

        public async Task<string> ExportMetricsToCsvAsync()
        {
            var history = GetMetricsHistory();
            var filePath = Path.Combine(Path.GetTempPath(), $"metrics_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");

            var csv = new StringBuilder();
            csv.AppendLine("Timestamp,StreamingFPS,AudioLatencyMs,FFTLatencyMs,EffectLatencyMs,TotalLatencyMs,FrameCount,CpuUsage,MemoryUsage");

            foreach (var metrics in history)
            {
                csv.AppendLine(string.Join(",",
                    metrics.Timestamp.ToString(CultureInfo.InvariantCulture),
                    metrics.StreamingFPS.ToString(CultureInfo.InvariantCulture),
                    metrics.AudioLatencyMs.ToString(CultureInfo.InvariantCulture),
                    metrics.FFTLatencyMs.ToString(CultureInfo.InvariantCulture),
                    metrics.EffectLatencyMs.ToString(CultureInfo.InvariantCulture),
                    metrics.TotalLatencyMs.ToString(CultureInfo.InvariantCulture),
                    metrics.FrameCount,
                    metrics.CpuUsage.ToString(CultureInfo.InvariantCulture),
                    metrics.MemoryUsage.ToString(CultureInfo.InvariantCulture)
                ));
            }

            await File.WriteAllTextAsync(filePath, csv.ToString());
            _logger.LogInformation("Exported metrics to {FilePath}", filePath);
            return filePath;
        }
    }
}
