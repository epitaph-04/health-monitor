using health_monitor.Models;
using health_monitor.Client.Model;
using health_monitor.Services;

namespace health_monitor.Services.Metrics;

public class MetricsService : IMetricsService
{
    private readonly IEnumerable<IHealthCheckService> _healthCheckServices;
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(IEnumerable<IHealthCheckService> healthCheckServices, ILogger<MetricsService> logger)
    {
        _healthCheckServices = healthCheckServices;
        _logger = logger;
    }

    public async Task<HealthMetrics> CalculateMetrics(string serviceId, TimeSpan period)
    {
        var service = _healthCheckServices.FirstOrDefault(s => s.Id == serviceId);
        if (service == null)
        {
            throw new ArgumentException($"Service {serviceId} not found");
        }

        var endTime = DateTime.UtcNow;
        var startTime = endTime - period;
        var historicalResults = service.GetHistoricalHealthCheckResults()
            .Where(r => r.LastCheckedUtc >= startTime && r.LastCheckedUtc <= endTime)
            .OrderBy(r => r.LastCheckedUtc)
            .ToList();

        var metrics = new HealthMetrics
        {
            ServiceId = serviceId,
            CalculatedAt = DateTime.UtcNow,
            CalculationPeriod = period,
            AvailabilityPercentage = await CalculateAvailability(serviceId, period),
            MeanTimeToRecovery = await CalculateMeanTimeToRecovery(serviceId, period),
            MeanTimeBetweenFailures = await CalculateMeanTimeBetweenFailures(serviceId, period),
            Performance = await CalculatePerformanceMetrics(serviceId, period)
        };

        // Calculate total downtime
        var downtime = TimeSpan.Zero;
        DateTime? failureStart = null;

        foreach (var result in historicalResults)
        {
            if (result.Status == Status.Critical && failureStart == null)
            {
                failureStart = result.LastCheckedUtc;
            }
            else if (result.Status != Status.Critical && failureStart.HasValue)
            {
                downtime = downtime.Add(result.LastCheckedUtc - failureStart.Value);
                failureStart = null;
            }
        }

        // If still in failure state
        if (failureStart.HasValue)
        {
            downtime = downtime.Add(endTime - failureStart.Value);
        }

        metrics.TotalDowntime = downtime;

        return metrics;
    }

    public async Task<PerformanceMetrics> CalculatePerformanceMetrics(string serviceId, TimeSpan period)
    {
        var service = _healthCheckServices.FirstOrDefault(s => s.Id == serviceId);
        if (service == null)
        {
            return new PerformanceMetrics();
        }

        var endTime = DateTime.UtcNow;
        var startTime = endTime - period;
        var historicalResults = service.GetHistoricalHealthCheckResults()
            .Where(r => r.LastCheckedUtc >= startTime && r.LastCheckedUtc <= endTime)
            .ToList();

        if (!historicalResults.Any())
        {
            return new PerformanceMetrics();
        }

        var responseTimes = historicalResults.Select(r => r.ResponseTime.TotalMilliseconds).OrderBy(r => r).ToList();
        var failedRequests = historicalResults.Count(r => r.Status == Status.Critical);

        return new PerformanceMetrics
        {
            TotalRequests = historicalResults.Count,
            FailedRequests = failedRequests,
            ErrorRate = historicalResults.Count > 0 ? (double)failedRequests / historicalResults.Count * 100 : 0,
            AverageResponseTime = responseTimes.Average(),
            P50ResponseTime = GetPercentile(responseTimes, 0.5),
            P95ResponseTime = GetPercentile(responseTimes, 0.95),
            P99ResponseTime = GetPercentile(responseTimes, 0.99)
        };
    }

    public async Task<double> CalculateAvailability(string serviceId, TimeSpan period)
    {
        var service = _healthCheckServices.FirstOrDefault(s => s.Id == serviceId);
        if (service == null)
        {
            return 0;
        }

        var endTime = DateTime.UtcNow;
        var startTime = endTime - period;
        var historicalResults = service.GetHistoricalHealthCheckResults()
            .Where(r => r.LastCheckedUtc >= startTime && r.LastCheckedUtc <= endTime)
            .ToList();

        if (!historicalResults.Any())
        {
            return 100; // Assume 100% if no data
        }

        var healthyResults = historicalResults.Count(r => r.Status == Status.Healthy);
        return (double)healthyResults / historicalResults.Count * 100;
    }

    public async Task<TimeSpan> CalculateMeanTimeToRecovery(string serviceId, TimeSpan period)
    {
        var service = _healthCheckServices.FirstOrDefault(s => s.Id == serviceId);
        if (service == null)
        {
            return TimeSpan.Zero;
        }

        var endTime = DateTime.UtcNow;
        var startTime = endTime - period;
        var historicalResults = service.GetHistoricalHealthCheckResults()
            .Where(r => r.LastCheckedUtc >= startTime && r.LastCheckedUtc <= endTime)
            .OrderBy(r => r.LastCheckedUtc)
            .ToList();

        var recoveryTimes = new List<TimeSpan>();
        DateTime? failureStart = null;

        foreach (var result in historicalResults)
        {
            if (result.Status == Status.Critical && failureStart == null)
            {
                failureStart = result.LastCheckedUtc;
            }
            else if (result.Status != Status.Critical && failureStart.HasValue)
            {
                recoveryTimes.Add(result.LastCheckedUtc - failureStart.Value);
                failureStart = null;
            }
        }

        return recoveryTimes.Any() ? 
            TimeSpan.FromTicks((long)recoveryTimes.Average(rt => rt.Ticks)) : 
            TimeSpan.Zero;
    }

