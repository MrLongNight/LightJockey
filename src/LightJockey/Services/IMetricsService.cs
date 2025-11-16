using System.Collections.Generic;
using System.Threading.Tasks;
using LightJockey.Models;

namespace LightJockey.Services
{
    public interface IMetricsService
    {
        void RecordMetrics(PerformanceMetrics metrics);
        void RecordCpuUsage(double cpuUsage);
        void RecordMemoryUsage(long memoryUsage);
        void RecordFps(double fps);
        Task<string> ExportMetricsToCsvAsync();
        IEnumerable<PerformanceMetrics> GetMetricsHistory();
    }
}
