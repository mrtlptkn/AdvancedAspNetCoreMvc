# Neden ViewComponent Kullanıyoruz?

Bu dosya, ProductCatalog projesinde yeniden kullanılabilir görünüm parçaları için neden **ViewComponent** tercih ettiğimizi açıklar. Konu haritası için `docs/KONU-HARITASI.md` dosyasındaki "View Components" bölümüne bakın.

## Kısa cevap

Üst menüdeki **sepet sayacı** ve **dil değiştirici** gibi parçalar:

- Tüm sayfalarda (layout'ta) görünür,
- Kendi verisini **servislerden** ya da **HTTP bağlamından** almak zorundadır,
- Hangi controller/action çalışırsa çalışsın aynı şekilde davranmalıdır.

Bu üç özellik ViewComponent'in tam olarak çözdüğü problemdir.

## Alternatiflerle karşılaştırma

| Yaklaşım | Bu projede neden uygun değil? |
|---|---|
| **Partial view** (`_CartBadge.cshtml`) | Sayıyı her controller action'ının `ViewBag`/model ile göndermesi gerekir. Layout tüm sayfalarda kullanıldığı için her action'a aynı kodu eklemek zorunda kalırız ve bir action'ı unutmak sessiz bir hataya yol açar. |
| **Layout içinde doğrudan kod** (`@inject ICartService`) | Çalışır, ama sepet mantığı layout'a sızar. Layout yalnızca sunum yapmalıdır; sayıyı nasıl hesapladığını bilmemelidir. Ayrıca bu kodu birim testiyle doğrulamak zordur. |
| **TagHelper** | Tek bir HTML etiketi üretmek için uygundur, ancak bizim ihtiyacımız bir model hazırlayıp kendi view'ını render etmek. Bu, ViewComponent'in asıl işi. |
| **Filter / Middleware ile `ViewData` doldurmak** | Her istekte çalışır, sepet okunmasa bile. Ayrıca `ViewData` tip güvenli değildir. |

## Projedeki iki örnek

### 1. Sepet sayacı — `CartBadgeViewComponent`

- Dosya: `src/ProductCatalog.Web/ViewComponents/CartBadgeViewComponent.cs`
- Görünüm: `src/ProductCatalog.Web/Views/Shared/Components/CartBadge/Default.cshtml`
- Kullanım: `_Layout.cshtml` içinde `Component.InvokeAsync("CartBadge")`

Neden ViewComponent?

- Sayının kaynağı `ICartService`'tir. Bileşen bu servisi **constructor injection** ile alır; controller'ların bu sayıyı bilmesine gerek yoktur.
- Layout, sepetin session'da nasıl tutulduğunu bilmez. Session yapısı değişirse yalnızca `CartService` ve bileşen etkilenir.
- `Invoke()` yalnızca sayıyı döner ve view'a verir; birim testte kolayca taklit edilebilir (`ICartService` mock'lanarak).

### 2. Dil değiştirici — `LanguageSwitcherViewComponent`

- Dosya: `src/ProductCatalog.Web/ViewComponents/LanguageSwitcherViewComponent.cs`
- Görünüm: `src/ProductCatalog.Web/Views/Shared/Components/LanguageSwitcher/Default.cshtml`
- Kullanım: `_Layout.cshtml` içinde `Component.InvokeAsync("LanguageSwitcher")`

Neden ViewComponent?

- Aktif kültür (`CultureInfo.CurrentUICulture`) ve mevcut istek yolu (`Request.Path` + `QueryString`) **HTTP bağlamından** okunur. Bu bilgiler bir controller'dan geçirilmek yerine bileşenin içinde alınır.
- Dil seçildiğinde kullanıcı aynı sayfada kalsın diye `returnUrl` bileşen tarafından üretilir. Her sayfanın bu bilgiyi ayrıca hazırlaması gerekmez.
- Bileşen `LanguageSwitcherViewModel` ile sunum verisini taşır; görünüm mantığı `.cshtml` içinde kalır.

## Ne zaman ViewComponent, ne zaman partial?

Projede şu kurallar izlenir:

- **ViewComponent:** Kendi verisini hazırlaması gereken, servis kullanan, birden fazla sayfada (özellikle layout'ta) tekrar eden parçalar.
- **Partial view:** Controller'ın zaten elinde olan modelle render edilen, mantık içermeyen parçalar. Örnek: `_ProductList.cshtml` — `ProductsController.ProductList` action'ı bu parçayı AJAX ile döndürür ve model zaten controller'dan gelir.

## Bilinen dezavantajlar

- Her bileşen için en az iki dosya (sınıf + görünüm) oluşur. Basit bir HTML parçası için bu gereksiz olabilir.
- Bileşen `Invoke` içinde veritabanına ya da servislere gidiyorsa, layout her render edildiğinde bu çağrı tekrar yapılır. Sayaç gibi hafif işlemler için sorun değildir; ağır sorgular için önbellek ya da başka bir strateji düşünülmelidir.
- Bileşenin çalışması için DI'a kayıtlı servislerin olması gerekir (`Program.cs`).

## Özet

ViewComponent, **"sunumu layout'ta tutup veriyi servisten alan"** parçalar için kullanılır. Bu projede sepet sayacı ve dil değiştirici tam olarak bu tanıma uyduğu için ViewComponent tercih edilmiştir.
