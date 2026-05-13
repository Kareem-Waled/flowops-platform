using FlowOps.Core.Entities;
using FlowOps.Core.Interfaces;

namespace FlowOps.Infrastructure.Prometheus;

public class PrometheusService : IPrometheusService
{
    public Task<double> GetCurrentCpuUsageAsync(string? appName = null, CancellationToken ct = default) => Task.FromResult(24.0);
    public Task<double> GetCurrentMemoryUsageAsync(string? appName = null, CancellationToken ct = default) => Task.FromResult(42.0);
    public Task<double> GetRequestsPerSecondAsync(string? appName = null, CancellationToken ct = default) => Task.FromResult(86.0);
    public Task<MetricSeries> GetCpuTimeSeriesAsync(string? appName = null, int minutes = 10, CancellationToken ct = default) => Task.FromResult(MakeSeries("cpu", minutes, 20));
    public Task<MetricSeries> GetMemoryTimeSeriesAsync(string? appName = null, int minutes = 10, CancellationToken ct = default) => Task.FromResult(MakeSeries("memory", minutes, 38));
    public Task<List<Alert>> GetActiveAlertsAsync(CancellationToken ct = default) => Task.FromResult(new List<Alert>());

    private static MetricSeries MakeSeries(string name, int minutes, double start)
        => new(){ MetricName=name, Points=Enumerable.Range(0, minutes).Select(i => new MetricPoint{ Timestamp=DateTime.UtcNow.AddMinutes(-(minutes-i)), Value=start+i }).ToList() };
}