    public async Task<TimeSpan> CalculateMeanTimeBetweenFailures(string serviceId, TimeSpan period)
    {
        var service = _healthCheckServices.FirstOrDefault(s => s.Id == serviceId);
        if (service == null)
        {
            return TimeSpan.Zero;
        }

        var endTime = DateTime.UtcNow;
        var startTime = endTime - period;
        var historicalResults = service.GetHistoricalHealthCheckResults()
            .Where(r => r.LastCheckedUtc >= startTime && r.LastCheckedUtc <= endTime)
            .OrderBy(r => r.LastCheckedUtc)
            .ToList();

        var failureTimes = historicalResults
            .Where(r => r.Status == Status.Critical)
            .Select(r => r.LastCheckedUtc)
            .ToList();

        if (failureTimes.Count <= 1)
        {
            return period; // If 0 or 1 failure, MTBF is the entire period
        }

        var intervals = new List<TimeSpan>();
        for (int i = 1; i < failureTimes.Count; i++)
        {
            intervals.Add(failureTimes[i] - failureTimes[i - 1]);
        }

        return intervals.Any() ? 
            TimeSpan.FromTicks((long)intervals.Average(i => i.Ticks)) : 
            TimeSpan.Zero;
    }

    public async Task<SlaReport> GenerateSlaReport(string serviceId, SlaReportingPeriod period)
    {
        var reportPeriod = GetReportingPeriod(period);
        var endDate = DateTime.UtcNow;
        var startDate = endDate - reportPeriod;

        var metrics = await CalculateMetrics(serviceId, reportPeriod);
        var targetAvailability = 99.9; // Default SLA target

        var allowedDowntime = CalculateAllowedDowntime(reportPeriod, targetAvailability);

        return new SlaReport
        {
            ServiceId = serviceId,
            Period = period,
            StartDate = startDate,
            EndDate = endDate,
            ActualAvailability = metrics.AvailabilityPercentage,
            TargetAvailability = targetAvailability,
            SlaAchieved = metrics.AvailabilityPercentage >= targetAvailability,
            TotalDowntime = metrics.TotalDowntime,
            AllowedDowntime = allowedDowntime,
            AverageResponseTime = metrics.Performance.AverageResponseTime,
            MaxResponseTimeTarget = 5000, // 5 seconds default
            TotalIncidents = await CountIncidents(serviceId, reportPeriod),
            Performance = metrics.Performance
        };
    }

    public async Task<bool> CheckSlaCompliance(string serviceId)
    {
        var monthlyMetrics = await CalculateMetrics(serviceId, TimeSpan.FromDays(30));
        return monthlyMetrics.AvailabilityPercentage >= 99.9; // Default SLA threshold
    }

    private double GetPercentile(List<double> sortedValues, double percentile)
    {
        if (!sortedValues.Any()) return 0;
        
        var index = (int)Math.Ceiling(sortedValues.Count * percentile) - 1;
        index = Math.Max(0, Math.Min(index, sortedValues.Count - 1));
        return sortedValues[index];
    }

    private TimeSpan GetReportingPeriod(SlaReportingPeriod period)
    {
        return period switch
        {
            SlaReportingPeriod.Daily => TimeSpan.FromDays(1),
            SlaReportingPeriod.Weekly => TimeSpan.FromDays(7),
            SlaReportingPeriod.Monthly => TimeSpan.FromDays(30),
            SlaReportingPeriod.Quarterly => TimeSpan.FromDays(90),
            SlaReportingPeriod.Yearly => TimeSpan.FromDays(365),
            _ => TimeSpan.FromDays(30)
        };
    }

    private TimeSpan CalculateAllowedDowntime(TimeSpan period, double availabilityTarget)
    {
        var allowedDowntimePercentage = (100 - availabilityTarget) / 100;
        return TimeSpan.FromTicks((long)(period.Ticks * allowedDowntimePercentage));
    }

    private async Task<int> CountIncidents(string serviceId, TimeSpan period)
    {
        var service = _healthCheckServices.FirstOrDefault(s => s.Id == serviceId);
        if (service == null) return 0;

        var endTime = DateTime.UtcNow;
        var startTime = endTime - period;
        var historicalResults = service.GetHistoricalHealthCheckResults()
            .Where(r => r.LastCheckedUtc >= startTime && r.LastCheckedUtc <= endTime)
            .OrderBy(r => r.LastCheckedUtc)
            .ToList();

        var incidents = 0;
        var inIncident = false;

        foreach (var result in historicalResults)
        {
            if (result.Status == Status.Critical && !inIncident)
            {
                incidents++;
                inIncident = true;
            }
            else if (result.Status != Status.Critical && inIncident)
            {
                inIncident = false;
            }
        }

        return incidents;
    }
}