using System.Net.Http.Json;

namespace simplebiztoolkit_api.Services;

public class RevalidationService : IRevalidationService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<RevalidationService> _logger;

    public RevalidationService(IHttpClientFactory clientFactory, IConfiguration config, ILogger<RevalidationService> logger)
    {
        _clientFactory = clientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task TriggerAsync(string type, string slug, CancellationToken cancellationToken = default)
    {
        var secret = _config["RevalidationSecret"];
        var nextJsUrl = _config["NextJsUrl"] ?? "https://www.simplebiztoolkit.com";

        if (string.IsNullOrWhiteSpace(secret))
        {
            return;
        }

        try
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Remove("X-Revalidation-Secret");
            client.DefaultRequestHeaders.Add("X-Revalidation-Secret", secret);

            await client.PostAsJsonAsync(
                $"{nextJsUrl.TrimEnd('/')}/api/revalidate",
                new { type, slug },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache revalidation failed for {Type}/{Slug}. The content change was saved successfully.", type, slug);
        }
    }
}
