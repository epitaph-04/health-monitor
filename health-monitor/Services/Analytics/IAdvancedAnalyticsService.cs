using health_monitor.Models;
using health_monitor.Services.Intelligence;

namespace health_monitor.Services.Analytics;

public interface IAdvancedAnalyticsService
{
    Task<TimeSeriesAnalysis> AnalyzeTimeSeries(string serviceId, TimeSpan period, TimeSeriesMetric metric);
    Task<CorrelationAnalysis> AnalyzeCorrelations(string[] serviceIds, TimeSpan period);
    Task<CapacityPlanningReport> GenerateCapacityPlan(string serviceId, TimeSpan historicalPeriod, TimeSpan forecastPeriod);
    Task<PerformanceRegressionReport> DetectPerformanceRegression(string serviceId, TimeSpan period);
    Task<ServiceBenchmark> BenchmarkService(string serviceId, string[] compareServiceIds, TimeSpan period);
    Task<AlertEffectivenessReport> AnalyzeAlertEffectiveness(TimeSpan period);
    Task<SystemInsights> GenerateSystemInsights(TimeSpan period);
    Task<HealthTrendData> GenerateHealthTrend(TimeSpan period);
    Task<CustomAnalyticsReport> RunCustomAnalysis(CustomAnalyticsQuery query);
    Task<DataExport> ExportData(DataExportRequest request);
}

public class TimeSeriesAnalysis
{
    public string ServiceId { get; set; } = null!;
    public TimeSeriesMetric Metric { get; set; }
    public TimeSpan Period { get; set; }
    public DataPoint[] DataPoints { get; set; } = [];
    public TrendAnalysis Trend { get; set; } = new();
    public SeasonalityAnalysis Seasonality { get; set; } = new();
    public AnomalyPoint[] Anomalies { get; set; } = [];
    public ForecastPoint[] Forecast { get; set; } = [];
    public StatisticalSummary Statistics { get; set; } = new();
}

public class CorrelationAnalysis
{
    public string[] ServiceIds { get; set; } = [];
    public TimeSpan Period { get; set; }
    public CorrelationMatrix CorrelationMatrix { get; set; } = new();
    public ServiceImpact[] ImpactAnalysis { get; set; } = [];
    public CascadeFailureRisk[] CascadeRisks { get; set; } = [];
    public string[] Recommendations { get; set; } = [];
}

public class CapacityPlanningReport
{
    public string ServiceId { get; set; } = null!;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan HistoricalPeriod { get; set; }
    public TimeSpan ForecastPeriod { get; set; }
    public GrowthProjection[] Projections { get; set; } = [];
    public ResourceRequirement[] ResourceNeeds { get; set; } = [];
    public ScalingRecommendation[] ScalingRecommendations { get; set; } = [];
    public double ConfidenceLevel { get; set; }
}

public class PerformanceRegressionReport
{
    public string ServiceId { get; set; } = null!;
    public TimeSpan Period { get; set; }
    public RegressionDetection[] Regressions { get; set; } = [];
    public PerformanceBaseline Baseline { get; set; } = new();
    public PerformanceBaseline Current { get; set; } = new();
    public double RegressionScore { get; set; }
    public string[] ImpactedMetrics { get; set; } = [];
    public string[] PossibleCauses { get; set; } = [];
}

public class DataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string? Label { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class TrendAnalysis
{
    public TrendDirection Direction { get; set; }
    public double Slope { get; set; }
    public double RSquared { get; set; }
    public string Description { get; set; } = null!;
    public double Confidence { get; set; }
}

public class SeasonalityAnalysis
{
    public bool HasSeasonality { get; set; }
    public TimeSpan SeasonalPeriod { get; set; }
    public double SeasonalStrength { get; set; }
    public SeasonalPattern[] Patterns { get; set; } = [];
}

public class AnomalyPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public double ExpectedValue { get; set; }
    public double AnomalyScore { get; set; }
    public AnomalyType Type { get; set; }
    public string Description { get; set; } = null!;
}

public class ForecastPoint
{
    public DateTime Timestamp { get; set; }
    public double PredictedValue { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public double Confidence { get; set; }
}

public class StatisticalSummary
{
    public double Mean { get; set; }
    public double Median { get; set; }
    public double StandardDeviation { get; set; }
    public double Variance { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double P25 { get; set; }
    public double P75 { get; set; }
    public double P90 { get; set; }
    public double P95 { get; set; }
    public double P99 { get; set; }
    public double Skewness { get; set; }
    public double Kurtosis { get; set; }
}