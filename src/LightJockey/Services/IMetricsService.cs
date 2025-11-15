using LightJockey.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightJockey.Services
{
    public interface IMetricsService
    {
        void RecordMetrics(PerformanceMetrics metrics);
        IEnumerable<PerformanceMetrics> GetMetricsHistory();
        Task<string> ExportMetricsToCsvAsync();
    }
}
