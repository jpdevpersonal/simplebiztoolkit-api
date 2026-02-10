namespace simplebiztoolkit_api.Models;

public class ProductCategory
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? HowThisHelps { get; set; }
    public string? HeroImage { get; set; }
}
