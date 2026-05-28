using Microsoft.EntityFrameworkCore;
using simplebiztoolkit_api.Data;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;
using simplebiztoolkit_api.Services;
using Xunit;

namespace simplebiztoolkit_api.Tests;

public class FaqServiceTests
{
    [Fact]
    public async Task GetPublishedAsync_FiltersOutDrafts_AndOrdersByGroupThenSortOrder()
    {
        await using var db = CreateDbContext();

        db.Faqs.AddRange(
            new Faq { Id = Guid.NewGuid(), Question = "Draft only", Answer = "x", Group = "billing", SortOrder = 1, Status = "draft" },
            new Faq { Id = Guid.NewGuid(), Question = "Z published", Answer = "z", Group = "shipping", SortOrder = 0, Status = "published" },
            new Faq { Id = Guid.NewGuid(), Question = "A published", Answer = "a", Group = "billing", SortOrder = 2, Status = "published" },
            new Faq { Id = Guid.NewGuid(), Question = "B published", Answer = "b", Group = "billing", SortOrder = 1, Status = "published" },
            new Faq { Id = Guid.NewGuid(), Question = "No group", Answer = "n", Group = null, SortOrder = 5, Status = "published" });

        await db.SaveChangesAsync();

        var service = new FaqService(db);

        var result = await service.GetPublishedAsync(CancellationToken.None);

        // Drafts excluded.
        Assert.DoesNotContain(result, f => f.Q == "Draft only");

        // Ordering: Group asc (null/"" first), then SortOrder asc.
        Assert.Equal(4, result.Count);
        Assert.Equal("n", result[0].A); // null group sorts first ("" )
        Assert.Equal("b", result[1].A); // billing, sortOrder 1
        Assert.Equal("a", result[2].A); // billing, sortOrder 2
        Assert.Equal("z", result[3].A); // shipping, sortOrder 0
    }

    [Fact]
    public async Task CreateAsync_SetsUpdatedUtc_AndTrimsQuestionAndGroup()
    {
        await using var db = CreateDbContext();
        var service = new FaqService(db);

        var before = DateTime.UtcNow.AddSeconds(-1);
        var created = await service.CreateAsync(new FaqInputDto
        {
            Q = "  How do I reset?  ",
            A = "<p>Click reset.</p>",
            Group = "  account  ",
            SortOrder = 3,
            Status = "published"
        }, CancellationToken.None);

        var stored = await db.Faqs.AsNoTracking().FirstAsync(f => f.Id == created.Id);

        Assert.Equal("How do I reset?", stored.Question);
        Assert.Equal("account", stored.Group);
        Assert.Equal("published", stored.Status);
        Assert.True(stored.UpdatedUtc >= before);
    }

    private static SimpleBizDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SimpleBizDbContext>()
            .UseInMemoryDatabase($"faq-tests-{Guid.NewGuid()}")
            .Options;

        return new SimpleBizDbContext(options);
    }
}
