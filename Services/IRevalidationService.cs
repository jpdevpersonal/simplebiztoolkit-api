namespace simplebiztoolkit_api.Services;

public interface IRevalidationService
{
    Task RevalidatePathsAsync(IEnumerable<string> paths);
}
