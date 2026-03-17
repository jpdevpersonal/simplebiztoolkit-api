namespace simplebiztoolkit_api.Dtos;

public class CreateMenuCategoryDto
{
    public Guid MenuItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "draft";
}
