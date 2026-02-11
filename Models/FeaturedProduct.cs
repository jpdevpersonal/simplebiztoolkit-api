namespace simplebiztoolkit_api.Models;

public class FeaturedProduct
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Problem { get; set; }
    public List<string> Bullets { get; set; } = [];
    public string? Image { get; set; }
    public string? EtsyUrl { get; set; }
    public string? Price { get; set; }
    public string? ProductPageUrl { get; set; }
}
