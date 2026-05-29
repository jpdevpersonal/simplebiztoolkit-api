using System.ComponentModel.DataAnnotations;

namespace simplebiztoolkit_api.Models;

public class Faq
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Answer { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Group { get; set; }

    public int SortOrder { get; set; } = 0;

    [MaxLength(20)]
    public string Status { get; set; } = "draft";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
