using Microsoft.AspNetCore.Http;

namespace simplebiztoolkit_api.Services;

public interface IImageStorageService
{
    Task<UploadedImageResult> UploadAsync(IFormFile file, CancellationToken cancellationToken);
    Task DeleteAsync(string blobName, CancellationToken cancellationToken);
}
