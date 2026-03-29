namespace simplebiztoolkit_api.Dtos;

public class MenuLayoutSettingsDto
{
    public string MenuKey { get; set; } = "primary";
    public List<string> OrderedMenuItemIds { get; set; } = [];
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
