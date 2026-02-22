using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;

namespace simplebiztoolkit_api.Services;

public interface IContentStore
{
    Task<List<Article>> GetArticlesAsync(string? status, bool includeAll, CancellationToken ct = default);
    Task<Article?> GetArticleByIdAsync(Guid id, CancellationToken ct = default);
    Task<Article?> GetArticleBySlugAsync(string slug, CancellationToken ct = default);
    Task<Article> AddArticleAsync(CreateArticleDto dto, CancellationToken ct = default);
    Task<Article?> UpdateArticleAsync(Guid id, CreateArticleDto dto, CancellationToken ct = default);
    Task<bool> DeleteArticleAsync(Guid id, CancellationToken ct = default);

    Task<List<ProductCategory>> GetCategoriesAsync(CancellationToken ct = default);
    Task<ProductCategory?> GetCategoryByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductCategory?> GetCategoryBySlugAsync(string slug, CancellationToken ct = default);
    Task<ProductCategory> AddCategoryAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<ProductCategory?> UpdateCategoryAsync(Guid id, CreateCategoryDto dto, CancellationToken ct = default);

    Task<List<Product>> GetProductsAsync(CancellationToken ct = default);
    Task<List<Product>> GetProductsByCategoryIdAsync(Guid categoryId, bool publishedOnly, CancellationToken ct = default);
    Task<Product?> GetProductByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetProductBySlugAsync(string categorySlug, string productSlug, CancellationToken ct = default);
    Task<Product> AddProductAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<Product?> UpdateProductAsync(Guid id, CreateProductDto dto, CancellationToken ct = default);
    Task<bool> DeleteProductAsync(Guid id, CancellationToken ct = default);
}
