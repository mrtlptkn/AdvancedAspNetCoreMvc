# Konu Haritası

Bu dosya, eğitimde işlenen her konunun projede nerede örneklendiğini gösterir.


## AJAX

**Neden AJAX?** Sayfanın tamamını yenilemeden yalnızca değişen bölümü güncellemek için. Daha hızlı ve akıcı bir kullanıcı deneyimi sağlar.

**AJAX yapısı:** Tarayıcı bir istek gönderir, sunucu HTML parçası ya da JSON döner, JavaScript sayfayı günceller.

- Scriptler ile çalışma: `wwwroot/js/cart.js`, `wwwroot/js/products.js`
- Partial page rendering: `ProductsController.ProductList` → `_ProductList.cshtml`, `products.js` içinde `#product-list` yenilenir
- Canlı arama: `products.js` (300 ms gecikmeli arama)
- Hata yakalama: `cart.js` ve `products.js` içindeki `try/catch`
- JSON ve MVC: `CartController.Add` JSON döner

## Client validation

`Create.cshtml` içindeki `asp-validation-for` etiketleri ve `_Layout.cshtml` içindeki jQuery Validation betikleri.

## Filtre

`Filters/RequestTimingFilter.cs`: her action'ın süresini loglayan özel Action Filter.

## Middleware

**Middleware nedir?** HTTP isteği ile yanıt arasında sırayla çalışan bileşenlerdir. Her middleware isteği inceleyebilir, değiştirebilir, yanıtı kısa devre ile kesebilir (`return`) ya da `_next` ile bir sonrakine aktarabilir. Sıra `Program.cs` içindeki `app.Use...` çağrılarının sırasını belirler.

**Filtre ile farkı:** Filtreler yalnızca MVC action'ları için çalışır ve controller bağlamını görür. Middleware ise MVC'den önce, tüm istekler için çalışır (statik dosyalar, yönlendirme, hata sayfası dahil).

**Projede:**

- Pipeline sırası (`Program.cs`): `UseExceptionHandler` → `UseHttpsRedirection` → `UseWhen` (sıkıştırma) → `UseStaticFiles` → `CultureRedirectMiddleware` → `UseRouting` → `UseRequestLocalization` → `UseSession`. Sıra önemlidir: dil önekini çözmeden önce yönlendirme yapılmalı, session ise endpoint'lerden önce hazır olmalıdır.
- Özel middleware: `Localization/CultureRedirectMiddleware.cs`. Dil öneki olmayan GET isteklerini (`/Products`) çerezdeki ya da varsayılan dilin önekiyle (`/tr-TR/Products`) yönlendirir. `/Home/Error` ve `/sitemap.xml` istisnadır. Kullanım: `app.UseMiddleware<CultureRedirectMiddleware>()`.
- Koşullu middleware: `app.UseWhen(ctx => ..., branch => branch.UseResponseCompression())`. HTTPS üzerindeki dinamik sayfalar sıkıştırılmaz (BREACH riski); yalnızca `/css` ve `/js` ile HTTP yanıtları sıkıştırılır. Bkz. "Veri sıkıştırma".
- Hata yakalama: `UseExceptionHandler("/Home/Error")` bir middleware'dir; hata sayfası bu yolu yeniden çalıştırır.

**Dikkat edilecekler:**

- Middleware'i yanlış sıraya koymak hataya yol açar. Örneğin `UseRouting`'den önce yazılan dil yönlendirmesi, route verisine erişemez; `UseSession`'ı endpoint'lerden sonra koymak session'ı boş bırakır.
- Middleware yalnızca bir kez oluşturulur (singleton gibi davranır). Scoped servisleri constructor'a almayın; `InvokeAsync` parametresine alın veya `HttpContext.RequestServices` kullanın.

## Model ile çalışma

`Models/ViewModels/` içindeki ViewModel'ler ve `ProductFormViewModel` içindeki Data Annotations.

## Önbellek ve bağımlılık (Caching & Dependency)

- `ProductService.cs`: tüm ürünler `IMemoryCache` ile önbelleğe alınır, 5 dakika geçerlidir.
- `ProductChangeNotifier.cs`: ürün eklenince önbellek bir bağımlılık belirteci (change token) ile geçersiz kılınır.
- Test: `ProductServiceTests.GetProductsAsync_UsesCacheOnSecondCall` ve `CreateAsync_InvalidatesCache`.

## Veri sıkıştırma

`Program.cs`: Brotli ve Gzip sıkıştırma, HTTPS üzerinde kapalı.

Not: HTTPS üzerinde sıkıştırma, sayfada gizli bir değer (ör. antiforgery token) ile kullanıcı girdisi aynı yanıtta olduğunda BREACH saldırısına açık olabilir. Bu projede risk kabul edilmiştir;

## Globalization ve Localization

