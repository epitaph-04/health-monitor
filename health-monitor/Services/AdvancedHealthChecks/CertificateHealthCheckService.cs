using health_monitor.Models;
using health_monitor.Client.Model;
using System.Diagnostics;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace health_monitor.Services.AdvancedHealthChecks;

public class CertificateHealthCheckService : IHealthCheckService
{
    private const int MaximumHistorySize = 500;
    private readonly ApplicationConfiguration _appConfig;
    private readonly ILogger<CertificateHealthCheckService> _logger;
    private HealthCheckResult _lastCheckedResult = new()
    {
        Message = "Unknown",
        ResponseTime = TimeSpan.Zero,
        Status = Status.Unknown,
        LastCheckedUtc = DateTime.UtcNow,
    };
    private readonly Queue<HealthCheckResult> _historicalHealthCheckResults = new();

    public CertificateHealthCheckService(ApplicationConfiguration appConfig, ILogger<CertificateHealthCheckService> logger)
    {
        _appConfig = appConfig;
        _logger = logger;
    }

    public string Id => _appConfig.Id;
    public string Name => _appConfig.Name;
    public ServiceType Type => ServiceType.Certificate;
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
                result.Message = "Certificate URL (Target) is missing.";
                return result;
            }

            var uri = new Uri(_appConfig.Target);
            using var client = new HttpClient();
            
            // Custom certificate validation callback to capture certificate
            X509Certificate2? serverCertificate = null;
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) =>
            {
                if (cert is X509Certificate2 cert2)
                {
                    serverCertificate = cert2;
                }
                return errors == SslPolicyErrors.None;
            };

            using var clientWithHandler = new HttpClient(handler);
            clientWithHandler.Timeout = TimeSpan.FromSeconds(_appConfig.TimeoutSeconds);

            // Make request to get certificate
            await clientWithHandler.GetAsync(_appConfig.Target);

            if (serverCertificate == null)
            {
                result.Status = Status.Critical;
                result.Message = "Could not retrieve SSL certificate.";
                return result;
            }

            // Check certificate expiration
            var daysUntilExpiry = (serverCertificate.NotAfter - DateTime.Now).TotalDays;
            var warningThreshold = 30; // 30 days
            var criticalThreshold = 7;  // 7 days

            if (daysUntilExpiry <= 0)
            {
                result.Status = Status.Critical;
                result.Message = $"Certificate has expired on {serverCertificate.NotAfter:yyyy-MM-dd}";
            }
            else if (daysUntilExpiry <= criticalThreshold)
            {
                result.Status = Status.Critical;
                result.Message = $"Certificate expires in {Math.Floor(daysUntilExpiry)} days ({serverCertificate.NotAfter:yyyy-MM-dd})";
            }
            else if (daysUntilExpiry <= warningThreshold)
            {
                result.Status = Status.Degraded;
                result.Message = $"Certificate expires in {Math.Floor(daysUntilExpiry)} days ({serverCertificate.NotAfter:yyyy-MM-dd})";
            }
            else
            {
                result.Status = Status.Healthy;
                result.Message = $"Certificate is valid, expires in {Math.Floor(daysUntilExpiry)} days ({serverCertificate.NotAfter:yyyy-MM-dd})";
            }
        }
        catch (Exception ex)
        {
            result.Status = Status.Critical;
            result.Message = $"Certificate check failed: {ex.Message}";
            _logger.LogError(ex, "Certificate health check failed for {Target}", _appConfig.Target);
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