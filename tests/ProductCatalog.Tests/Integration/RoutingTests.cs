using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ProductCatalog.Tests.Integration;

// Rotaların gerçek uygulama üzerinde çalıştığını doğrular.
public class RoutingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RoutingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task SlugUrl_RoutesToDetailsPage()
    {
        var response = await _client.GetAsync("/tr-TR/product/klavye-pro");

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Klavye Pro", html);
    }

    [Fact]
    public async Task UnknownSlug_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/tr-TR/product/olmayan-urun");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ProductList_ReturnsPartialWithoutLayout()
    {
        var response = await _client.GetAsync("/tr-TR/Products/ProductList?search=mouse");

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Mouse Lite", html);
        Assert.DoesNotContain("<nav>", html);
    }

    [Fact]
    public async Task UrlWithoutCulture_RedirectsToDefaultCulture()
    {
        using var noRedirectClient = CreateNoRedirectClient();

        var response = await noRedirectClient.GetAsync("/Products?search=mouse");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/tr-TR/Products?search=mouse", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task EnglishUrl_RendersEnglishPageWithCultureInRoute()
    {
        var response = await _client.GetAsync("/en-US/Products");

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("lang=\"en\"", html);
        Assert.Contains("href=\"/en-US/Products\"", html);
    }

    [Fact]
    public async Task Pages_ShowTextInSelectedLanguage()
    {
        // Razor Türkçe karakterleri HTML entity olarak yazar; karşılaştırmadan önce çözülür.
        var turkish = WebUtility.HtmlDecode(await _client.GetStringAsync("/tr-TR/Products"));
        var english = WebUtility.HtmlDecode(await _client.GetStringAsync("/en-US/Products"));

        Assert.Contains("Ürün Kataloğu", turkish);
        Assert.Contains("Product Catalog", english);
        Assert.DoesNotContain("Product Catalog", turkish);
    }

    [Fact]
    public async Task SetLanguage_RedirectsToSameTranslatedPath()
    {
        using var noRedirectClient = CreateNoRedirectClient();

        var response = await noRedirectClient.GetAsync("/en-US/Home/SetLanguage?returnUrl=/tr-TR/Products%3Fsearch%3Dmouse");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/en-US/Products?search=mouse", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Sitemap_ListsProductUrls()
    {
        var response = await _client.GetAsync("/sitemap.xml");

        response.EnsureSuccessStatusCode();
        var xml = await response.Content.ReadAsStringAsync();
        Assert.Contains("/tr-TR/product/mouse-lite", xml);
    }

    private HttpClient CreateNoRedirectClient()
        => _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
}
