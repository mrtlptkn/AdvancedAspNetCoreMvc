using Microsoft.Extensions.Caching.Memory;
using ProductCatalog.Data.Entities;
using ProductCatalog.Data.Repositories;

namespace ProductCatalog.Services;

public class ProductService : IProductService
{
    private const string AllProductsKey = "products:all";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IProductRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ProductChangeNotifier _notifier; // Inmemory yaklaşımlarda cache bozmak için bir yöntem olmadığında istek iptali ile çözüm yapılmış.

    public ProductService(IProductRepository repository, IMemoryCache cache, ProductChangeNotifier notifier)
    {
        _repository = repository;
        _cache = cache;
        _notifier = notifier;
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(string? category, string? search, CancellationToken ct = default)
    {
        var all = await GetAllCachedAsync(ct);

        IEnumerable<Product> query = all;

    // cachelenmiş veri üszerinde filtreleme
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase) || p.CategoryEn.Equals(category,StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || p.NameEn.Contains(search, StringComparison.OrdinalIgnoreCase));

        return query.ToList();
    }

    public Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => _repository.GetBySlugAsync(slug, ct);

    public async Task CreateAsync(Product product, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (product.Price <= 0)
            throw new ArgumentOutOfRangeException(nameof(product.Price), "Fiyat 0'dan büyük olmalı.");

        // TODO: Türkçe karakterleri dönüştüren gerçek slug üreticisi
        product.Slug = product.Name.ToLowerInvariant().Replace(' ', '-');

        await _repository.AddAsync(product, ct);

        // Ürün listesi değişti: önbelleği geçersiz kıl
        _notifier.NotifyChanged();
    }

    // Tüm ürünler önbellekten okunur; girdi 5 dakika sonra ya da ürün değişince yenilenir.
    private Task<IReadOnlyList<Product>> GetAllCachedAsync(CancellationToken ct)
    {
    // key varsa cacheden döner
        return _cache.GetOrCreateAsync(AllProductsKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration; // 5 minutes
            entry.AddExpirationToken(_notifier.GetChangeToken()); // eğer 5 dakika dolarsa  _notifier.GetChangeToken() methodu ile event fırlat cache boz.
          return _repository.GetAllAsync(ct); // içeriği data katmanındaki repository çeker ve 5 dakikalığına cacheler.
        });
    }
}
