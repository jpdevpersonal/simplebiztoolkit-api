using simplebiztoolkit_api.Dtos;

namespace simplebiztoolkit_api.Services;

public interface IFaqService
{
    Task<IReadOnlyList<FaqDto>> GetPublishedAsync(CancellationToken ct);
    Task<IReadOnlyList<FaqDto>> GetAllAsync(CancellationToken ct);
    Task<FaqDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<FaqDto> CreateAsync(FaqInputDto input, CancellationToken ct);
    Task<FaqDto?> UpdateAsync(Guid id, FaqInputDto input, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
