using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Services;
using ProductCatalog.Web.Localization;

namespace ProductCatalog.Web.Controllers;

public class SitemapController : Controller
{
    private readonly IProductService _products;

    public SitemapController(IProductService products)
    {
        _products = products;
    }

    // GET: /sitemap.xml
    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var products = await _products.GetProductsAsync(null, null, ct);
        var urls = new List<XElement>();

        // Her dil için ayrı adresler: /tr-TR/..., /en-US/...
        foreach (var culture in CultureRoutes.SupportedCultures)
        {
            urls.Add(new XElement(ns + "url", new XElement(ns + "loc", $"{baseUrl}/{culture}/")));
            urls.Add(new XElement(ns + "url", new XElement(ns + "loc", $"{baseUrl}/{culture}/Products")));

            foreach (var p in products)
            {
                urls.Add(new XElement(ns + "url",
                    new XElement(ns + "loc", $"{baseUrl}/{culture}/product/{p.Slug}"),
                    new XElement(ns + "lastmod", today)));
            }
        }

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "urlset", urls));

        var xml = $"{document.Declaration}{Environment.NewLine}{document.Root}";

        return Content(xml, "application/xml", Encoding.UTF8);
    }
}
