using LightJockey.Models;
using System.Collections.Generic;

namespace LightJockey.Services
{
    public interface IMetricsService
    {
        void RecordMetrics(PerformanceMetrics metrics);
        IEnumerable<PerformanceMetrics> GetMetricsHistory();
    }
}
