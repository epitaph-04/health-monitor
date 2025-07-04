using health_monitor.Models;
using System.Diagnostics;

namespace health_monitor.Services;

public class SnsHealthCheckService : IHealthCheckService
{
    private const int MaximumHistorySize = 500;
    private readonly ApplicationConfiguration _appConfig;
    private readonly ILogger<SnsHealthCheckService> _logger;
    private HealthCheckResult _lastCheckedResult = new()
    {
        Message = "Unknown",
        ResponseTime = TimeSpan.Zero,
        Status = Status.Unknown,
        LastCheckedUtc = DateTime.UtcNow,
    };
    private readonly Queue<HealthCheckResult> _historicalHealthCheckResults = new();

    public SnsHealthCheckService(ApplicationConfiguration appConfig, ILogger<SnsHealthCheckService> logger)
    {
        _appConfig = appConfig;
        _logger = logger;
    }

    public string Id => _appConfig.Id;
    public string Name => _appConfig.Name;
    public ServiceType Type => ServiceType.Sns;
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
            if (string.IsNullOrWhiteSpace(_appConfig.Target))
            {
                result.Status = Status.Critical;
                result.Message = "SNS topic ARN (Target) is missing.";
            }
            else if (_appConfig.Target.Contains("simulated-failure"))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(50, 200)));
                throw new Exception("Simulated SNS connection failure.");
            }
            else
            {
                // Simulate SNS topic health check
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(10, 120)));
                
                // In a real implementation, you would:
                // 1. Create SNS client
                // 2. Get topic attributes
                // 3. Check topic exists and is accessible
                // 4. Optionally publish a test message
                
                result.Status = Status.Healthy;
                result.Message = "SNS topic is accessible and healthy";
            }
        }
        catch (Exception ex)
        {
            result.Status = Status.Critical;
            result.Message = $"SNS health check failed: {ex.Message}";
            _logger.LogError(ex, "SNS health check failed for topic {TopicArn}", _appConfig.Target);
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