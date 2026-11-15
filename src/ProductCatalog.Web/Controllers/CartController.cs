using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Services;
using ProductCatalog.Web.Models.ViewModels;
using ProductCatalog.Web.Services;

namespace ProductCatalog.Web.Controllers;

public class CartController : Controller
{
    private readonly IProductService _products;
    private readonly ICartService _cart;

    public CartController(IProductService products, ICartService cart)
    {
        _products = products;
        _cart = cart;
    }

    // GET: /Cart
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var lines = await BuildLinesAsync(_cart.GetCart(), ct);
        return View(new CartViewModel { Lines = lines });
    }

    // POST: /Cart/Add  (AJAX, JSON; antiforgery token header ile gelir)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add([FromBody] AddToCartRequest request, CancellationToken ct)
    {
        if (request.Quantity < 1 || request.Quantity > Cart.MaxQuantityPerProduct)
            return BadRequest(new { message = "Geçersiz adet." });

        var exists = (await _products.GetProductsAsync(null, null, ct))
            .Any(p => p.Id == request.ProductId);

        if (!exists)
            return NotFound(new { message = "Ürün bulunamadı." });

        var cart = _cart.AddItem(request.ProductId, request.Quantity);

        return Json(new { count = cart.ItemCount });
    }

    // POST: /Cart/Remove  (AJAX, JSON; antiforgery token header ile gelir)
    // Dönen değer: güncel adet ve biçimlendirilmiş toplam. Sayfa satırı kendisi kaldırır.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove([FromBody] RemoveFromCartRequest request, CancellationToken ct)
    {
        var cart = _cart.RemoveItem(request.ProductId);

        var lines = await BuildLinesAsync(cart, ct);
        var total = lines.Sum(l => l.LineTotal);

        return Json(new { count = cart.ItemCount, total = total.ToString("C") });
    }

    // Sepet satırlarını katalogdan okunan fiyat ve ürün bilgisiyle birleştirir.
    private async Task<List<CartLineViewModel>> BuildLinesAsync(Cart cart, CancellationToken ct)
    {
        var catalog = (await _products.GetProductsAsync(null, null, ct)).ToDictionary(p => p.Id);

        // Katalogdan silinmiş ürünler sepette atlanır
        return cart.Items
            .Where(i => catalog.ContainsKey(i.ProductId))
            .Select(i => new CartLineViewModel
            {
                Product = catalog[i.ProductId],
                Quantity = i.Quantity
            })
            .ToList();
    }
}

public record AddToCartRequest(int ProductId, int Quantity = 1);

public record RemoveFromCartRequest(int ProductId);
