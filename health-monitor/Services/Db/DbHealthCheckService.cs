using health_monitor.Models;
using System.Data.Common;
using System.Diagnostics;
using health_monitor.Client.Model;

namespace health_monitor.Services;

public class DbHealthCheckService(ApplicationConfiguration appConfig) : IHealthCheckService
{
    private HealthCheckResult _lastCheckedResult = new()
    {
        Message = "Unknown",
        ResponseTime = TimeSpan.Zero,
        Status = Status.Unknown,
        LastCheckedUtc = DateTime.UtcNow,
    };
    private readonly Queue<HealthCheckResult> _historicalHealthCheckResults = new(10);
    
    public string Id => appConfig.Id;
    public string Name => appConfig.Name;
    public ServiceType Type => ServiceType.Db;
    public string Target => appConfig.Target;
    public HealthCheckResult LastCheckedResult => _lastCheckedResult;
    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        var result = new HealthCheckResult();
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        try
        {
            if (string.IsNullOrWhiteSpace(appConfig.Target))
            {
                result.Status = Status.Critical;
                result.Message = "Database connection string (Target) is missing.";
            }
            else if (appConfig.Target.Contains("simulated-failure-connection"))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(50, 200)));
                throw new Exception("Simulated connection failure.");
            }
            else if (string.IsNullOrWhiteSpace(appConfig.Query))
            {
                result.Status = Status.Critical;
                result.Message = "Database query is missing.";
            }
            else
            {
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(10, 150)));

                if (appConfig.Target.Contains("simulated-failure-query")) {
                    result.Status = Status.Critical;
                    result.Message = "Simulated query execution failure.";
                } else {
                    result.Status = Status.Healthy;
                    result.Message = "Ok - 200";
                }
            }
        }
        catch (DbException ex) 
        {
            result.Status = Status.Critical;
            result.Message = $"Database operation failed: {ex.Message}";
        }
        catch (Exception ex)
        {
            result.Status = Status.Critical;
            result.Message = $"An error occurred during DB check: {ex.Message}";
        }
        finally
        {
            stopwatch.Stop();
            result.ResponseTime = stopwatch.Elapsed;
            result.LastCheckedUtc = DateTime.UtcNow;
        }
        _historicalHealthCheckResults.Enqueue(_lastCheckedResult);
        _lastCheckedResult = result;
        return result;
    }
    public IEnumerable<HealthCheckResult> GetHistoricalHealthCheckResults()
    {
        return _historicalHealthCheckResults.Reverse();
    }
}