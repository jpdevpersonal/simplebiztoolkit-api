using System.Net.Http.Json;

namespace simplebiztoolkit_api.Services;

public class RevalidationService : IRevalidationService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IConfiguration _config;

    public RevalidationService(IHttpClientFactory clientFactory, IConfiguration config)
    {
        _clientFactory = clientFactory;
        _config = config;
    }

    public async Task TriggerAsync(string type, string slug, CancellationToken cancellationToken = default)
    {
        var secret = _config["RevalidationSecret"];
        var nextJsUrl = _config["NextJsUrl"] ?? "https://www.simplebiztoolkit.com";

        if (string.IsNullOrWhiteSpace(secret))
        {
            return;
        }

        var client = _clientFactory.CreateClient();
        client.DefaultRequestHeaders.Remove("X-Revalidation-Secret");
        client.DefaultRequestHeaders.Add("X-Revalidation-Secret", secret);

        await client.PostAsJsonAsync(
            $"{nextJsUrl.TrimEnd('/')}/api/revalidate",
            new { type, slug },
            cancellationToken);
    }
}
