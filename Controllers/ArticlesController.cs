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
    public ActionResult GetAll([FromQuery] string? status)
    {
        var isAuthenticated = User?.Identity?.IsAuthenticated == true;

        if (!isAuthenticated && !string.Equals(status, "published", StringComparison.OrdinalIgnoreCase))
        {
            return ErrorResponse("Unauthorized", StatusCodes.Status401Unauthorized);
        }

        var articles = _store.GetArticles(status, includeAll: isAuthenticated);
        return Ok(new { data = articles });
    }

    [HttpGet("slug/{slug}")]
    public ActionResult GetBySlug(string slug)
    {
        var article = _store.GetArticleBySlug(slug);
        var isAuthenticated = User?.Identity?.IsAuthenticated == true;

        if (article == null || (!isAuthenticated && !string.Equals(article.Status, "published", StringComparison.OrdinalIgnoreCase)))
        {
            return ErrorResponse("Article not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = article });
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public ActionResult GetById(Guid id)
    {
        var article = _store.GetArticleById(id);
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
            var article = _store.AddArticle(dto);
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
            var article = _store.UpdateArticle(id, dto);
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
    public ActionResult Delete(Guid id)
    {
        var removed = _store.DeleteArticle(id);
        if (!removed)
        {
            return ErrorResponse("Article not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { success = true });
    }
}
