using ProductCatalog.Data.Entities;

namespace ProductCatalog.Data.Repositories;

// Geçici bellek içi depo. İleride EF Core ile değiştirilecek.
public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products =
    [
        new()
        {
            Id = 1, Name = "Kulaklık X-100", Slug = "kulaklik-x-100", Category = "Ses",
            Description = "Gürültü engelleyici, kablosuz kulaklık.", Price = 1499.90m,
            NameEn = "Headphones X-100", CategoryEn = "Audio",
            DescriptionEn = "Noise-cancelling wireless headphones."
        },
        new()
        {
            Id = 2, Name = "Klavye Pro", Slug = "klavye-pro", Category = "Bilgisayar",
            Description = "Mekanik tuşlu, arka aydınlatmalı klavye.", Price = 899.00m,
            NameEn = "Keyboard Pro", CategoryEn = "Computers",
            DescriptionEn = "Mechanical keyboard with backlit keys."
        },
        new()
        {
            Id = 3, Name = "Mouse Lite", Slug = "mouse-lite", Category = "Bilgisayar",
            Description = "Hafif yapılı kablolu mouse.", Price = 349.50m,
            NameEn = "Mouse Lite", CategoryEn = "Computers",
            DescriptionEn = "Lightweight wired mouse."
        }
    ];

    private readonly object _lock = new();

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            IReadOnlyList<Product> snapshot = _products.ToList();
            return Task.FromResult(snapshot);
        }
    }

    public Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var product = _products.FirstOrDefault(p => p.Slug == slug);
            return Task.FromResult(product);
        }
    }

    public Task AddAsync(Product product, CancellationToken ct = default)
    {
        lock (_lock)
        {
            product.Id = _products.Count == 0 ? 1 : _products.Max(p => p.Id) + 1;
            _products.Add(product);
            return Task.CompletedTask;
        }
    }
}
