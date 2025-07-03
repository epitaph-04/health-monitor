using Microsoft.AspNetCore.Mvc;
using health_monitor.Services.Analytics;
using health_monitor.Models;

namespace health_monitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsApiController : ControllerBase
{
    private readonly IAdvancedAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsApiController> _logger;

    public AnalyticsApiController(IAdvancedAnalyticsService analyticsService, ILogger<AnalyticsApiController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Analyze time series data for a specific service and metric
    /// </summary>
    [HttpGet("services/{serviceId}/timeseries")]
    public async Task<ActionResult<TimeSeriesAnalysis>> AnalyzeTimeSeries(
        string serviceId, 
        [FromQuery] TimeSeriesMetric metric = TimeSeriesMetric.ResponseTime, 
        [FromQuery] int days = 7)
    {
        try
        {
            var analysis = await _analyticsService.AnalyzeTimeSeries(serviceId, TimeSpan.FromDays(days), metric);
            return Ok(analysis);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze time series for service {ServiceId}", serviceId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Analyze correlations between multiple services
    /// </summary>
    [HttpPost("correlations")]
    public async Task<ActionResult<CorrelationAnalysis>> AnalyzeCorrelations([FromBody] CorrelationRequest request)
    {
        try
        {
            var analysis = await _analyticsService.AnalyzeCorrelations(request.ServiceIds, TimeSpan.FromDays(request.Days));
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze correlations");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Generate capacity planning report for a service
    /// </summary>
    [HttpGet("services/{serviceId}/capacity")]
    public async Task<ActionResult<CapacityPlanningReport>> GenerateCapacityPlan(
        string serviceId, 
        [FromQuery] int historicalDays = 30, 
        [FromQuery] int forecastDays = 30)
    {
        try
        {
            var report = await _analyticsService.GenerateCapacityPlan(
                serviceId, 
                TimeSpan.FromDays(historicalDays), 
                TimeSpan.FromDays(forecastDays));
            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate capacity plan for service {ServiceId}", serviceId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Detect performance regressions for a service
    /// </summary>
    [HttpGet("services/{serviceId}/regression")]
    public async Task<ActionResult<PerformanceRegressionReport>> DetectPerformanceRegression(
        string serviceId, 
        [FromQuery] int days = 14)
    {
        try
        {
            var report = await _analyticsService.DetectPerformanceRegression(serviceId, TimeSpan.FromDays(days));
            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect regression for service {ServiceId}", serviceId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Benchmark a service against others
    /// </summary>
    [HttpPost("services/{serviceId}/benchmark")]
    public async Task<ActionResult<ServiceBenchmark>> BenchmarkService(string serviceId, [FromBody] BenchmarkRequest request)
    {
        try
        {
            var benchmark = await _analyticsService.BenchmarkService(serviceId, request.CompareServiceIds, TimeSpan.FromDays(request.Days));
            return Ok(benchmark);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to benchmark service {ServiceId}", serviceId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Analyze alert effectiveness
    /// </summary>
    [HttpGet("alerts/effectiveness")]
    public async Task<ActionResult<AlertEffectivenessReport>> AnalyzeAlertEffectiveness([FromQuery] int days = 30)
    {
        try
        {
            var report = await _analyticsService.AnalyzeAlertEffectiveness(TimeSpan.FromDays(days));
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze alert effectiveness");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Generate comprehensive system insights
    /// </summary>
    [HttpGet("insights")]
    public async Task<ActionResult<SystemInsights>> GenerateSystemInsights([FromQuery] int days = 7)
    {
        try
        {
            var insights = await _analyticsService.GenerateSystemInsights(TimeSpan.FromDays(days));
            return Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate system insights");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Run custom analytics query
    /// </summary>
    [HttpPost("custom")]
    public async Task<ActionResult<CustomAnalyticsReport>> RunCustomAnalysis([FromBody] CustomAnalyticsQuery query)
    {
        try
        {
            var report = await _analyticsService.RunCustomAnalysis(query);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run custom analysis {QueryName}", query.QueryName);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Export health check data
    /// </summary>
    [HttpPost("export")]
    public async Task<ActionResult> ExportData([FromBody] DataExportRequest request)
    {
        try
        {
            var export = await _analyticsService.ExportData(request);
            
            var contentType = request.Format switch
            {
                ExportFormat.JSON => "application/json",
                ExportFormat.CSV => "text/csv",
                ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ExportFormat.XML => "application/xml",
                _ => "application/octet-stream"
            };

            var fileName = $"health-check-data-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{request.Format.ToString().ToLower()}";
            
            return File(export.Data, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export data");
            return StatusCode(500, "Internal server error");
        }
    }
}

// Request models for API endpoints
public class CorrelationRequest
{
    public string[] ServiceIds { get; set; } = [];
    public int Days { get; set; } = 7;
}

public class BenchmarkRequest
{
    public string[] CompareServiceIds { get; set; } = [];
    public int Days { get; set; } = 30;
}