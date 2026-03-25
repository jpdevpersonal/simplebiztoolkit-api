namespace simplebiztoolkit_api.Models;

public class ImageAsset
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string BlobName { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
