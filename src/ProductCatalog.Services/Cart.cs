using System.Text.Json.Serialization;

namespace ProductCatalog.Services;

// Session'da JSON olarak saklanan sepet nesnesi.
public class Cart
{
    public const int MaxQuantityPerProduct = 99;

    public List<CartItem> Items { get; set; } = new();

    // Sepetteki farklı ürün sayısı (satır sayısı); rozet bunu gösterir.
    [JsonIgnore]
    public int ItemCount => Items.Count;

    [JsonIgnore]
    public int TotalQuantity => Items.Sum(i => i.Quantity);

    public void Add(int productId, int quantity)
    {
        if (quantity < 1 || quantity > MaxQuantityPerProduct)
            throw new ArgumentOutOfRangeException(nameof(quantity),
                $"Adet 1 ile {MaxQuantityPerProduct} arasında olmalı.");

        var existing = Items.FirstOrDefault(i => i.ProductId == productId);

        if (existing is null)
        {
            Items.Add(new CartItem { ProductId = productId, Quantity = quantity });
            return;
        }

        existing.Quantity = Math.Min(MaxQuantityPerProduct, existing.Quantity + quantity);
    }

    public void Remove(int productId)
        => Items.RemoveAll(i => i.ProductId == productId);
}
