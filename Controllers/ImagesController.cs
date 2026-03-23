using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using simplebiztoolkit_api.Data;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/images")]
public class ImagesController : ApiControllerBase
{
    private const long MaxFileSizeBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly SimpleBizDbContext _db;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(SimpleBizDbContext db, IImageStorageService imageStorageService, ILogger<ImagesController> logger)
    {
        _db = db;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(CancellationToken cancellationToken)
    {
        var images = await _db.Images
            .AsNoTracking()
            .OrderByDescending(image => image.CreatedUtc)
            .ToListAsync(cancellationToken);

        return Ok(new { data = images });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var image = await _db.Images
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (image == null)
        {
            return await ErrorResponse("Image not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = image });
    }

    [HttpGet("/api/admin/images")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> GetAllAdmin(CancellationToken cancellationToken)
    {
        var images = await _db.Images
            .AsNoTracking()
            .OrderByDescending(image => image.CreatedUtc)
            .ToListAsync(cancellationToken);

        return Ok(new { data = images });
    }

    [HttpGet("/api/admin/images/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> GetAdminById(Guid id, CancellationToken cancellationToken)
    {
        var image = await _db.Images
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (image == null)
        {
            return await ErrorResponse("Image not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = image });
    }

    [HttpPost("/api/admin/images")]
    [Authorize]
    [EnableRateLimiting("admin")]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSizeBytes)]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<ActionResult> Create([FromForm] CreateImageAssetDto dto, CancellationToken cancellationToken)
    {
        var validationError = ValidateUploadedFile(dto.File);
        if (validationError is not null)
        {
            return await ErrorResponse(validationError, StatusCodes.Status400BadRequest);
        }

        UploadedImageResult? uploadedImage = null;

        try
        {
            uploadedImage = await _imageStorageService.UploadAsync(dto.File!, cancellationToken);

            var now = DateTime.UtcNow;
            var image = new ImageAsset
            {
                Id = Guid.NewGuid(),
                Url = uploadedImage.Url,
                BlobName = uploadedImage.BlobName,
                AltText = NormalizeNullable(dto.AltText),
                Caption = NormalizeNullable(dto.Caption),
                CreatedUtc = now,
                UpdatedUtc = now
            };

            _db.Images.Add(image);
            await _db.SaveChangesAsync(cancellationToken);
            return Ok(new { data = image });
        }
        catch
        {
            if (uploadedImage is not null)
            {
                await TryDeleteBlobAsync(uploadedImage.BlobName, cancellationToken);
            }

            throw;
        }
    }

    [HttpPut("/api/admin/images/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSizeBytes)]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<ActionResult> Update(Guid id, [FromForm] UpdateImageAssetDto dto, CancellationToken cancellationToken)
    {
        var image = await _db.Images.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (image == null)
        {
            return await ErrorResponse("Image not found", StatusCodes.Status404NotFound);
        }

        var validationError = ValidateUploadedFile(dto.File, requireFile: false);
        if (validationError is not null)
        {
            return await ErrorResponse(validationError, StatusCodes.Status400BadRequest);
        }

        var oldBlobName = image.BlobName;
        UploadedImageResult? replacementImage = null;

        try
        {
            if (dto.File is not null)
            {
                replacementImage = await _imageStorageService.UploadAsync(dto.File, cancellationToken);
                image.Url = replacementImage.Url;
                image.BlobName = replacementImage.BlobName;
            }

            image.AltText = NormalizeNullable(dto.AltText);
            image.Caption = NormalizeNullable(dto.Caption);
            image.UpdatedUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (replacementImage is not null)
            {
                await TryDeleteBlobAsync(replacementImage.BlobName, cancellationToken);
            }

            throw;
        }

        if (replacementImage is not null && !string.Equals(oldBlobName, replacementImage.BlobName, StringComparison.Ordinal))
        {
            await TryDeleteBlobAsync(oldBlobName, cancellationToken);
        }

        return Ok(new { data = image });
    }

    [HttpDelete("/api/admin/images/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var image = await _db.Images.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (image == null)
        {
            return await ErrorResponse("Image not found", StatusCodes.Status404NotFound);
        }

        _db.Images.Remove(image);
        await _db.SaveChangesAsync(cancellationToken);
        await TryDeleteBlobAsync(image.BlobName, cancellationToken);

        return Ok(new { success = true });
    }

    private static string? ValidateUploadedFile(IFormFile? file, bool requireFile = true)
    {
        if (file == null)
        {
            return requireFile ? "Image file is required." : null;
        }

        if (file.Length <= 0)
        {
            return "Image file is empty.";
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return "Image file must be 2MB or smaller.";
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

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
