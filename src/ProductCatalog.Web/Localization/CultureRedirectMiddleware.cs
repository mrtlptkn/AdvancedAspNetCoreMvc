using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;

namespace ProductCatalog.Web.Localization;

// Dil öneki olmayan GET isteklerini (örn. /Products) dil öneki olan adrese yönlendirir (/tr-TR/Products).
// Dil, çerezde kayıtlıysa o dil; değilse varsayılan dil kullanılır.
public class CultureRedirectMiddleware
{
    // Dil öneki gerektirmeyen yollar
    private static readonly string[] PassThroughPaths = { "/Home/Error", "/sitemap.xml" };

    private readonly RequestDelegate _next;

    public CultureRedirectMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        if (HttpMethods.IsGet(request.Method) && NeedsCulturePrefix(request.Path))
        {
            var culture = ResolveCookieCulture(request) ?? CultureRoutes.DefaultCulture;
            var target = $"{request.PathBase.ToUriComponent()}/{culture}{request.Path.ToUriComponent()}{request.QueryString.ToUriComponent()}";

            context.Response.Redirect(target);
            return;
        }

        await _next(context);
    }

    private static bool NeedsCulturePrefix(PathString path)
    {
        if (CultureRoutes.HasCulturePrefix(path))
            return false;

        return !PassThroughPaths.Contains(path.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);
    }

    private static string? ResolveCookieCulture(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue(CookieRequestCultureProvider.DefaultCookieName, out var raw))
            return null;

        var uiCulture = CookieRequestCultureProvider.ParseCookieValue(raw)?.UICultures.FirstOrDefault();
        var value = uiCulture?.Value;

        return CultureRoutes.IsSupported(value) ? value : null;
    }
}