- `Program.cs`: TR varsayılan, EN destekli. `RequestLocalizationOptions` ile kültür ayarları.
- `Resources/SharedResource.resx` (TR) ve `Resources/SharedResource.en-US.resx` (EN): tüm arayüz metinleri.
- `_Layout.cshtml`, `_ViewImports.cshtml`: `IStringLocalizer<SharedResource>` ile metinler.
- `HomeController.SetLanguage`: dil seçimini çerezde saklar.
- Fiyatlar `ToString("C")` ile seçili kültüre göre biçimlenir.

Not: Data Annotations hata mesajları (`ProductFormViewModel.cs`) şu an Türkçe. Bunları yerelleştirmek için ayrı bir kaynak yapılandırması gerekir.

## Arama motoru optimizasyonu (SEO)

- Slug'lı adresler: `/product/klavye-pro`
- Meta description: `_Layout.cshtml` içinde `ViewData["MetaDescription"]`, ürün sayfasında ürünün açıklaması
- Sitemap: `Controllers/SitemapController.cs` → `/sitemap.xml`

## Controller teknikleri

`ProductsController.cs`: `Index`, `ProductList` (partial), `Details` (attribute route), `Create` (GET/POST, `ActionName`, `ValidateAntiForgeryToken`).

## Sunucu ve istemci platform özellikleri

- Cookie: son bakılan ürünler (`ProductsController.Details`, `RecentlyViewed` cookie'si, en fazla 5 ürün)
- Session: sepet nesnesi JSON string olarak saklanır. Session okuma/yazma `Web/Services/CartService.cs` (`ICartService`) içinde yapılır; `SessionObjectExtensions` (`SetObject<T>` / `GetObject<T>`) ile yazılır ve okunur. Sepet kuralları `Services/Cart.cs` içinde, sepet sayfası `CartController` (yalnızca `ICartService` üzerinden çalışır) ve `Views/Cart/Index.cshtml` içinde.
- Antiforgery: AJAX sepet istekleri (ekleme ve çıkarma) `RequestVerificationToken` header'ı ile gönderilir (`cart.js`)
- Sepetten çıkarma: AJAX ile satır kaldırılır, toplam ve sayaç sunucudan gelen yanıtla güncellenir (`CartController.Remove`)
- Dil çerezi: `HomeController.SetLanguage`

## Hata yakalama, görüntüleme ve izleme

- Merkezi hata yakalama: `Program.cs` içinde `UseExceptionHandler("/Home/Error")`
- Hata sayfası: `HomeController.Error` ve `Views/Home/Error.cshtml`
- İzleme: `RequestTimingFilter` içinde `ILogger` ile işlem süresi kaydı


## Tasarım ve mimari

Katmanlı yapı: `ProductCatalog.Web` → `ProductCatalog.Services` → `ProductCatalog.Data`. Bağımlılıklar DI ile verilir (`Program.cs`).

## View Components

Yeniden kullanılabilir, kendi mantığını ve DI bağımlılıklarını taşıyan görünüm parçaları. Partial view'dan farkı: `Invoke` içinde servis kullanabilir ve kendi verisini hazırlar.

- Sepet sayacı: `ViewComponents/CartBadgeViewComponent.cs` (`ICartService` ile sayıyı alır) → `Views/Shared/Components/CartBadge/Default.cshtml`; layout'ta `Component.InvokeAsync("CartBadge")`
- Dil değiştirici (TR | EN): `ViewComponents/LanguageSwitcherViewComponent.cs` → `Views/Shared/Components/LanguageSwitcher/Default.cshtml`; layout'ta `Component.InvokeAsync("LanguageSwitcher")`



## TDD

**TDD nedir?** Önce başarısız olacak bir test yazılır (Red), sonra testi geçirecek en az kod yazılır (Green), en son kod temizlenir (Refactor). Amaç, kodun ne yapması gerektiğini önce netleştirmektir.

**Teste başlamak için ipuçları**
- Her test tek bir davranışı kontrol etsin.
- Test adı ne beklendiğini anlatsın: `CreateAsync_ThrowsWhenPriceIsNotPositive`.
- Veritabanı ya da dosya gibi dış bağımlılıkları taklit (mock) ile değiştirin.
- Testler birbirine bağlı olmasın; her test kendi verisini kursun.

**Projede:** `tests/ProductCatalog.Tests/Services/ProductServiceTests.cs`

## Controller testleri ve routing

- Controller testi: `tests/ProductCatalog.Tests/Controllers/ProductsControllerTests.cs`
- Routing testi (gerçek uygulama, HTTP istekleri): `tests/ProductCatalog.Tests/Integration/RoutingTests.cs`

## Test doubles ve mocks

- Moq ile `IProductRepository` taklit edilir: `ProductServiceTests.cs`
- Test doubles türleri: dummy (`new Product`), stub (`ReturnsAsync`), mock (`Verify`)

## xUnit

Test projesi xUnit ile yazılır: `ProductCatalog.Tests.csproj`