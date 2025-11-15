using System;
using LightJockey.Models;

namespace LightJockey.Services
{
    public interface IMetricsService
    {
        event EventHandler<PerformanceMetrics> MetricsUpdated;
        void ExportMetrics(string filePath);
    }
}
