using System.ComponentModel.DataAnnotations;

namespace simplebiztoolkit_api.Dtos;

public sealed class UpsertStatDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Value { get; set; } = string.Empty;

    public bool Hidden { get; set; }
}
