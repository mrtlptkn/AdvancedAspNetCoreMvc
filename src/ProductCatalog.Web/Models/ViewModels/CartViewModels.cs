using ProductCatalog.Data.Entities;

namespace ProductCatalog.Web.Models.ViewModels;

public class CartViewModel
{
    public IReadOnlyList<CartLineViewModel> Lines { get; set; } = Array.Empty<CartLineViewModel>();

    public decimal Total => Lines.Sum(l => l.LineTotal);
}

public class CartLineViewModel
{
    public Product Product { get; set; } = new();
    public int Quantity { get; set; }

    public decimal LineTotal => Product.Price * Quantity;
}
