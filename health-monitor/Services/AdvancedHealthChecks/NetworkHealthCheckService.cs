using health_monitor.Models;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace health_monitor.Services.AdvancedHealthChecks;

public class NetworkHealthCheckService : IHealthCheckService
{
    private const int MaximumHistorySize = 500;
    private readonly ApplicationConfiguration _appConfig;
    private readonly ILogger<NetworkHealthCheckService> _logger;
    private HealthCheckResult _lastCheckedResult = new()
    {
        Message = "Unknown",
        ResponseTime = TimeSpan.Zero,
        Status = Status.Unknown,
        LastCheckedUtc = DateTime.UtcNow,
    };
    private readonly Queue<HealthCheckResult> _historicalHealthCheckResults = new();

    public NetworkHealthCheckService(ApplicationConfiguration appConfig, ILogger<NetworkHealthCheckService> logger)
    {
        _appConfig = appConfig;
        _logger = logger;
    }

    public string Id => _appConfig.Id;
    public string Name => _appConfig.Name;
    public ServiceType Type => ServiceType.Network;
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
            var checks = new List<string>();
            var hasWarnings = false;
            var hasCritical = false;

            // Parse target for host and port
            var parts = _appConfig.Target.Split(':');
            var host = parts[0];
            var port = parts.Length > 1 ? int.Parse(parts[1]) : 80;

            // Ping test
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host, _appConfig.TimeoutSeconds * 1000);
                
                if (reply.Status == IPStatus.Success)
                {
                    checks.Add($"Ping: {reply.RoundtripTime}ms");
                    if (reply.RoundtripTime > 1000)
                        hasWarnings = true;
                }
                else
                {
                    checks.Add($"Ping: Failed ({reply.Status})");
                    hasCritical = true;
                }
            }
            catch (Exception ex)
            {
                checks.Add($"Ping: Error ({ex.Message})");
                hasCritical = true;
            }

            // Port connectivity test
            try
            {
                using var tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync(host, port);
                var timeoutTask = Task.Delay(_appConfig.TimeoutSeconds * 1000);
                
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                
                if (completedTask == connectTask && tcpClient.Connected)
                {
                    checks.Add($"Port {port}: Open");
                }
                else
                {
                    checks.Add($"Port {port}: Closed/Timeout");
                    hasCritical = true;
                }
            }
            catch (Exception ex)
            {
                checks.Add($"Port {port}: Error ({ex.Message})");
                hasCritical = true;
            }

            result.Status = hasCritical ? Status.Critical : hasWarnings ? Status.Degraded : Status.Healthy;
            result.Message = string.Join(", ", checks);
        }
        catch (Exception ex)
        {
            result.Status = Status.Critical;
            result.Message = $"Network check failed: {ex.Message}";
            _logger.LogError(ex, "Network health check failed for {Target}", _appConfig.Target);
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