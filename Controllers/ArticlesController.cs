using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult> GetAll([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var isAuthenticated = User?.Identity?.IsAuthenticated == true;

        if (!isAuthenticated && !string.Equals(status, "published", StringComparison.OrdinalIgnoreCase))
        {
            return ErrorResponse("Unauthorized", StatusCodes.Status401Unauthorized);
        }

        var articles = await _store.GetArticlesAsync(status, includeAll: isAuthenticated, cancellationToken);
        return Ok(new { data = articles });
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var article = await _store.GetArticleBySlugAsync(slug, cancellationToken);
        var isAuthenticated = User?.Identity?.IsAuthenticated == true;

        if (article == null || (!isAuthenticated && !string.Equals(article.Status, "published", StringComparison.OrdinalIgnoreCase)))
        {
            return ErrorResponse("Article not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = article });
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var article = await _store.GetArticleByIdAsync(id, cancellationToken);
        if (article == null)
        {
            return ErrorResponse("Article not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = article });
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create([FromBody] CreateArticleDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Title))
        {
            return ErrorResponse("Slug and title are required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var article = await _store.AddArticleAsync(dto, cancellationToken);
            await _revalidationService.TriggerAsync("article", article.Slug, cancellationToken);
            return Ok(new { data = article });
        }
        catch (InvalidOperationException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> Update(Guid id, [FromBody] CreateArticleDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Title))
        {
            return ErrorResponse("Slug and title are required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var article = await _store.UpdateArticleAsync(id, dto, cancellationToken);
            if (article == null)
            {
                return ErrorResponse("Article not found", StatusCodes.Status404NotFound);
            }

            await _revalidationService.TriggerAsync("article", article.Slug, cancellationToken);
            return Ok(new { data = article });
        }
        catch (InvalidOperationException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var removed = await _store.DeleteArticleAsync(id, cancellationToken);
        if (!removed)
        {
            return ErrorResponse("Article not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { success = true });
    }
}
