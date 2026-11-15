using ProductCatalog.Data.Entities;

namespace ProductCatalog.Services;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetProductsAsync(string? category, string? search, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task CreateAsync(Product product, CancellationToken ct = default);
}
