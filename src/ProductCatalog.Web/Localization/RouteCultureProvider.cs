using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;

namespace ProductCatalog.Web.Localization;

// Dili URL'deki {culture} segmentinden okur (/en-US/Products -> en-US).
public class RouteCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var routeData = httpContext.GetRouteData();

        if (routeData is not null
            && routeData.Values.TryGetValue("culture", out var value)
            && value is string culture
            && CultureRoutes.IsSupported(culture))
        {
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture));
        }

        return NullProviderCultureResult;
    }
}
