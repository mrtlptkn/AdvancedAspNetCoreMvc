using Microsoft.Extensions.Caching.Memory;
using Moq;
using ProductCatalog.Data.Entities;
using ProductCatalog.Data.Repositories;
using ProductCatalog.Services;
using Xunit;

namespace ProductCatalog.Tests.Services;

public class ProductServiceTests
{
    private static ProductService CreateService(Mock<IProductRepository> repository)
        => new(repository.Object, new MemoryCache(new MemoryCacheOptions()), new ProductChangeNotifier());

    private static void SetupProducts(Mock<IProductRepository> repository)
    {
        repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>
            {
                new() { Name = "Kulaklık", Category = "Ses", Price = 1 },
                new() { Name = "Mouse", Category = "Bilgisayar", Price = 1 }
            });
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenPriceIsNotPositive()
    {
        var repository = new Mock<IProductRepository>();
        var service = CreateService(repository);
        var product = new Product { Name = "Test", Category = "Genel", Price = 0 };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.CreateAsync(product));
    }

    [Fact]
    public async Task GetProductsAsync_FiltersByCategory()
    {
        var repository = new Mock<IProductRepository>();
        SetupProducts(repository);
        var service = CreateService(repository);

        var result = await service.GetProductsAsync("ses", null);

        Assert.Single(result);
        Assert.Equal("Kulaklık", result[0].Name);
    }

    [Fact]
    public async Task GetProductsAsync_FiltersBySearchText()
    {
        var repository = new Mock<IProductRepository>();
        SetupProducts(repository);
        var service = CreateService(repository);

        var result = await service.GetProductsAsync(null, "MOUSE");

        Assert.Single(result);
        Assert.Equal("Mouse", result[0].Name);
    }

    [Fact]
    public async Task GetProductsAsync_UsesCacheOnSecondCall()
    {
        var repository = new Mock<IProductRepository>();
        SetupProducts(repository);
        var service = CreateService(repository);

        await service.GetProductsAsync(null, null);
        await service.GetProductsAsync(null, null);

        repository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidatesCache()
    {
        var repository = new Mock<IProductRepository>();
        SetupProducts(repository);
        repository
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var service = CreateService(repository);

        await service.GetProductsAsync(null, null);
        await service.CreateAsync(new Product { Name = "Yeni", Category = "Genel", Price = 10 });
        await service.GetProductsAsync(null, null);

        // Ürün eklenince önbellek silinir, liste yeniden okunur
        repository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
