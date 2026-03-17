namespace simplebiztoolkit_api.Services;

public interface IRevalidationService
{
    Task TriggerAsync(string type, string slug, CancellationToken cancellationToken = default);
}
