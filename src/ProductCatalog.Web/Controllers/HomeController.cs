using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Web.Localization;

namespace ProductCatalog.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();

    // GET: /en-US/Home/SetLanguage?returnUrl=/tr-TR/Products
    // Dil tercihini çerezde saklar; kullanıcıyı geldiği sayfaya, dil öneki yeni dile çevrilmiş olarak yönlendirir.
    public IActionResult SetLanguage(string culture, string? returnUrl)
    {
        if (!CultureRoutes.IsSupported(culture))
            culture = CultureRoutes.DefaultCulture;

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1)});

        var target = Url.IsLocalUrl(returnUrl)
            ? CultureRoutes.WithCulture(returnUrl!, culture)
            : $"/{culture}/";

        return LocalRedirect(target);
    }
}
