using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/articles")]
public class ArticlesController : ApiControllerBase
{
    private readonly IContentStore _store;
    private readonly IRevalidationService _revalidationService;

    public ArticlesController(IContentStore store, IRevalidationService revalidationService)
    {
        _store = store;
        _revalidationService = revalidationService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] string? status)
    {
        if (!string.Equals(status, "published", StringComparison.OrdinalIgnoreCase))
        {
            status = "published";
        }

        var articles = _store.GetArticles(status, includeAll: false);
        return Ok(new { data = articles });
    }

    [HttpGet("/api/admin/articles")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> GetAllAdmin([FromQuery] string? status)
    {
        var articles = _store.GetArticles(status, includeAll: true);
        return Ok(new { data = articles });
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult> GetBySlug(string slug)
    {
        var article = _store.GetArticleBySlug(slug);
        var isAuthenticated = User?.Identity?.IsAuthenticated == true;

        if (article == null || (!isAuthenticated && !string.Equals(article.Status, "published", StringComparison.OrdinalIgnoreCase)))
        {
            return await ErrorResponse("Article not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = article });
    }

    [HttpGet("/api/admin/articles/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var article = _store.GetArticleById(id);
        if (article == null)
        {
            return await ErrorResponse("Article not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = article });
    }

    [HttpPost("/api/admin/articles")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> Create([FromBody] CreateArticleDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Title))
        {
            return await ErrorResponse("Slug and title are required.", StatusCodes.Status400BadRequest);
        }

        var article = _store.AddArticle(dto);
        await _revalidationService.TriggerAsync("article", article.Slug, cancellationToken);
        return Ok(new { data = article });
    }

    [HttpPut("/api/admin/articles/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> Update(Guid id, [FromBody] CreateArticleDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Title))
        {
            return await ErrorResponse("Slug and title are required.", StatusCodes.Status400BadRequest);
        }

        var article = _store.UpdateArticle(id, dto);
        if (article == null)
        {
            return await ErrorResponse("Article not found", StatusCodes.Status404NotFound);
        }

        await _revalidationService.TriggerAsync("article", article.Slug, cancellationToken);
        return Ok(new { data = article });
    }

    [HttpDelete("/api/admin/articles/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var removed = _store.DeleteArticle(id);
        if (!removed)
        {
            return await ErrorResponse("Article not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { success = true });
    }
}
