using health_monitor.Models;
using System.Data.Common;
using System.Diagnostics;

namespace health_monitor.Services;

public class DbHealthCheckService(ApplicationConfiguration appConfig) : IHealthCheckService
{
    public ApplicationType Type => ApplicationType.Db;
    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        var result = new HealthCheckResult(appConfig.Name);
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            if (string.IsNullOrWhiteSpace(appConfig.Target))
            {
                result.Status = HealthStatus.Unhealthy;
                result.ErrorMessage = "Database connection string (Target) is missing.";
            }
            else if (appConfig.Target.Contains("simulated-failure-connection"))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(50, 200)));
                throw new Exception("Simulated connection failure.");
            }
            else if (string.IsNullOrWhiteSpace(appConfig.Query))
            {
                result.Status = HealthStatus.Unhealthy;
                result.ErrorMessage = "Database query is missing.";
            }
            else
            {
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(10, 150)));

                if (appConfig.Target.Contains("simulated-failure-query")) {
                    result.Status = HealthStatus.Unhealthy;
                    result.ErrorMessage = "Simulated query execution failure.";
                } else {
                    result.Status = HealthStatus.Healthy;
                }
            }

            // In a real scenario:
            // using (var connection = new DbConnection(appConfig.Target)) // Specific provider needed
            // {
            //    await connection.OpenAsync(); // Consider CancellationToken
            //    using (var command = connection.CreateCommand())
            //    {
            //        command.CommandText = string.IsNullOrWhiteSpace(appConfig.Query) ? "SELECT 1" : appConfig.Query;
            //        command.CommandTimeout = appConfig.TimeoutSeconds; // Or a portion for the command itself
            //        await command.ExecuteNonQueryAsync(); // Or ExecuteScalarAsync, etc.
            //        result.Status = HealthStatus.Healthy;
            //    }
            // }
        }
        catch (DbException ex) 
        {
            result.Status = HealthStatus.Unhealthy;
            result.ErrorMessage = $"Database operation failed: {ex.Message}";
        }
        catch (Exception ex)
        {
            result.Status = HealthStatus.Unhealthy;
            result.ErrorMessage = $"An error occurred during DB check: {ex.Message}";
        }
        finally
        {
            stopwatch.Stop();
            result.ResponseTime = stopwatch.Elapsed;
            result.LastCheckedUtc = DateTime.UtcNow;
        }
        return result;
    }
}