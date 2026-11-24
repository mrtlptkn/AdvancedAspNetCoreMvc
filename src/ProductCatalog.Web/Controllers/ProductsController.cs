using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Data.Entities;
using ProductCatalog.Services;
using ProductCatalog.Web.Models.ViewModels;

namespace ProductCatalog.Web.Controllers;

public class ProductsController : Controller
{
    private const string RecentCookieName = "RecentlyViewed";
    private const int RecentLimit = 5;

    private readonly IProductService _products;

    /// <summary>
    /// Ürün işlemleri için gereken servisi (<see cref="IProductService"/>) dependency injection ile alır.
    /// Controller'ın veri erişimini doğrudan yapmasını engeller; tüm ürün işlemleri bu servis üzerinden yürür.
    /// </summary>
    public ProductsController(IProductService products)
    {
        _products = products;
    }

    /// <summary>
    /// Ürün listesinin tam sayfasını döner (GET /Products?category=...&amp;search=...).
    /// Kullanıcı sayfayı doğrudan açtığında veya bir bağlantıyla geldiğinde kullanılır.
    /// Filtreleme mantığı <see cref="BuildListAsync"/> içinde yapılır, sonuç Index.cshtml görünümüne gönderilir.
    /// </summary>
    public async Task<IActionResult> Index(string? category, string? search, CancellationToken ct)
    {
        var model = await BuildListAsync(category, search, ct);
        return View(model);
    }

    /// <summary>
    /// Yalnızca ürün listesi bölümünü (_ProductList partial view) döner; layout içermez.
    /// Index.cshtml içindeki data-list-url üzerinden AJAX ile çağrılır: kullanıcı filtre veya arama
    /// yaptığında sayfa yenilenmeden sadece liste güncellenir.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ProductList(string? category, string? search, CancellationToken ct)
    {
        var model = await BuildListAsync(category, search, ct);
        return PartialView("_ProductList", model);
    }

    /// <summary>
    /// Tek bir ürünün detay sayfasını gösterir (GET /{culture}/product/{slug}).
    /// Slug ile ürünü bulur, yoksa 404 döner. Route'taki dil kısıtı yalnızca desteklenen dillerin
    /// (tr-TR, en-US) bu adresle eşleşmesini sağlar.
    /// Ayrıca son bakılan ürünleri "RecentlyViewed" çerezinde (en fazla 5, en yenisi başta) tutar ve
    /// diğer son bakılan ürünleri Details.cshtml içindeki "Son Bakılanlar" bölümünde göstermek için yükler.
    /// </summary>
    [HttpGet("{culture:regex(^(tr-TR|en-US)$)}/product/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        var product = await _products.GetBySlugAsync(slug, ct);
        if (product is null)
            return NotFound();

        // Son bakılan ürünleri cookie'de tut (en fazla 5, en yenisi başta)
        var recentSlugs = ReadRecentSlugs();
        recentSlugs.Remove(slug);
        recentSlugs.Insert(0, slug);
        if (recentSlugs.Count > RecentLimit)
            recentSlugs.RemoveRange(RecentLimit, recentSlugs.Count - RecentLimit);
        WriteRecentSlugs(recentSlugs);

        var recent = new List<Product>();
        foreach (var recentSlug in recentSlugs.Where(s => s != slug))
        {
            var item = await _products.GetBySlugAsync(recentSlug, ct);
            if (item is not null)
                recent.Add(item);
        }

        return View(new ProductDetailsViewModel
        {
            Product = product,
            RecentlyViewed = recent
        });
    }


    /// <summary>
    /// Eski detay adresini (GET /{culture}/Products/Details?slug=...) kanonik adrese (/{culture}/product/{slug})
    /// 301 Moved Permanently ile yönlendirir. Ürün yoksa 404 döner.
    /// </summary>
    [HttpGet("{culture:regex(^(tr-TR|en-US)$)}/Products/Details")]
    public async Task<IActionResult> LegacyDetails(string culture, string slug, CancellationToken ct)
    {
        var product = await _products.GetBySlugAsync(slug, ct);
        if (product is null)
            return NotFound();

        return RedirectToActionPermanent(nameof(Details), new { culture, slug });
    }

    /// <summary>
    /// Yeni ürün ekleme formunu (boş <see cref="ProductFormViewModel"/> ile) gösterir (GET /Products/Create).
    /// Kullanıcının ürün ekleme akışındaki ilk adımdır; form gönderildiğinde <see cref="CreatePost"/> çalışır.
    /// </summary>
    [HttpGet]
    public IActionResult Create() => View(new ProductFormViewModel());

    /// <summary>
    /// Doldurulmuş ürün formunu alıp yeni ürünü kaydeder (POST /Products/Create).
    /// <c>[ValidateAntiForgeryToken]</c> sahte istekleri (CSRF) engeller. Form geçersizse hata mesajlarıyla
    /// formu tekrar gösterir; geçerliyse <see cref="Product"/> nesnesi oluşturup servis üzerinden kaydeder
    /// ve ürün listesine yönlendirir. <c>ActionName("Create")</c> sayesinde GET ve POST aynı URL'i kullanır.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Create")]
    public async Task<IActionResult> CreatePost(ProductFormViewModel form, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(form);

        var product = new Product
        {
            Name = form.Name,
            Category = form.Category,
            Description = form.Description,
            Price = form.Price
        };

        await _products.CreateAsync(product, ct);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Filtrelenmiş ürün listesi için ortak yardımcı metot. <see cref="Index"/> ve <see cref="ProductList"/>
    /// aynı mantığı tekrar yazmamak için bunu kullanır: servisten kategori ve arama metnine göre ürünleri
    /// çeker ve <see cref="ProductListViewModel"/> olarak paketler (filtre değerleri de görünümde kalsın diye).
    /// </summary>
    private async Task<ProductListViewModel> BuildListAsync(string? category, string? search, CancellationToken ct)
    {
        var items = await _products.GetProductsAsync(category, search, ct); // ProductService

        return new ProductListViewModel
        {
            Products = items,
            Category = category,
            Search = search
        };
    }

    /// <summary>
    /// İstek çerezindeki "RecentlyViewed" değerini okuyup '|' karakteriyle ayrılmış ürün slug listesine çevirir.
    /// Çerez yoksa boş liste döner. Yazma işlemi için <see cref="WriteRecentSlugs"/> ile birlikte kullanılır.
    /// </summary>
    private List<string> ReadRecentSlugs()
    {
        if (!Request.Cookies.TryGetValue(RecentCookieName, out var raw))
            return new List<string>();

        return raw.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    /// <summary>
    /// Son bakılan ürün slug listesini '|' ile birleştirip "RecentlyViewed" çerezine yazar.
    /// Güvenlik ayarları: HttpOnly (JavaScript okuyamaz), SameSite=Lax (çapraz site isteklerde sınırlı gönderim),
    /// Secure (yalnızca HTTPS'te; HTTPS kullanılmıyorsa <c>Request.IsHttps</c> false olur) ve 30 gün geçerlilik.
    /// </summary>
    private void WriteRecentSlugs(List<string> slugs)
    {
        Response.Cookies.Append(RecentCookieName, string.Join('|', slugs), new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
    }
}
