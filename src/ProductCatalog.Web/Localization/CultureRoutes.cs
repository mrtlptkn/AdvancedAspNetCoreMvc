using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace ProductCatalog.Web.Localization;

// URL'lerde dil öneki: /tr-TR/Products, /en-US/product/mouse-lite
// Route kısıtı (regex(^(tr-TR|en-US)$)) Program.cs ve ProductsController'da aynı listeyi kullanır.
public static class CultureRoutes
{
    public const string DefaultCulture = "tr-TR";

    public static readonly string[] SupportedCultures = { "tr-TR", "en-US" };

    
    private static readonly Regex LeadingCulture =
        new(@"^/(tr-TR|en-US)(?=[/?#]|$)", RegexOptions.Compiled);

    /// <summary>
    /// Verilen dil kodunun (ör. "tr-TR", "en-US") desteklenen diller listesinde olup olmadığını kontrol eder.
    /// Projede: RouteCultureProvider'da URL'deki dil kodunu doğrulamak, CultureRedirectMiddleware'de
    /// çerezdeki dil değerinin geçerli olup olmadığını anlamak ve HomeController.SetLanguage'da
    /// kullanıcının seçtiği dili kabul etmeden önce doğrulamak için kullanılır.
    /// Desteklenmeyen bir değer gelirse uygulama varsayılan dile (DefaultCulture) düşer.
    /// </summary>
    public static bool IsSupported(string? culture)
        => culture is not null && SupportedCultures.Contains(culture);

    /// <summary>
    /// Verilen yolun (path) zaten geçerli bir dil öneki ("/tr-TR/..." veya "/en-US/...") ile başlayıp
    /// başlamadığını kontrol eder.
    /// Projede CultureRedirectMiddleware tarafından kullanılır: önek yoksa kullanıcı çerezdeki ya da
    /// varsayılan dilin önekiyle yeni adrese yönlendirilir; önek varsa istek olduğu gibi devam eder.
    /// </summary>
    public static bool HasCulturePrefix(PathString path)
        => LeadingCulture.IsMatch(path.Value ?? string.Empty);

    /// <summary>
    /// Bir URL'nin dil önekini verilen dille değiştirir; önek yoksa adresin başına ekler.
    /// Örnek: "/tr-TR/Products?x=1" → "/en-US/Products?x=1".
    /// Projede HomeController.SetLanguage'da kullanılır: dil değiştirildikten sonra kullanıcı
    /// returnUrl ile aynı sayfada, yeni dilde yönlendirilir (dil değiştirmek sayfayı değiştirmez).
    /// </summary>
    public static string WithCulture(string url, string culture)
    {
        var match = LeadingCulture.Match(url);
        return match.Success
            ? $"/{culture}{url[match.Length..]}"
            : $"/{culture}{url}";
    }
}
