using health_monitor.Models;

namespace health_monitor.Services.Intelligence;

public class PredictiveAnalysisService : IPredictiveAnalysisService
{
    private readonly IEnumerable<IHealthCheckService> _healthCheckServices;
    private readonly ILogger<PredictiveAnalysisService> _logger;

    public PredictiveAnalysisService(IEnumerable<IHealthCheckService> healthCheckServices, ILogger<PredictiveAnalysisService> logger)
    {
        _healthCheckServices = healthCheckServices;
        _logger = logger;
    }

    public async Task<HealthPrediction> PredictHealth(string serviceId, TimeSpan period)
    {
        var service = _healthCheckServices.FirstOrDefault(s => s.Id == serviceId);
        if (service == null)
        {
            throw new ArgumentException($"Service {serviceId} not found");
        }

        var historicalData = service.GetHistoricalHealthCheckResults()
            .OrderBy(r => r.LastCheckedUtc)
            .ToList();

        if (historicalData.Count < 10)
        {
            return new HealthPrediction
            {
                ServiceId = serviceId,
                PredictionPeriod = period,
                PredictedStatus = Status.Unknown,
                Confidence = 0.1,
                Reasoning = "Insufficient historical data for prediction"
            };
        }

        // Simple trend analysis
        var recentData = historicalData.TakeLast(20).ToList();
        var failureRate = recentData.Count(r => r.Status == Status.Critical) / (double)recentData.Count;
        var avgResponseTime = recentData.Average(r => r.ResponseTime.TotalMilliseconds);
        
        // Calculate response time trend
        var responseTimeTrend = CalculateResponseTimeTrend(recentData);
        var statusTrend = CalculateStatusTrend(recentData);

        var prediction = new HealthPrediction
        {
            ServiceId = serviceId,
            PredictionPeriod = period,
            Factors = new Dictionary<string, double>
            {
                ["FailureRate"] = failureRate,
                ["ResponseTimeTrend"] = responseTimeTrend,
                ["StatusTrend"] = statusTrend,
                ["AvgResponseTime"] = avgResponseTime
            }
        };

        // Simple rule-based prediction
        if (failureRate > 0.5 && responseTimeTrend > 0.2)
        {
            prediction.PredictedStatus = Status.Critical;
            prediction.Confidence = 0.8;
            prediction.Reasoning = "High failure rate and increasing response times indicate likely failure";
        }
        else if (failureRate > 0.2 || responseTimeTrend > 0.1)
        {
            prediction.PredictedStatus = Status.Degraded;
            prediction.Confidence = 0.6;
            prediction.Reasoning = "Moderate failure rate or response time increase indicates potential degradation";
        }
        else
        {
            prediction.PredictedStatus = Status.Healthy;
            prediction.Confidence = 0.7;
            prediction.Reasoning = "Service metrics indicate continued healthy operation";
        }

        return prediction;
    }

    public async Task<AnomalyDetection[]> DetectAnomalies(string serviceId)
    {
        var service = _healthCheckServices.FirstOrDefault(s => s.Id == serviceId);
        if (service == null)
        {
            return Array.Empty<AnomalyDetection>();
        }

        var historicalData = service.GetHistoricalHealthCheckResults()
            .OrderBy(r => r.LastCheckedUtc)
            .ToList();

        if (historicalData.Count < 30)
        {
            return Array.Empty<AnomalyDetection>();
        }

        var anomalies = new List<AnomalyDetection>();

        // Response time anomaly detection
        var responseTimes = historicalData.Select(r => r.ResponseTime.TotalMilliseconds).ToList();
        var responseTimeAnomalies = DetectResponseTimeAnomalies(serviceId, responseTimes);
        anomalies.AddRange(responseTimeAnomalies);

        // Error rate anomaly detection
        var errorRateAnomalies = DetectErrorRateAnomalies(serviceId, historicalData);
        anomalies.AddRange(errorRateAnomalies);

        // Pattern anomaly detection
        var patternAnomalies = DetectPatternAnomalies(serviceId, historicalData);
        anomalies.AddRange(patternAnomalies);

        return anomalies.ToArray();
    }

