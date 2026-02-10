namespace simplebiztoolkit_api.Dtos;

public class CreateCategoryDto
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? HowThisHelps { get; set; }
    public string? HeroImage { get; set; }
}
