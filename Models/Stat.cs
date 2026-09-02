namespace simplebiztoolkit_api.Models;

public sealed class Stat
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool? Hidden { get; set; }
}
