using Microsoft.AspNetCore.Mvc;
using health_monitor.Services;
using health_monitor.Services.Metrics;
using health_monitor.Services.Alerting;
using health_monitor.Services.Dependencies;
using health_monitor.Models;

namespace health_monitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthCheckApiController : ControllerBase
{
    private readonly StatusService _statusService;
    private readonly IMetricsService _metricsService;
    private readonly IAlertingService _alertingService;
    private readonly IDependencyService _dependencyService;
    private readonly IEnumerable<IHealthCheckService> _healthCheckServices;
    private readonly ILogger<HealthCheckApiController> _logger;

    public HealthCheckApiController(
        StatusService statusService,
        IMetricsService metricsService,
        IAlertingService alertingService,
        IDependencyService dependencyService,
        IEnumerable<IHealthCheckService> healthCheckServices,
        ILogger<HealthCheckApiController> logger)
    {
        _statusService = statusService;
        _metricsService = metricsService;
        _alertingService = alertingService;
        _dependencyService = dependencyService;
        _healthCheckServices = healthCheckServices;
        _logger = logger;
    }

    /// <summary>
    /// Get all services and their current status
    /// </summary>
    [HttpGet("services")]
    public ActionResult<Service[]> GetServices()
    {
        try
        {
            var services = _statusService.GetServices();
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve services");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get specific service status by ID
    /// </summary>
    [HttpGet("services/{id}/status")]
    public ActionResult<Service> GetServiceStatus(string id)
    {
        try
        {
            var services = _statusService.GetServices();
            var service = services.FirstOrDefault(s => s.Id == id);
            
            if (service == null)
            {
                return NotFound($"Service with ID '{id}' not found");
            }

            return Ok(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve service status for {ServiceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get health metrics for a service
    /// </summary>
    [HttpGet("services/{id}/metrics")]
    public async Task<ActionResult<HealthMetrics>> GetServiceMetrics(string id, [FromQuery] int days = 30)
    {
        try
        {
            var period = TimeSpan.FromDays(days);
            var metrics = await _metricsService.CalculateMetrics(id, period);
            return Ok(metrics);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate metrics for service {ServiceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get SLA report for a service
    /// </summary>
    [HttpGet("services/{id}/sla")]
    public async Task<ActionResult<SlaReport>> GetServiceSla(string id, [FromQuery] SlaReportingPeriod period = SlaReportingPeriod.Monthly)
    {
        try
        {
            var report = await _metricsService.GenerateSlaReport(id, period);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SLA report for service {ServiceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get service dependencies
    /// </summary>
    [HttpGet("services/{id}/dependencies")]
    public async Task<ActionResult<string[]>> GetServiceDependencies(string id)
    {
        try
        {
            var dependencies = await _dependencyService.GetDependenciesOf(id);
            return Ok(dependencies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve dependencies for service {ServiceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get services that depend on this service
    /// </summary>
    [HttpGet("services/{id}/dependents")]
    public async Task<ActionResult<string[]>> GetServiceDependents(string id)
    {
        try
        {
            var dependents = await _dependencyService.GetDependentsOf(id);
            return Ok(dependents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve dependents for service {ServiceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Trigger a manual health check for a service
    /// </summary>
    [HttpPost("services/{id}/check")]
    public async Task<ActionResult<HealthCheckResult>> TriggerHealthCheck(string id)
    {
        try
        {
            var service = _healthCheckServices.FirstOrDefault(s => s.Id == id);
            if (service == null)
            {
                return NotFound($"Service with ID '{id}' not found");
            }

            var result = await service.CheckHealthAsync();
            _logger.LogInformation("Manual health check triggered for service {ServiceId}", id);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger health check for service {ServiceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get overall system health summary
    /// </summary>
    [HttpGet("health/summary")]
    public ActionResult<object> GetHealthSummary()
    {
        try
        {
            var services = _statusService.GetServices();
            var summary = new
            {
                TotalServices = services.Length,
                HealthyServices = services.Count(s => s.LastCheckStatus.Status == Status.Healthy),
                DegradedServices = services.Count(s => s.LastCheckStatus.Status == Status.Degraded),
                CriticalServices = services.Count(s => s.LastCheckStatus.Status == Status.Critical),
                UnknownServices = services.Count(s => s.LastCheckStatus.Status == Status.Unknown),
                OverallStatus = GetOverallStatus(services),
                LastUpdated = DateTime.UtcNow
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate health summary");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get alert rules configuration
    /// </summary>
    [HttpGet("alerts/rules")]
    public async Task<ActionResult<AlertRule[]>> GetAlertRules()
    {
        try
        {
            var rules = await _alertingService.GetAlertRules();
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve alert rules");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Configure alert rules
    /// </summary>
    [HttpPost("alerts/rules")]
    public async Task<ActionResult> ConfigureAlertRules([FromBody] AlertRule[] rules)
    {
        try
        {
            await _alertingService.ConfigureAlertRules(rules);
            _logger.LogInformation("Alert rules configured: {Count} rules", rules.Length);
            return Ok(new { message = "Alert rules configured successfully", count = rules.Length });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure alert rules");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get dependency graph
    /// </summary>
    [HttpGet("dependencies/graph")]
    public async Task<ActionResult<DependencyGraph>> GetDependencyGraph()
    {
        try
        {
            var graph = await _dependencyService.BuildDependencyGraph();
            return Ok(graph);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build dependency graph");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Find root cause of service failure
    /// </summary>
    [HttpGet("services/{id}/root-cause")]
    public async Task<ActionResult<string[]>> FindRootCause(string id)
    {
        try
        {
            var rootCauses = await _dependencyService.FindRootCause(id);
            return Ok(rootCauses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find root cause for service {ServiceId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private Status GetOverallStatus(Service[] services)
    {
        if (services.Any(s => s.LastCheckStatus.Status == Status.Critical))
            return Status.Critical;
        if (services.Any(s => s.LastCheckStatus.Status == Status.Degraded))
            return Status.Degraded;
        if (services.Any(s => s.LastCheckStatus.Status == Status.Unknown))
            return Status.Unknown;
        return Status.Healthy;
    }
}