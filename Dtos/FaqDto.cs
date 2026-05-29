using System.Text.Json.Serialization;

namespace simplebiztoolkit_api.Dtos;

public sealed class FaqDto
{
    public Guid Id { get; init; }

    [JsonPropertyName("q")]
    public string Q { get; init; } = string.Empty;

    [JsonPropertyName("a")]
    public string A { get; init; } = string.Empty;

    [JsonPropertyName("group")]
    public string? Group { get; init; }

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "draft";
}
