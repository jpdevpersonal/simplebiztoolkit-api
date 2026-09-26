using System.Text.Json.Serialization;

namespace simplebiztoolkit_api.Models;

public class MenuCategory
{
    public Guid Id { get; set; }
    public Guid MenuItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "draft";
    public string? Description { get; set; }
    public List<MenuItemPage> Pages { get; set; } = [];

    [JsonIgnore]
    public MenuItem? MenuItem { get; set; }
}
