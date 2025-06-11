using health_monitor.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace health_monitor.Services;

public static class ConfigurationService
{

    public static async Task ConfigureHealthCheckService(this IServiceCollection services, IWebHostEnvironment env, string configFileName)
    {
        try
        {
            var configFilePath = Path.Combine(env.ContentRootPath, configFileName);
            if (File.Exists(configFilePath))
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    Converters = { new JsonStringEnumConverter() }
                };
                await using var stream = File.OpenRead(configFilePath);
                await foreach (var application in JsonSerializer.DeserializeAsyncEnumerable<ApplicationConfiguration>(stream, jsonOptions))
                {
                    Console.WriteLine($"App name (type): {application!.Name} ({application!.Type}).");
                    services.AddKeyedSingleton<IHealthCheckService>(application!.Name, (provider, o) =>
                    {
                        return application.Type switch
                        {
                            ApplicationType.Http => new HttpHealthCheckService(
                                provider.GetRequiredService<HttpClient>(), application),
                            ApplicationType.Db => new DbHealthCheckService(application),
                            ApplicationType.Sns => new SnsHealthCheckService(application),
                            ApplicationType.Sqs => new SqsHealthCheckService(application),
                            ApplicationType.Rabbitmq => new RabbitmqHealthCheckService(application),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    });
                }
            }
            else
            {
                Console.WriteLine($"Error: Configuration file '{configFilePath}' not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading or parsing configuration: {ex.Message}");
        }
    }
}