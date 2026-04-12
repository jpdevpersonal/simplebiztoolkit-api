using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace simplebiztoolkit_api.Services;

public class RevalidationService : IRevalidationService
{
    private const string DefaultRevalidationUrl = "https://www.simplebiztoolkit.com/api/revalidate";

    private readonly IHttpClientFactory _clientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<RevalidationService> _logger;

    public RevalidationService(
        IHttpClientFactory clientFactory,
        IConfiguration config,
        ILogger<RevalidationService> logger)
    {
        _clientFactory = clientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task RevalidatePathsAsync(IEnumerable<string> paths)
    {
        var normalizedPaths = paths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(NormalizePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedPaths.Length == 0)
        {
            return;
        }

        var url = _config["Revalidation:Url"];
        if (string.IsNullOrWhiteSpace(url))
        {
            var nextJsUrl = _config["NextJsUrl"];
            url = string.IsNullOrWhiteSpace(nextJsUrl)
                ? DefaultRevalidationUrl
                : $"{nextJsUrl.TrimEnd('/')}/api/revalidate";
        }

        var secret = _config["Revalidation:Secret"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            secret = _config["RevalidationSecret"];
        }

        if (string.IsNullOrWhiteSpace(secret))
        {
            _logger.LogWarning("Skipping Next.js revalidation because the revalidation secret is not configured.");
            return;
        }

        _logger.LogInformation("Triggering Next.js revalidation for paths: {Paths}", normalizedPaths);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { paths = normalizedPaths }),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json)
            };

            request.Headers.Add("x-revalidate-secret", secret);

            var client = _clientFactory.CreateClient();
            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Next.js revalidation completed for paths: {Paths}", normalizedPaths);
                return;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "Next.js revalidation failed with status code {StatusCode} for paths {Paths}. Response: {ResponseBody}",
                (int)response.StatusCode,
                normalizedPaths,
                responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Next.js revalidation failed for paths: {Paths}", normalizedPaths);
        }
    }

    private static string NormalizePath(string path)
    {
        var trimmedPath = path.Trim();

        if (trimmedPath == "/")
        {
            return trimmedPath;
        }

        trimmedPath = trimmedPath.Trim('/');
        return $"/{trimmedPath}";
    }
}
