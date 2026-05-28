using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace simplebiztoolkit_api.Models;

public class MenuItemPage
{
    public Guid Id { get; set; }
    public Guid? MenuCategoryId { get; set; }
    public Guid? MenuItemId { get; set; }

    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public DateOnly DateISO { get; set; }
    public DateTime DateModified { get; set; }
    public Guid? FeaturedImageId { get; set; }
    public Guid? HeaderImageId { get; set; }
    public string Status { get; set; } = "draft";
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? OgImage { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool ShowLastUpdated { get; set; } = true;

    [NotMapped]
    public string? FeaturedImage => FeaturedImageAsset?.Url;

    [NotMapped]
    public string? HeaderImage => HeaderImageAsset?.Url;

    [JsonIgnore]
    public MenuCategory? MenuCategory { get; set; }

    [JsonIgnore]
    public MenuItem? MenuItem { get; set; }

    [JsonIgnore]
    public ImageAsset? FeaturedImageAsset { get; set; }

    [JsonIgnore]
    public ImageAsset? HeaderImageAsset { get; set; }
}
