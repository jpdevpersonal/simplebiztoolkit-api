namespace simplebiztoolkit_api.Dtos;

public sealed class StatDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public bool Hidden { get; init; }
}
