using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Web.Services;

namespace ProductCatalog.Web.ViewComponents;

// Üst menüdeki sepet sayacı. Layout sepet mantığını bilmez; sayıyı bu bileşen ICartService'ten alır.
// Görünüm: Views/Shared/Components/CartBadge/Default.cshtml
public class CartBadgeViewComponent : ViewComponent
{
    private readonly ICartService _cart;

    public CartBadgeViewComponent(ICartService cart) => _cart = cart;

    public IViewComponentResult Invoke() => View(_cart.GetCart().ItemCount);
}
