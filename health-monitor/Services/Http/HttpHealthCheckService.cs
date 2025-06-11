using health_monitor.Models;
using System.Diagnostics;
using System.Text;
using health_monitor.Client.Model;

namespace health_monitor.Services
{
    public class HttpHealthCheckService : IHealthCheckService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationConfiguration _appConfig;
        private readonly ILogger<HttpHealthCheckService> _logger;
        private HealthCheckResult _lastCheckedResult = new()
        {
            Message = "Unknown",
            ResponseTime = TimeSpan.Zero,
            Status = Status.Unknown,
            LastCheckedUtc = DateTime.UtcNow,
        };
        private readonly Queue<HealthCheckResult> _historicalHealthCheckResults = new(10);

        public HttpHealthCheckService(HttpClient httpClient, ApplicationConfiguration appConfig, ILogger<HttpHealthCheckService> logger)
        {
            _httpClient = httpClient;
            _appConfig = appConfig;
            _logger = logger;
            
            _httpClient.Timeout = TimeSpan.FromSeconds(appConfig.TimeoutSeconds);
        }

        public string Id => _appConfig.Id;
        public string Name => _appConfig.Name;
        public ServiceType Type => ServiceType.Http;
        public string Target => _appConfig.Target;
        public HealthCheckResult LastCheckedResult => _lastCheckedResult;

        public async Task<HealthCheckResult> CheckHealthAsync()
        {
            var result = new HealthCheckResult();
            var stopwatch = new Stopwatch();

            try
            {
                var request = new HttpRequestMessage(new HttpMethod(_appConfig.Method), _appConfig.Target);
                if (_appConfig.Headers != null)
                {
                    foreach (var header in _appConfig.Headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                if (!string.IsNullOrEmpty(_appConfig.RequestBody) && (_appConfig.Method.ToUpper() == "POST" || _appConfig.Method.ToUpper() == "PUT"))
                {
                    string contentType = "application/json";
                    if(_appConfig.Headers != null && _appConfig.Headers.TryGetValue("Content-Type", out var ctHeader))
                    {
                        contentType = ctHeader;
                    }
                    request.Content = new StringContent(_appConfig.RequestBody, Encoding.UTF8, contentType);
                }

                stopwatch.Start();
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                stopwatch.Stop();

                result.ResponseTime = stopwatch.Elapsed;
                result.Message = "Ok";
                result.Status = response.StatusCode == (System.Net.HttpStatusCode)_appConfig.ExpectedResponseCode
                                ? Status.Healthy
                                : Status.Critical;
                if (result.Status == Status.Critical)
                {
                    result.Message = $"Unexpected status code: {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}";
                }
            }
            catch (TaskCanceledException ex)
            {
                stopwatch.Stop();
                result.Status = Status.Critical;
                result.Message = $"Request timed out after {_appConfig.TimeoutSeconds} seconds. {ex.Message}";
                result.ResponseTime = stopwatch.Elapsed;
                _logger.LogError("Request timed out, {error}", ex);
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                result.Status = Status.Critical;
                result.Message = $"HTTP request failed: {ex.Message}";
                if(stopwatch.IsRunning) stopwatch.Stop();
                result.ResponseTime = stopwatch.Elapsed;
                _logger.LogError("HTTP request failed, {error}", ex);
            }
            catch (Exception ex)
            {
                if(stopwatch.IsRunning) stopwatch.Stop();
                result.Status = Status.Critical;
                result.Message = $"An unexpected error occurred: {ex.Message}";
                result.ResponseTime = stopwatch.Elapsed;
                _logger.LogError("An unexpected error occurred, {error}", ex);
            }
            finally
            {
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
}
