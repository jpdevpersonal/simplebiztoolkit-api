using Microsoft.AspNetCore.Http;

namespace simplebiztoolkit_api.Dtos;

public class CreateImageAssetDto
{
    public IFormFile? File { get; set; }
    public string? AltText { get; set; }
    public string? Caption { get; set; }
}
