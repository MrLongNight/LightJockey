using System.Collections.Generic;
using LightJockey.Models;

namespace LightJockey.Services
{
    /// <summary>
    /// Provides a service for recording and retrieving performance metrics.
    /// </summary>
    public interface IMetricsService
    {
        /// <summary>
        /// Gets the history of recorded metrics.
        /// </summary>
        IReadOnlyList<PerformanceMetrics> MetricsHistory { get; }

        /// <summary>
        * Records a new set of performance metrics.
        * </summary>
        * <param name="metrics">The performance metrics to record.</param>
        void RecordMetrics(PerformanceMetrics metrics);
    }
}
