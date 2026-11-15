using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductCatalog.Data.Entities;
using ProductCatalog.Services;
using ProductCatalog.Web.Controllers;
using ProductCatalog.Web.Models.ViewModels;
using Xunit;

namespace ProductCatalog.Tests.Controllers;

public class ProductsControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithProducts()
    {
        var service = new Mock<IProductService>();
        service
            .Setup(s => s.GetProductsAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { new() { Name = "A", Price = 1 } });

        var controller = new ProductsController(service.Object);

        var result = await controller.Index(null, null, CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProductListViewModel>(view.Model);
        Assert.Single(model.Products);
    }

    [Fact]
    public async Task ProductList_ReturnsPartialView()
    {
        var service = new Mock<IProductService>();
        service
            .Setup(s => s.GetProductsAsync(null, "a", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        var controller = new ProductsController(service.Object);

        var result = await controller.ProductList(null, "a", CancellationToken.None);

        var partial = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_ProductList", partial.ViewName);
    }
}
