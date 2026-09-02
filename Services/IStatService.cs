using simplebiztoolkit_api.Dtos;

namespace simplebiztoolkit_api.Services;

public interface IStatService
{
    bool IsSupportedName(string? name);
    Task<IReadOnlyList<StatDto>> GetVisibleAsync(CancellationToken ct);
    Task<IReadOnlyList<StatDto>> GetAllAsync(CancellationToken ct);
    Task<StatDto?> GetByNameAsync(string name, bool includeHidden, CancellationToken ct);
    Task<StatDto> UpsertAsync(string name, StatValueInputDto input, CancellationToken ct);
    Task<IReadOnlyList<StatDto>> UpsertManyAsync(IReadOnlyCollection<UpsertStatDto> inputs, CancellationToken ct);
}
