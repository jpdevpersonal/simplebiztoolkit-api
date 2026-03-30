using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;

namespace simplebiztoolkit_api.Services;

public interface IContentStore
{
    IEnumerable<ProductCategory> GetCategories();
    ProductCategory? GetCategoryById(Guid id);
    ProductCategory? GetCategoryBySlug(string slug);
    ProductCategory AddCategory(CreateCategoryDto dto);
    ProductCategory? UpdateCategory(Guid id, CreateCategoryDto dto);
    bool DeleteCategory(Guid id);

    IEnumerable<Product> GetProducts();
    Product? GetProductById(Guid id);
    Product? GetProductBySlug(string categorySlug, string productSlug);
    Product AddProduct(CreateProductDto dto);
    Product? UpdateProduct(Guid id, CreateProductDto dto);
    bool DeleteProduct(Guid id);
}
