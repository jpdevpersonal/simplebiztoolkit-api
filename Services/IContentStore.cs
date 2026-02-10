using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;

namespace simplebiztoolkit_api.Services;

public interface IContentStore
{
    IEnumerable<Article> GetArticles(string? status, bool includeAll);
    Article? GetArticleById(Guid id);
    Article? GetArticleBySlug(string slug);
    Article AddArticle(CreateArticleDto dto);
    Article? UpdateArticle(Guid id, CreateArticleDto dto);
    bool DeleteArticle(Guid id);

    IEnumerable<ProductCategory> GetCategories();
    ProductCategory? GetCategoryById(Guid id);
    ProductCategory? GetCategoryBySlug(string slug);
    ProductCategory AddCategory(CreateCategoryDto dto);
    ProductCategory? UpdateCategory(Guid id, CreateCategoryDto dto);

    IEnumerable<Product> GetProducts();
    Product? GetProductById(Guid id);
    Product? GetProductBySlug(string categorySlug, string productSlug);
    Product AddProduct(CreateProductDto dto);
    Product? UpdateProduct(Guid id, CreateProductDto dto);
    bool DeleteProduct(Guid id);
}