    public async Task<HealthScore> CalculateHealthScore(string serviceId)
    {
        var service = _healthCheckServices.FirstOrDefault(s => s.Id == serviceId);
        if (service == null)
        {
            throw new ArgumentException($"Service {serviceId} not found");
        }

        var historicalData = service.GetHistoricalHealthCheckResults()
            .Take(100) // Last 100 checks
            .ToList();

        if (!historicalData.Any())
        {
            return new HealthScore
            {
                ServiceId = serviceId,
                Overall = 0,
                Availability = 0,
                Performance = 0,
                Reliability = 0,
                Trend = HealthTrend.Critical
            };
        }

        // Calculate availability score (0-100)
        var healthyCount = historicalData.Count(r => r.Status == Status.Healthy);
        var availabilityScore = (double)healthyCount / historicalData.Count * 100;

        // Calculate performance score based on response times
        var responseTimes = historicalData.Select(r => r.ResponseTime.TotalMilliseconds).ToList();
        var avgResponseTime = responseTimes.Average();
        var performanceScore = Math.Max(0, 100 - (avgResponseTime / 50)); // Scale: 0ms=100, 5000ms=0

        // Calculate reliability score (consistency)
        var responseTimeVariance = CalculateVariance(responseTimes);
        var reliabilityScore = Math.Max(0, 100 - (responseTimeVariance / 1000));

        // Overall score (weighted average)
        var overallScore = (availabilityScore * 0.5) + (performanceScore * 0.3) + (reliabilityScore * 0.2);

        // Determine trend
        var trend = await AnalyzeTrend(serviceId, TimeSpan.FromHours(24));

        var recommendations = GenerateRecommendations(availabilityScore, performanceScore, reliabilityScore);

        return new HealthScore
        {
            ServiceId = serviceId,
            Overall = Math.Round(overallScore, 1),
            Availability = Math.Round(availabilityScore, 1),
            Performance = Math.Round(performanceScore, 1),
            Reliability = Math.Round(reliabilityScore, 1),
            Trend = trend,
            Recommendations = recommendations
        };
    }

    public async Task<HealthTrend> AnalyzeTrend(string serviceId, TimeSpan period)
    {
        var service = _healthCheckServices.FirstOrDefault(s => s.Id == serviceId);
        if (service == null)
        {
            return HealthTrend.Critical;
        }

        var endTime = DateTime.UtcNow;
        var startTime = endTime - period;
        var historicalData = service.GetHistoricalHealthCheckResults()
            .Where(r => r.LastCheckedUtc >= startTime)
            .OrderBy(r => r.LastCheckedUtc)
            .ToList();

        if (historicalData.Count < 10)
        {
            return HealthTrend.Stable;
        }

        // Calculate trend in different metrics
        var statusTrend = CalculateStatusTrend(historicalData);
        var responseTimeTrend = CalculateResponseTimeTrend(historicalData);

        // Determine overall trend
        if (statusTrend < -0.3 || responseTimeTrend > 0.5)
        {
            return HealthTrend.Critical;
        }
        else if (statusTrend < -0.1 || responseTimeTrend > 0.2)
        {
            return HealthTrend.Degrading;
        }
        else if (statusTrend > 0.1 && responseTimeTrend < -0.1)
        {
            return HealthTrend.Improving;
        }
        else
        {
            return HealthTrend.Stable;
        }
    }

