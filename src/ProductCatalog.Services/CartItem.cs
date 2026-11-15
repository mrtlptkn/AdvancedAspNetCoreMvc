namespace ProductCatalog.Services;

// Sepetteki bir satır. Fiyat ve ürün adı burada tutulmaz; katalogdan okunur.
public class CartItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
