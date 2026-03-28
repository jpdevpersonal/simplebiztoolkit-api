using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace simplebiztoolkit_api.Services;

public class AzureBlobImageStorageService : IImageStorageService
{
    private static readonly IReadOnlyDictionary<string, string> AllowedContentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    private readonly string _connectionString;
    private readonly string _containerName;
    private readonly ILogger<AzureBlobImageStorageService> _logger;
    private readonly SemaphoreSlim _containerLock = new(1, 1);
    private BlobContainerClient? _containerClient;
    private bool _containerReady;

    public AzureBlobImageStorageService(IOptions<AzureBlobStorageOptions> options, ILogger<AzureBlobImageStorageService> logger)
    {
        var settings = options.Value;
        _connectionString = settings.ConnectionString?.Trim() ?? string.Empty;
        _containerName = settings.ContainerName?.Trim().ToLowerInvariant() ?? string.Empty;
        _logger = logger;
    }

    public async Task<UploadedImageResult> UploadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        await EnsureContainerReadyAsync(cancellationToken);

        if (!AllowedContentTypes.TryGetValue(file.ContentType, out var extension))
        {
            throw new InvalidOperationException("Unsupported image content type.");
        }

        var blobName = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{extension}";
        var blobClient = _containerClient!.GetBlobClient(blobName);
        await using var stream = file.OpenReadStream();

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType,
                CacheControl = "public, max-age=31536000"
            },
            TransferValidation = new UploadTransferValidationOptions
            {
                ChecksumAlgorithm = StorageChecksumAlgorithm.Auto
            }
        };

        await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);
        _logger.LogInformation("Uploaded image blob {BlobName}", blobName);

        return new UploadedImageResult(blobName, blobClient.Uri.ToString());
    }

    public async Task DeleteAsync(string blobName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(blobName))
        {
            return;
        }

        await EnsureContainerReadyAsync(cancellationToken);

        var blobClient = _containerClient!.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        _logger.LogInformation("Deleted image blob {BlobName}", blobName);
    }

    private async Task EnsureContainerReadyAsync(CancellationToken cancellationToken)
    {
        if (_containerReady)
        {
            return;
        }

        await _containerLock.WaitAsync(cancellationToken);
        try
        {
            if (_containerReady)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException("Azure Blob Storage connection string is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_containerName))
            {
                throw new InvalidOperationException("Azure Blob Storage container name is not configured.");
            }

            _containerClient ??= CreateContainerClient();

            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

            var properties = await _containerClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            if (properties.Value.PublicAccess != PublicAccessType.Blob)
            {
                await _containerClient.SetAccessPolicyAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
            }

            _containerReady = true;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to initialize Azure Blob container {ContainerName}", _containerClient.Name);
            throw;
        }
        finally
        {
            _containerLock.Release();
        }
    }

    private BlobContainerClient CreateContainerClient()
    {
        var blobClientOptions = new BlobClientOptions
        {
            Retry =
            {
                Delay = TimeSpan.FromSeconds(1),
                MaxDelay = TimeSpan.FromSeconds(8),
                MaxRetries = 3,
                Mode = RetryMode.Exponential
            }
        };

        var blobServiceClient = new BlobServiceClient(_connectionString, blobClientOptions);
        return blobServiceClient.GetBlobContainerClient(_containerName);
    }
}
