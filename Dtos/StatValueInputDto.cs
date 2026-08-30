using System.ComponentModel.DataAnnotations;

namespace simplebiztoolkit_api.Dtos;

public sealed class StatValueInputDto
{
    [Required]
    [MaxLength(10)]
    public string Value { get; set; } = string.Empty;

    public bool Hidden { get; set; }
}
