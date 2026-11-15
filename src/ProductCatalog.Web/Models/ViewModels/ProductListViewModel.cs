using ProductCatalog.Data.Entities;

namespace ProductCatalog.Web.Models.ViewModels;

public class ProductListViewModel
{
    public IReadOnlyList<Product> Products { get; set; } = Array.Empty<Product>();
    public string? Category { get; set; }
    public string? Search { get; set; }
}
