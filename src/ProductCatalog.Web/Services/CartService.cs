using ProductCatalog.Services;
using ProductCatalog.Web.Extensions;

namespace ProductCatalog.Web.Services;

// Sepet işlemleri. Sepet, HTTP session'da JSON olarak saklanır.
public interface ICartService
{
    Cart GetCart();
    Cart AddItem(int productId, int quantity);
    Cart RemoveItem(int productId);
}

public class CartService : ICartService
{
    private const string SessionKey = "Cart";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session
        => _httpContextAccessor.HttpContext?.Session
           ?? throw new InvalidOperationException("Sepet işlemi için aktif bir HTTP isteği gerekir.");

    public Cart GetCart() => LoadCart();

    public Cart AddItem(int productId, int quantity)
    {
        var cart = LoadCart();
        cart.Add(productId, quantity);
        SaveCart(cart);
        return cart;
    }

    public Cart RemoveItem(int productId)
    {
        var cart = LoadCart();
        cart.Remove(productId);
        SaveCart(cart);
        return cart;
    }

    private Cart LoadCart()
        => Session.GetObject<Cart>(SessionKey) ?? new Cart();

    private void SaveCart(Cart cart)
        => Session.SetObject(SessionKey, cart);
}
