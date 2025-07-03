using health_monitor.Client.Model;
using health_monitor.Models;
using health_monitor.Services;
using System.Diagnostics;

namespace health_monitor.Services;

public class RabbitmqHealthCheckService : IHealthCheckService
{
    private const int MaximumHistorySize = 500;
    private readonly ApplicationConfiguration _appConfig;
    private readonly ILogger<RabbitmqHealthCheckService> _logger;
    private HealthCheckResult _lastCheckedResult = new()
    {
        Message = "Unknown",
        ResponseTime = TimeSpan.Zero,
        Status = Status.Unknown,
        LastCheckedUtc = DateTime.UtcNow,
    };
    private readonly Queue<HealthCheckResult> _historicalHealthCheckResults = new();

    public RabbitmqHealthCheckService(ApplicationConfiguration appConfig, ILogger<RabbitmqHealthCheckService> logger)
    {
        _appConfig = appConfig;
        _logger = logger;
    }

    public string Id => _appConfig.Id;
    public string Name => _appConfig.Name;
    public ServiceType Type => ServiceType.Rabbitmq;
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
                result.Message = "RabbitMQ connection string (Target) is missing.";
            }
            else if (_appConfig.Target.Contains("simulated-failure"))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(50, 200)));
                throw new Exception("Simulated RabbitMQ connection failure.");
            }
            else
            {
                // Simulate RabbitMQ connection health check
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(15, 150)));
                
                // In a real implementation, you would:
                // 1. Create RabbitMQ connection
                // 2. Open a channel
                // 3. Check broker is responding
                // 4. Optionally declare a test queue/exchange
                // 5. Send/receive a test message
                
                result.Status = Status.Healthy;
                result.Message = "RabbitMQ broker is accessible and healthy";
            }
        }
        catch (Exception ex)
        {
            result.Status = Status.Critical;
            result.Message = $"RabbitMQ health check failed: {ex.Message}";
            _logger.LogError(ex, "RabbitMQ health check failed for connection {ConnectionString}", _appConfig.Target);
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