using Microsoft.EntityFrameworkCore;
using simplebiztoolkit_api.Data;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;

namespace simplebiztoolkit_api.Services;

public sealed class StatService : IStatService
{
    private static readonly string[] SupportedNames = ["rating", "reviews", "sales", "star-seller"];
    private static readonly HashSet<string> SupportedNameSet = new(SupportedNames, StringComparer.OrdinalIgnoreCase);
    private readonly SimpleBizDbContext _db;

    public StatService(SimpleBizDbContext db)
    {
        _db = db;
    }

    public bool IsSupportedName(string? name)
        => !string.IsNullOrWhiteSpace(name) && SupportedNameSet.Contains(name.Trim());

    public async Task<IReadOnlyList<StatDto>> GetVisibleAsync(CancellationToken ct)
    {
        var stats = await _db.Stats.AsNoTracking()
            .Where(stat => stat.Hidden != true)
            .ToListAsync(ct);

        return ToOrderedDtos(stats);
    }

    public async Task<IReadOnlyList<StatDto>> GetAllAsync(CancellationToken ct)
    {
        var stats = await _db.Stats.AsNoTracking().ToListAsync(ct);
        return ToOrderedDtos(stats);
    }

    public async Task<StatDto?> GetByNameAsync(string name, bool includeHidden, CancellationToken ct)
    {
        var normalizedName = NormalizeName(name);
        var stat = await _db.Stats.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Name == normalizedName && (includeHidden || item.Hidden != true), ct);

        return stat is null ? null : ToDto(stat);
    }

    public async Task<StatDto> UpsertAsync(string name, StatValueInputDto input, CancellationToken ct)
    {
        var normalizedName = NormalizeName(name);
        var stat = await _db.Stats.FirstOrDefaultAsync(item => item.Name == normalizedName, ct);

        if (stat is null)
        {
            stat = new Stat { Name = normalizedName };
            _db.Stats.Add(stat);
        }

        stat.Value = input.Value;
        stat.Hidden = input.Hidden;
        await _db.SaveChangesAsync(ct);

        return ToDto(stat);
    }

    public async Task<IReadOnlyList<StatDto>> UpsertManyAsync(
        IReadOnlyCollection<UpsertStatDto> inputs,
        CancellationToken ct)
    {
        var normalizedInputs = inputs
            .Select(input => new
            {
                Name = NormalizeName(input.Name),
                input.Value,
                input.Hidden
            })
            .ToList();

        if (normalizedInputs.Select(input => input.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            != normalizedInputs.Count)
        {
            throw new InvalidOperationException("Each stat name can only appear once per request.");
        }

        var names = normalizedInputs.Select(input => input.Name).ToList();
        var existingByName = await _db.Stats
            .Where(stat => names.Contains(stat.Name))
            .ToDictionaryAsync(stat => stat.Name, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var input in normalizedInputs)
        {
            if (!existingByName.TryGetValue(input.Name, out var stat))
            {
                stat = new Stat { Name = input.Name };
                _db.Stats.Add(stat);
                existingByName.Add(input.Name, stat);
            }

            stat.Value = input.Value;
            stat.Hidden = input.Hidden;
        }

        await _db.SaveChangesAsync(ct);
        return ToOrderedDtos(existingByName.Values);
    }

    private static string NormalizeName(string name)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        if (!SupportedNameSet.Contains(normalizedName))
        {
            throw new InvalidOperationException($"Unsupported stat name: {name}");
        }

        return normalizedName;
    }

    private static IReadOnlyList<StatDto> ToOrderedDtos(IEnumerable<Stat> stats)
        => stats
            .OrderBy(stat => Array.IndexOf(SupportedNames, stat.Name))
            .Select(ToDto)
            .ToList();

    private static StatDto ToDto(Stat stat) => new()
    {
        Id = stat.Id,
        Name = stat.Name,
        Value = stat.Value,
        Hidden = stat.Hidden ?? false
    };
}
