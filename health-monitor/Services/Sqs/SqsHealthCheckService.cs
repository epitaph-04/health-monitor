using health_monitor.Models;
using System.Diagnostics;

namespace health_monitor.Services;

public class SqsHealthCheckService : IHealthCheckService
{
    private const int MaximumHistorySize = 500;
    private readonly ApplicationConfiguration _appConfig;
    private readonly ILogger<SqsHealthCheckService> _logger;
    private HealthCheckResult _lastCheckedResult = new()
    {
        Message = "Unknown",
        ResponseTime = TimeSpan.Zero,
        Status = Status.Unknown,
        LastCheckedUtc = DateTime.UtcNow,
    };
    private readonly Queue<HealthCheckResult> _historicalHealthCheckResults = new();

    public SqsHealthCheckService(ApplicationConfiguration appConfig, ILogger<SqsHealthCheckService> logger)
    {
        _appConfig = appConfig;
        _logger = logger;
    }

    public string Id => _appConfig.Id;
    public string Name => _appConfig.Name;
    public ServiceType Type => ServiceType.Sqs;
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
                result.Message = "SQS queue URL (Target) is missing.";
            }
            else if (_appConfig.Target.Contains("simulated-failure"))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(50, 200)));
                throw new Exception("Simulated SQS connection failure.");
            }
            else
            {
                // Simulate SQS queue health check
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(10, 100)));
                
                // In a real implementation, you would:
                // 1. Create SQS client
                // 2. Get queue attributes
                // 3. Check queue exists and is accessible
                // 4. Optionally send a test message and receive it
                
                result.Status = Status.Healthy;
                result.Message = "SQS queue is accessible and healthy";
            }
        }
        catch (Exception ex)
        {
            result.Status = Status.Critical;
            result.Message = $"SQS health check failed: {ex.Message}";
            _logger.LogError(ex, "SQS health check failed for queue {QueueUrl}", _appConfig.Target);
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