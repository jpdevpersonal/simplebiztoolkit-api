using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace simplebiztoolkit_api.Dtos;

public sealed class FaqInputDto
{
    [Required]
    [MaxLength(500)]
    [JsonPropertyName("q")]
    public string Q { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("a")]
    public string A { get; set; } = string.Empty;

    [MaxLength(200)]
    [JsonPropertyName("group")]
    public string? Group { get; set; }

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; } = 0;

    [JsonPropertyName("status")]
    [RegularExpression("^(draft|published)$", ErrorMessage = "Status must be either 'draft' or 'published'.")]
    public string Status { get; set; } = "draft";
}
