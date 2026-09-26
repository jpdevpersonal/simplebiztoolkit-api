using simplebiztoolkit_api.Models;

namespace simplebiztoolkit_api.Dtos;

public class CreateMenuItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "draft";
    public List<MenuCategory> Categories { get; set; } = [];

}
