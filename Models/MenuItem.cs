namespace simplebiztoolkit_api.Models;

public class MenuItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "draft";
    public List<MenuCategory> Categories { get; set; } = [];
}
