namespace simplebiztoolkit_api.Models;

public class Article
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public DateOnly DateISO { get; set; }
    public DateOnly DateModified { get; set; }
    public string? Category { get; set; }
    public int ReadingMinutes { get; set; }
    public List<string> Badges { get; set; } = [];
    public string? FeaturedImage { get; set; }
    public string? HeaderImage { get; set; }
    public string Status { get; set; } = "draft";
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? OgImage { get; set; }
    public string? CanonicalUrl { get; set; }
}
