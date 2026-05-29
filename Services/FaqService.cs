using Microsoft.EntityFrameworkCore;
using simplebiztoolkit_api.Data;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;

namespace simplebiztoolkit_api.Services;

public class FaqService : IFaqService
{
    private readonly SimpleBizDbContext _db;

    public FaqService(SimpleBizDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<FaqDto>> GetPublishedAsync(CancellationToken ct)
    {
        var faqs = await OrderedQuery(_db.Faqs.AsNoTracking()
                .Where(f => f.Status == "published"))
            .ToListAsync(ct);

        return faqs.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<FaqDto>> GetAllAsync(CancellationToken ct)
    {
        var faqs = await OrderedQuery(_db.Faqs.AsNoTracking()).ToListAsync(ct);
        return faqs.Select(ToDto).ToList();
    }

    public async Task<FaqDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var faq = await _db.Faqs.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, ct);
        return faq is null ? null : ToDto(faq);
    }

    public async Task<FaqDto> CreateAsync(FaqInputDto input, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var faq = new Faq
        {
            Id = Guid.NewGuid(),
            Question = (input.Q ?? string.Empty).Trim(),
            Answer = input.A ?? string.Empty,
            Group = string.IsNullOrWhiteSpace(input.Group) ? null : input.Group.Trim(),
            SortOrder = input.SortOrder,
            Status = NormalizeStatus(input.Status),
            CreatedUtc = now,
            UpdatedUtc = now
        };

        _db.Faqs.Add(faq);
        await _db.SaveChangesAsync(ct);
        return ToDto(faq);
    }

    public async Task<FaqDto?> UpdateAsync(Guid id, FaqInputDto input, CancellationToken ct)
    {
        var existing = await _db.Faqs.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (existing is null)
        {
            return null;
        }

        existing.Question = (input.Q ?? string.Empty).Trim();
        existing.Answer = input.A ?? string.Empty;
        existing.Group = string.IsNullOrWhiteSpace(input.Group) ? null : input.Group.Trim();
        existing.SortOrder = input.SortOrder;
        existing.Status = NormalizeStatus(input.Status);
        existing.UpdatedUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDto(existing);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var existing = await _db.Faqs.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (existing is null)
        {
            return false;
        }

        _db.Faqs.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static IQueryable<Faq> OrderedQuery(IQueryable<Faq> source)
        => source
            .OrderBy(f => f.Group ?? "")
            .ThenBy(f => f.SortOrder)
            .ThenBy(f => f.Id);

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "draft";
        }

        var trimmed = status.Trim().ToLowerInvariant();
        return trimmed == "published" ? "published" : "draft";
    }

    private static FaqDto ToDto(Faq faq) => new()
    {
        Id = faq.Id,
        Q = faq.Question,
        A = faq.Answer,
        Group = faq.Group,
        SortOrder = faq.SortOrder,
        Status = faq.Status
    };
}
