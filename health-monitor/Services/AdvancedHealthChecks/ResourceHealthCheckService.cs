using health_monitor.Models;
using System.Diagnostics;

namespace health_monitor.Services.AdvancedHealthChecks;

public class ResourceHealthCheckService : IHealthCheckService
{
    private const int MaximumHistorySize = 500;
    private readonly ApplicationConfiguration _appConfig;
    private readonly ILogger<ResourceHealthCheckService> _logger;
    private HealthCheckResult _lastCheckedResult = new()
    {
        Message = "Unknown",
        ResponseTime = TimeSpan.Zero,
        Status = Status.Unknown,
        LastCheckedUtc = DateTime.UtcNow,
    };
    private readonly Queue<HealthCheckResult> _historicalHealthCheckResults = new();

    public ResourceHealthCheckService(ApplicationConfiguration appConfig, ILogger<ResourceHealthCheckService> logger)
    {
        _appConfig = appConfig;
        _logger = logger;
    }

    public string Id => _appConfig.Id;
    public string Name => _appConfig.Name;
    public ServiceType Type => ServiceType.Resource;
    public string Target => _appConfig.Target;
    public string[] Tag => _appConfig.Tag;
    public HealthCheckResult LastCheckedResult => _lastCheckedResult;

    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        var result = new HealthCheckResult();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            var resourceMetrics = new List<string>();
            var hasWarnings = false;
            var hasCritical = false;

            // Simulated resource checks for demo
            var diskUsage = new Random().NextDouble() * 60 + 20; // 20-80%
            var memoryUsage = new Random().NextDouble() * 50 + 30; // 30-80%
            var cpuUsage = new Random().NextDouble() * 70 + 10; // 10-80%

            // Check thresholds
            if (diskUsage > 90 || memoryUsage > 90 || cpuUsage > 90)
                hasCritical = true;
            else if (diskUsage > 80 || memoryUsage > 80 || cpuUsage > 80)
                hasWarnings = true;

            resourceMetrics.Add($"Disk: {diskUsage:F1}%");
            resourceMetrics.Add($"Memory: {memoryUsage:F1}%");
            resourceMetrics.Add($"CPU: {cpuUsage:F1}%");

            result.Status = hasCritical ? Status.Critical : hasWarnings ? Status.Degraded : Status.Healthy;
            result.Message = string.Join(", ", resourceMetrics);
        }
        catch (Exception ex)
        {
            result.Status = Status.Critical;
            result.Message = $"Resource check failed: {ex.Message}";
            _logger.LogError(ex, "Resource health check failed");
        }
        finally
        {
            stopwatch.Stop();
            result.ResponseTime = stopwatch.Elapsed;
            result.LastCheckedUtc = DateTime.UtcNow;
        }

        EnqueueHealthCheckResult(_lastCheckedResult);
        _lastCheckedResult = result;
        return result;
    }

    public IEnumerable<HealthCheckResult> GetHistoricalHealthCheckResults()
    {
        return _historicalHealthCheckResults.Reverse();
    }

    private void EnqueueHealthCheckResult(HealthCheckResult result)
    {
        _historicalHealthCheckResults.TrimExcess();
        if (_historicalHealthCheckResults.Count >= MaximumHistorySize)
        {
            for (int i = 0; i < _historicalHealthCheckResults.Count - MaximumHistorySize; i++)
            {
                _historicalHealthCheckResults.Dequeue();
            }
        }
        _historicalHealthCheckResults.Enqueue(result);
    }
}