using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using simplebiztoolkit_api.Data;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/menuitempages")]
public class MenuItemPagesController : ApiControllerBase
{
    private const long MaxSingleImageFileSizeBytes = 2 * 1024 * 1024;
    private const long MaxRequestBodySizeBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly SimpleBizDbContext _db;
    private readonly IMenuStore _store;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<MenuItemPagesController> _logger;

    public MenuItemPagesController(
        SimpleBizDbContext db,
        IMenuStore store,
        IImageStorageService imageStorageService,
        ILogger<MenuItemPagesController> logger)
    {
        _db = db;
        _store = store;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] Guid? menuCategoryId, [FromQuery] string? status, [FromQuery] Guid? menuItemId)
    {
        if (!string.Equals(status, "published", StringComparison.OrdinalIgnoreCase))
        {
            status = "published";
        }

        var pages = await _store.GetMenuItemPagesAsync(menuCategoryId: menuCategoryId, status: status, menuItemId: menuItemId);
        return Ok(new { data = pages });
    }

    [HttpGet("/api/admin/pages")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> GetAllAdmin([FromQuery] Guid? menuCategoryId, [FromQuery] string? status, [FromQuery] Guid? menuItemId)
    {
        var pages = await _store.GetMenuItemPagesAsync(menuCategoryId: menuCategoryId, status: status, menuItemId: menuItemId);
        return Ok(new { data = pages });
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult> GetBySlug(string slug)
    {
        var page = await _store.GetMenuItemPageBySlugAsync(slug);
        var isAuthenticated = User?.Identity?.IsAuthenticated == true;

        if (page == null || (!isAuthenticated && !string.Equals(page.Status, "published", StringComparison.OrdinalIgnoreCase)))
        {
            return await ErrorResponse("Page not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = page });
    }

    [HttpGet("/api/admin/pages/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var page = await _store.GetMenuItemPageByIdAsync(id);
        if (page == null)
        {
            return await ErrorResponse("Page not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = page });
    }

    [HttpPost("/api/admin/pages")]
    [Authorize]
    [EnableRateLimiting("admin")]
    [Consumes("application/json")]
    public async Task<ActionResult> Create([FromBody] CreateMenuItemPageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Title))
        {
            return await ErrorResponse("Slug and title are required.", StatusCodes.Status400BadRequest);
        }

        var page = await _store.AddMenuItemPageAsync(dto);
        return Ok(new { data = page });
    }

    [HttpPut("/api/admin/pages/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    [Consumes("application/json")]
    public async Task<ActionResult> Update(Guid id, [FromBody] CreateMenuItemPageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Title))
        {
            return await ErrorResponse("Slug and title are required.", StatusCodes.Status400BadRequest);
        }

        var page = await _store.UpdateMenuItemPageAsync(id, dto);
        if (page == null)
        {
            return await ErrorResponse("Page not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = page });
    }

    [HttpPost("/api/admin/pages")]
    [Authorize]
    [EnableRateLimiting("admin")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBodySizeBytes)]
    [RequestSizeLimit(MaxRequestBodySizeBytes)]
    public async Task<ActionResult> CreateWithImages([FromForm] CreateMenuItemPageFormDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Title))
        {
            return await ErrorResponse("Slug and title are required.", StatusCodes.Status400BadRequest);
        }

        var featuredImageValidation = ValidateUploadedFile(dto.FeaturedImageFile, requireFile: false);
        if (featuredImageValidation is not null)
        {
            return await ErrorResponse(featuredImageValidation, StatusCodes.Status400BadRequest);
        }

        var headerImageValidation = ValidateUploadedFile(dto.HeaderImageFile, requireFile: false);
        if (headerImageValidation is not null)
        {
            return await ErrorResponse(headerImageValidation, StatusCodes.Status400BadRequest);
        }

        var pageDto = MapToCreateDto(dto);
        var uploadedBlobNames = new List<string>();

        try
        {
            if (dto.FeaturedImageFile is not null)
            {
                var featuredImageAsset = await SaveImageAssetAsync(dto.FeaturedImageFile, cancellationToken);
                pageDto.FeaturedImageId = featuredImageAsset.Id;
                uploadedBlobNames.Add(featuredImageAsset.BlobName);
            }

            if (dto.HeaderImageFile is not null)
            {
                var headerImageAsset = await SaveImageAssetAsync(dto.HeaderImageFile, cancellationToken);
                pageDto.HeaderImageId = headerImageAsset.Id;
                uploadedBlobNames.Add(headerImageAsset.BlobName);
            }

            var page = await _store.AddMenuItemPageAsync(pageDto);
            return Ok(new { data = page });
        }
        catch
        {
            foreach (var blobName in uploadedBlobNames)
            {
                await TryDeleteBlobAsync(blobName, cancellationToken);
            }

            throw;
        }
    }

    [HttpPut("/api/admin/pages/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBodySizeBytes)]
    [RequestSizeLimit(MaxRequestBodySizeBytes)]
    public async Task<ActionResult> UpdateWithImages(Guid id, [FromForm] CreateMenuItemPageFormDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Title))
        {
            return await ErrorResponse("Slug and title are required.", StatusCodes.Status400BadRequest);
        }

        var featuredImageValidation = ValidateUploadedFile(dto.FeaturedImageFile, requireFile: false);
        if (featuredImageValidation is not null)
        {
            return await ErrorResponse(featuredImageValidation, StatusCodes.Status400BadRequest);
        }

        var headerImageValidation = ValidateUploadedFile(dto.HeaderImageFile, requireFile: false);
        if (headerImageValidation is not null)
        {
            return await ErrorResponse(headerImageValidation, StatusCodes.Status400BadRequest);
        }

        var pageDto = MapToCreateDto(dto);
        var uploadedBlobNames = new List<string>();

        try
        {
            if (dto.FeaturedImageFile is not null)
            {
                var featuredImageAsset = await SaveImageAssetAsync(dto.FeaturedImageFile, cancellationToken);
                pageDto.FeaturedImageId = featuredImageAsset.Id;
                uploadedBlobNames.Add(featuredImageAsset.BlobName);
            }

            if (dto.HeaderImageFile is not null)
            {
                var headerImageAsset = await SaveImageAssetAsync(dto.HeaderImageFile, cancellationToken);
                pageDto.HeaderImageId = headerImageAsset.Id;
                uploadedBlobNames.Add(headerImageAsset.BlobName);
            }

            var page = await _store.UpdateMenuItemPageAsync(id, pageDto);
            if (page == null)
            {
                return await ErrorResponse("Page not found", StatusCodes.Status404NotFound);
            }

            return Ok(new { data = page });
        }
        catch
        {
            foreach (var blobName in uploadedBlobNames)
            {
                await TryDeleteBlobAsync(blobName, cancellationToken);
            }

            throw;
        }
    }

    [HttpDelete("/api/admin/pages/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var removed = await _store.DeleteMenuItemPageAsync(id);
        if (!removed)
        {
            return await ErrorResponse("Page not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { success = true });
    }

    private CreateMenuItemPageDto MapToCreateDto(CreateMenuItemPageFormDto dto)
        => new()
        {
            Id = dto.Id,
            MenuCategoryId = dto.MenuCategoryId,
            MenuItemId = dto.MenuItemId,
            Slug = dto.Slug,
            Title = dto.Title,
            Subtitle = dto.Subtitle,
            Description = dto.Description,
            Content = dto.Content,
            DateISO = dto.DateISO,
            DateModified = dto.DateModified,
            FeaturedImageId = dto.FeaturedImageId,
            HeaderImageId = dto.HeaderImageId,
            Status = dto.Status,
            SeoTitle = dto.SeoTitle,
            SeoDescription = dto.SeoDescription,
            OgImage = dto.OgImage,
            CanonicalUrl = dto.CanonicalUrl
        };

    private async Task<ImageAsset> SaveImageAssetAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var uploadedImage = await _imageStorageService.UploadAsync(file, cancellationToken);
        var imageAsset = new ImageAsset
        {
            Id = Guid.NewGuid(),
            Url = uploadedImage.Url,
            BlobName = uploadedImage.BlobName,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };

        _db.Images.Add(imageAsset);

        return imageAsset;
    }

    private static string? ValidateUploadedFile(IFormFile? file, bool requireFile)
    {
        if (file is null)
        {
            return requireFile ? "Image file is required." : null;
        }

        if (file.Length <= 0)
        {
            return "Image file is empty.";
        }

        if (file.Length > MaxSingleImageFileSizeBytes)
        {
            return "Each image file must be 2MB or smaller.";
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return "Only image/jpeg, image/png, and image/webp files are allowed.";
        }

        if (!HasValidSignature(file))
        {
            return "The uploaded file content does not match a supported image format.";
        }

        return null;
    }

    private static bool HasValidSignature(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        Span<byte> header = stackalloc byte[12];
        var bytesRead = stream.Read(header);

        if (bytesRead < 3)
        {
            return false;
        }

        return file.ContentType switch
        {
            "image/jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "image/png" => bytesRead >= 8
                && header[0] == 0x89
                && header[1] == 0x50
                && header[2] == 0x4E
                && header[3] == 0x47
                && header[4] == 0x0D
                && header[5] == 0x0A
                && header[6] == 0x1A
                && header[7] == 0x0A,
            "image/webp" => bytesRead >= 12
                && header[0] == 0x52
                && header[1] == 0x49
                && header[2] == 0x46
                && header[3] == 0x46
                && header[8] == 0x57
                && header[9] == 0x45
                && header[10] == 0x42
                && header[11] == 0x50,
            _ => false
        };
    }

    private async Task TryDeleteBlobAsync(string blobName, CancellationToken cancellationToken)
    {
        try
        {
            await _imageStorageService.DeleteAsync(blobName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete blob {BlobName}", blobName);
        }
    }
}
