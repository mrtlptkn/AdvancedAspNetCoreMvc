# Yerelleştirme (Localization)

Bu site iki dili destekler: **Türkçe (`tr-TR`, varsayılan)** ve **İngilizce (`en-US`)**. Bu belge, dil desteğinin nasıl çalıştığını, her sınıfın neden kullanıldığını ve yeni metin ya da yeni dil eklerken nelere dikkat edilmesi gerektiğini anlatır.

---

## 1. Genel akış

Bir istek geldiğinde dil şu sırayla belirlenir:

1. **URL öneki** — `/en-US/Products` gibi bir adres doğrudan dili belirler. (`RouteCultureProvider`)
2. **Çerez** — Kullanıcı daha önce TR/EN butonuna bastıysa seçimi `.AspNetCore.Culture` çerezinde tutulur. (`CookieRequestCultureProvider`, ASP.NET Core'un varsayılan sağlayıcısı)
3. **Varsayılan** — İkisi de yoksa `tr-TR` kullanılır.

Dil öneki olmayan bir GET isteği (ör. `/Products`) gelirse `CultureRedirectMiddleware` kullanıcıyı çerezdeki dile (yoksa `tr-TR`'ye) yönlendirir: `/tr-TR/Products`.

Metinler `Resources/` klasöründeki `.resx` dosyalarında tutulur ve `IStringLocalizer<SharedResource>` ile okunur.

```
İstek ──► CultureRedirectMiddleware (öneki yoksa yönlendir)
     ──► UseRouting (URL'den {culture} değerini oku)
     ──► UseRequestLocalization
            ├─ RouteCultureProvider   (1. öncelik: URL öneki)
            ├─ CookieRequestCultureProvider (2. öncelik: çerez)
            └─ varsayılan: tr-TR
     ──► Controller / View ──► IStringLocalizer<SharedResource> ──► .resx
```

---

## 2. Dosyalar ve sınıfların görevleri

### 2.1 `Program.cs` — kurulum

| Kod | Ne yapar | Neden |
|---|---|---|
| `AddLocalization(o => o.ResourcesPath = "Resources")` | `IStringLocalizer<T>` servisini ekler ve kaynak klasörünü `Resources` olarak belirler. | Metinlerin `.resx` dosyalarında tutulması ve View'lara enjekte edilebilmesi için gerekli. |
| `RequestLocalizationOptions` | Desteklenen kültürleri (`tr-TR`, `en-US`) ve varsayılanı (`tr-TR`) tanımlar. | Tanımsız bir dil gelirse ASP.NET Core'un yanlış dile düşmemesi için. |
| `RequestCultureProviders.Insert(0, new RouteCultureProvider())` | URL önekini en yüksek önceliğe koyar. | Kullanıcı `/en-US/...` adresine girdiyse çerezdeki dil bunu geçersiz kılmamalı. |
| `app.UseRequestLocalization()` | Seçilen kültürü isteğin `CurrentCulture` / `CurrentUICulture` değerlerine atar. | Yerelleştirmenin istek boyunca etkili olması için. `UseRouting`'den sonra çağrılır ki `{culture}` değeri okunabilsin. |
| `UseMiddleware<CultureRedirectMiddleware>()` | Dil öneksiz adresleri yönlendirir. | Her sayfanın kanonik bir dil adresi olması ve SEO için. |
| `MapControllerRoute(... "{culture:regex(^(tr-TR|en-US)$)}/...")` | Tüm controller rotalarına dil öneki ekler. | Dil bilgisi URL'de kalsın diye. Regex kısıtı desteklenmeyen dilleri 404 yapar. |
| `MapControllerRoute("error", "Home/Error")` | Hata sayfasını dil öneksiz tutar. | `UseExceptionHandler` bu yolu yeniden çalıştırır; yol dil öneki içermediği için bu rota ayrıca tanımlanır. |

### 2.2 `Localization/CultureRoutes.cs` — dil sabitleri ve URL yardımcıları

Desteklenen dillerin **tek kaynağı**. Program.cs, ProductsController, SitemapController, HomeController ve middleware bu sınıfı kullanır; böylece bir dil eklemek için tek yerde değişiklik yeterli olur (regex sabiti hariç, bkz. Bölüm 5).

- `DefaultCulture` (`tr-TR`) — varsayılan dil.
- `SupportedCultures` — desteklenen diller; `RequestLocalizationOptions` ve sitemap bunu kullanır.
- `IsSupported(culture)` — bir değerin desteklenip desteklenmediğini kontrol eder (çerez ve `SetLanguage` için).
- `HasCulturePrefix(path)` — adresin zaten dil öneki içerip içermediğini söyler (middleware için).
- `WithCulture(url, culture)` — bir adresin dil önekini değiştirir ya da ekler. Dil değiştirme butonunun kullanıcıyı **aynı sayfada, yeni dilde** tutması için kullanılır (`/tr-TR/Products?x=1` → `/en-US/Products?x=1`).

### 2.3 `Localization/RouteCultureProvider.cs` — URL'den dil okuma

`RequestCultureProvider`'dan türer. İstekteki route verisinde `culture` değeri varsa ve desteklenen bir dilse onu döndürür, yoksa `NullProviderCultureResult` ile sonraki sağlayıcıya geçer.

**Neden var?** ASP.NET Core'un hazır sağlayıcıları (çerez, Accept-Language) URL'deki dili okumaz. Bu sınıf, URL'yi tek gerçek kaynak yapar: adres `/en-US/...` ise sayfa İngilizce olur, hangi çerez ya da tarayıcı ayarı olursa olsun.

### 2.4 `Localization/CultureRedirectMiddleware.cs` — öneksiz adresleri yönlendirme

- Sadece **GET** isteklerinde çalışır.
- Yol zaten dil önekliyse ya da `/Home/Error`, `/sitemap.xml` gibi istisnalardan biriyse dokunmaz.
- Diğer durumlarda çerezdeki dili (yoksa `tr-TR`'yi) kullanıp `/{culture}{path}{query}` adresine yönlendirir.

**Neden var?** Kullanıcı `/Products` yazdığında ya da eski bir bağlantıyla geldiğinde her zaman geçerli, dil öneki olan bir adresle karşılaşması için. Yönlendirme **çerezi** okur, Accept-Language'i değil; böylece kullanıcının açık seçimi tarayıcı ayarından önce gelir.

### 2.5 `Localization/` — ilgili diğer kullanımlar

- **`HomeController.SetLanguage`** (`GET /{mevcutDil}/Home/SetLanguage?culture=en-US&returnUrl=...`; layout'taki `asp-route-culture` değeri hedef dili verir): Dil seçimini `CookieRequestCultureProvider.DefaultCookieName` çerezine yazar (1 yıl, `IsEssential = true` — cookie onayı olmadan da çalışsın diye). Ardından `returnUrl`'i `WithCulture` ile yeni dile çevirip yönlendirir. `returnUrl` yerel değilse ana sayfaya gider (açık yönlendirme saldırısına karşı `Url.IsLocalUrl` kontrolü).
- **`SitemapController`**: `SupportedCultures` üzerinde döngü kurup her dil için ayrı adresler üretir (`/tr-TR/`, `/en-US/Products`, `/en-US/product/{slug}`). Böylece arama motorları iki dildeki sayfaları ayrı ayrı indeksler.
- **`ProductsController`**: `[HttpGet("{culture:regex(^(tr-TR|en-US)$)}/product/{slug}")]` — ürün detay rotası dil önekini zorunlu tutar.

### 2.6 `Resources/SharedResource.cs` — "marker" sınıf

```csharp
namespace ProductCatalog.Web;
public class SharedResource { }
```

Boş bir sınıftır. `IStringLocalizer<SharedResource>` ifadesindeki tip parametresi olarak kullanılır; ASP.NET Core bu tipin adından (`ProductCatalog.Web.SharedResource`) kaynak dosyasının adını türetir (`Resources/SharedResource.resx`). Yani **metin taşımaz, sadece kaynak dosyasını işaret eder.**

### 2.7 `Resources/*.resx` — çeviri dosyaları

| Dosya | Dil | İçerik |
|---|---|---|
| `SharedResource.resx` | Türkçe (nötr, varsayılan) | Tüm metinler (36 anahtar) |
| `SharedResource.en-US.resx` | İngilizce | Aynı anahtarların İngilizce karşılıkları (36 anahtar) |

Anahtarlar her iki dosyada aynı olmalıdır. Bir anahtar yalnızca Türkçe dosyada varsa, İngilizce sayfada anahtarın kendisi ya da Türkçe metin görünür.

### 2.8 `Resources/SharedResource.Designer.cs` — otomatik üretilen sınıf

Visual Studio, `.resx` kaydedildiğinde bu dosyayı üretir. İçindeki `SharedResource.BackToProducts` gibi property'ler **kodda kullanılmaz**; `IStringLocalizer` doğrudan `.resx` dosyasını okur. Bu dosyayı silmek güvenlidir, fakat `.resx` dosyasının Özellikler penceresinde "Custom Tool" alanı `ResXFileCodeGenerator` olarak kalırsa dosya yeniden oluşur (bkz. Bölüm 5).

### 2.9 `ProductCatalog.Web.csproj` — kaynak adları

```xml
<EmbeddedResource Update="Resources\SharedResource.en-US.resx"
                  LogicalName="ProductCatalog.Web.Resources.SharedResource.en-US.resources" />
```

`.resx` dosyaları derlemeye gömülür. `LogicalName` olmazsa `IStringLocalizer` kaynağı bulamaz ve **sayfalarda metin yerine anahtar adı** (`NavHome` gibi) görünür. Yeni bir `.resx` eklenirse buraya da bir satır eklenmelidir.

### 2.10 Views

- **`Views/_ViewImports.cshtml`**: `@inject IStringLocalizer<SharedResource> Localizer` — tüm View'larda `Localizer["Anahtar"]` kullanılabilir.
- **`Views/Shared/_Layout.cshtml`**:
  - `<html lang="...">` — `CurrentUICulture`'dan gelir; tarayıcı ve arama motorları sayfanın dilini bilir.
  - TR / EN bağlantıları — `SetLanguage` action'ına gider, `returnUrl` ile mevcut sayfayı korur. Aktif dil `aria-current` ve kalın yazıyla gösterilir.
  - `Localizer["SiteName"]`, `Localizer["NavHome"]` vb. — menü ve başlık metinleri.

---

## 3. Yeni bir metin eklemek

1. `Resources/SharedResource.resx` dosyasına `<data name="YeniAnahtar">` ekle, Türkçe değerini yaz.
2. `Resources/SharedResource.en-US.resx` dosyasına aynı `name` ile İngilizce değeri ekle.
3. View ya da controller'da kullan:
   - View: `@Localizer["YeniAnahtar"]`
   - Controller: `IStringLocalizer<SharedResource>` enjekte et, `_localizer["YeniAnahtar"]`

## 4. Yeni bir dil eklemek (ör. `de-DE`)

1. `CultureRoutes.SupportedCultures`'a `"de-DE"` ekle.
2. `CultureRoutes.LeadingCulture` regex'ine ve `Program.cs` / `ProductsController` içindeki `regex(^(tr-TR|en-US)$)` ifadelerine de ekle. (Bu regex'ler şu an sabit; tek noktadan yönetilmesi için `SupportedCultures`'tan üretilebilir.)
3. `Resources/SharedResource.de-DE.resx` oluştur, anahtarları çevir.
4. `csproj`'a `EmbeddedResource` satırı ekle.
5. Layout'a yeni dil bağlantısını ekle.

## 5. Bilinen notlar ve dikkat edilecekler

- **Regex tekrarı:** `^(tr-TR|en-US)$` ifadesi `Program.cs` ve `ProductsController`'da elle yazılmış durumda. Bir dil eklenirse ikisi de güncellenmelidir; aksi halde yeni dildeki adresler 404 döner.
- **Eksik çeviri:** İngilizce dosyada olmayan bir anahtar, İngilizce sayfada nötr (Türkçe) dosyadan okunur. Bu bir hata vermez ama metin Türkçe kalır.
- **Designer dosyası:** `SharedResource.Designer.cs` kodda kullanılmaz. Kaldırılacaksa `.resx` dosyalarının "Custom Tool" ayarı da boşaltılmalıdır, yoksa Visual Studio yeniden üretir.
- **Çerez:** Dil seçimi 1 yıl saklanır. Kullanıcı çerezi silerse varsayılan dile (`tr-TR`) döner.
- **Sıkıştırma ve dil:** Dinamik HTTPS sayfaları sıkıştırılmaz (`Program.cs` içindeki BREACH notu); dil yönlendirmesi bunu etkilemez.
- **Hata sayfası:** `/Home/Error` dil önekine ihtiyaç duymaz; hata durumunda bile çalışması için istisna listesinde tutulur.
