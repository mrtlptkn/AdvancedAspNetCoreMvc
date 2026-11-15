using ProductCatalog.Data.Entities;

namespace ProductCatalog.Web.Models.ViewModels;

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = new();
    public IReadOnlyList<Product> RecentlyViewed { get; set; } = Array.Empty<Product>();
}
