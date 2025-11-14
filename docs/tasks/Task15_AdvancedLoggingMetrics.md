# Task 15: Advanced Logging & Metrics

This document provides an overview of the advanced logging and metrics system implemented in LightJockey.

## Serilog Configuration

The Serilog logging configuration has been updated to provide more detailed and structured logs. The following enrichers have been added:

- `WithProcessId()`: Adds the process ID to each log event.
- `WithThreadId()`: Adds the thread ID to each log event.

The log output template has been updated to include this new information, which will help with debugging and performance analysis.

## Metrics Service

A new `MetricsService` has been implemented to collect and store historical performance data. This service records a history of `PerformanceMetrics` objects, which include the following information:

- Streaming FPS
- Audio Latency
- FFT Latency
- Effect Latency
- Total Latency
- Frame Count
- Timestamp

The `MetricsService` stores a maximum of 100 metrics records in memory.

## UI Display

A new "Metrics" tab has been added to the main window to display the collected metrics. This view provides a real-time display of the metrics history in a data grid.

### Screenshot

![Metrics View](placeholder.png)
