using health_monitor.Models;

namespace health_monitor.Services.Intelligence;

public interface IPredictiveAnalysisService
{
    Task<HealthPrediction> PredictHealth(string serviceId, TimeSpan period);
    Task<AnomalyDetection[]> DetectAnomalies(string serviceId);
    Task<HealthScore> CalculateHealthScore(string serviceId);
    Task<HealthTrend> AnalyzeTrend(string serviceId, TimeSpan period);
    Task<string[]> PredictFailures(TimeSpan lookAhead);
    Task TrainModel(string serviceId);
}

public class HealthPrediction
{
    public string ServiceId { get; set; } = null!;
    public DateTime PredictionTime { get; set; } = DateTime.UtcNow;
    public TimeSpan PredictionPeriod { get; set; }
    public Status PredictedStatus { get; set; }
    public double Confidence { get; set; }
    public string Reasoning { get; set; } = null!;
    public Dictionary<string, double> Factors { get; set; } = new();
}

public class AnomalyDetection
{
    public string ServiceId { get; set; } = null!;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public AnomalyType Type { get; set; }
    public string Description { get; set; } = null!;
    public double Severity { get; set; }
    public double Confidence { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum AnomalyType
{
    ResponseTimeSpike,
    ErrorRateIncrease,
    AvailabilityDrop,
    UnusualPattern,
    PerformanceDegradation
}

public class HealthScore
{
    public string ServiceId { get; set; } = null!;
    public double Overall { get; set; }        // 0-100
    public double Availability { get; set; }
    public double Performance { get; set; }
    public double Reliability { get; set; }
    public HealthTrend Trend { get; set; }     
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public string[] Recommendations { get; set; } = [];
}

public enum HealthTrend
{
    Improving,
    Stable,
    Degrading,
    Critical
}