    public async Task<string[]> PredictFailures(TimeSpan lookAhead)
    {
        var servicesAtRisk = new List<string>();

        foreach (var service in _healthCheckServices)
        {
            try
            {
                var prediction = await PredictHealth(service.Id, lookAhead);
                if (prediction.PredictedStatus == Status.Critical && prediction.Confidence > 0.6)
                {
                    servicesAtRisk.Add(service.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to predict health for service {ServiceId}", service.Id);
            }
        }

        return servicesAtRisk.ToArray();
    }

    public async Task TrainModel(string serviceId)
    {
        // Placeholder for ML model training
        // In a real implementation, this would train ML models using historical data
        _logger.LogInformation("Training predictive model for service {ServiceId}", serviceId);
        await Task.Delay(100); // Simulate training time
    }

    private double CalculateResponseTimeTrend(List<HealthCheckResult> data)
    {
        if (data.Count < 5) return 0;

        var responseTimes = data.Select(r => r.ResponseTime.TotalMilliseconds).ToArray();
        var firstHalf = responseTimes.Take(responseTimes.Length / 2).Average();
        var secondHalf = responseTimes.Skip(responseTimes.Length / 2).Average();

        return (secondHalf - firstHalf) / firstHalf;
    }

    private double CalculateStatusTrend(List<HealthCheckResult> data)
    {
        if (data.Count < 5) return 0;

        var statusValues = data.Select(r => (int)r.Status).ToArray();
        var firstHalf = statusValues.Take(statusValues.Length / 2).Average();
        var secondHalf = statusValues.Skip(statusValues.Length / 2).Average();

        return (secondHalf - firstHalf) / Math.Max(firstHalf, 1);
    }

    private AnomalyDetection[] DetectResponseTimeAnomalies(string serviceId, List<double> responseTimes)
    {
        var anomalies = new List<AnomalyDetection>();
        
        if (responseTimes.Count < 20) return anomalies.ToArray();

        var mean = responseTimes.Average();
        var stdDev = Math.Sqrt(CalculateVariance(responseTimes));
        var threshold = mean + (2 * stdDev); // 2 sigma threshold

        var recentValues = responseTimes.TakeLast(5);
        if (recentValues.Any(rt => rt > threshold))
        {
            anomalies.Add(new AnomalyDetection
            {
                ServiceId = serviceId,
                Type = AnomalyType.ResponseTimeSpike,
                Description = $"Response time spike detected. Recent max: {recentValues.Max():F0}ms, threshold: {threshold:F0}ms",
                Severity = Math.Min(1.0, recentValues.Max() / threshold),
                Confidence = 0.8
            });
        }

        return anomalies.ToArray();
    }

    private AnomalyDetection[] DetectErrorRateAnomalies(string serviceId, List<HealthCheckResult> data)
    {
        var anomalies = new List<AnomalyDetection>();
        
        if (data.Count < 20) return anomalies.ToArray();

        var recentData = data.TakeLast(10).ToList();
        var historicalData = data.Take(data.Count - 10).ToList();

        var recentErrorRate = recentData.Count(r => r.Status == Status.Critical) / (double)recentData.Count;
        var historicalErrorRate = historicalData.Count(r => r.Status == Status.Critical) / (double)historicalData.Count;

        if (recentErrorRate > historicalErrorRate * 2 && recentErrorRate > 0.2)
        {
            anomalies.Add(new AnomalyDetection
            {
                ServiceId = serviceId,
                Type = AnomalyType.ErrorRateIncrease,
                Description = $"Error rate increased from {historicalErrorRate:P1} to {recentErrorRate:P1}",
                Severity = Math.Min(1.0, recentErrorRate / 0.5),
                Confidence = 0.7
            });
        }

        return anomalies.ToArray();
    }

    private AnomalyDetection[] DetectPatternAnomalies(string serviceId, List<HealthCheckResult> data)
    {
        var anomalies = new List<AnomalyDetection>();
        
        // Simple pattern detection - consecutive failures
        var consecutiveFailures = 0;
        var maxConsecutiveFailures = 0;

        foreach (var result in data.TakeLast(50))
        {
            if (result.Status == Status.Critical)
            {
                consecutiveFailures++;
                maxConsecutiveFailures = Math.Max(maxConsecutiveFailures, consecutiveFailures);
            }
            else
            {
                consecutiveFailures = 0;
            }
        }

        if (maxConsecutiveFailures >= 5)
        {
            anomalies.Add(new AnomalyDetection
            {
                ServiceId = serviceId,
                Type = AnomalyType.UnusualPattern,
                Description = $"Unusual pattern: {maxConsecutiveFailures} consecutive failures detected",
                Severity = Math.Min(1.0, maxConsecutiveFailures / 10.0),
                Confidence = 0.9
            });
        }

        return anomalies.ToArray();
    }

    private double CalculateVariance(List<double> values)
    {
        if (values.Count <= 1) return 0;
        
        var mean = values.Average();
        var sumOfSquaredDifferences = values.Sum(val => Math.Pow(val - mean, 2));
        return sumOfSquaredDifferences / (values.Count - 1);
    }

    private string[] GenerateRecommendations(double availability, double performance, double reliability)
    {
        var recommendations = new List<string>();

        if (availability < 95)
        {
            recommendations.Add("Consider implementing redundancy or failover mechanisms");
        }

        if (performance < 70)
        {
            recommendations.Add("Optimize response times through caching or performance tuning");
        }

        if (reliability < 80)
        {
            recommendations.Add("Investigate and reduce performance variability");
        }

        if (availability > 99 && performance > 90 && reliability > 95)
        {
            recommendations.Add("Service is performing excellently - maintain current practices");
        }

        return recommendations.ToArray();
    }
}