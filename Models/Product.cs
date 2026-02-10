namespace simplebiztoolkit_api.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Problem { get; set; }
    public string? Description { get; set; }
    public List<string> Bullets { get; set; } = [];
    public string? Image { get; set; }
    public string? EtsyUrl { get; set; }
    public string? Price { get; set; }
    public Guid CategoryId { get; set; }
    public string Status { get; set; } = "draft";
}
