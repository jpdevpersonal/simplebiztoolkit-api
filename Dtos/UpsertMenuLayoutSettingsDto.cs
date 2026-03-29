namespace simplebiztoolkit_api.Dtos;

public class UpsertMenuLayoutSettingsDto
{
    public string MenuKey { get; set; } = "primary";
    public List<string> OrderedMenuItemIds { get; set; } = [];
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    public string? UpdatedBy { get; set; }
}
