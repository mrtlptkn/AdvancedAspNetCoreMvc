using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ProductCatalog.Tests.Integration;

// Detay sayfasının URL'deki dile göre doğru metinleri gösterdiğini doğrular.
public class LocalizedDetailsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public LocalizedDetailsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task TurkishDetails_ShowsTurkishContentAndButton()
    {
        var response = await _client.GetAsync("/tr-TR/product/mouse-lite");

        response.EnsureSuccessStatusCode();
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains("Hafif yapılı kablolu mouse.", html);
        Assert.Contains("Kategori: Bilgisayar", html);
        Assert.Contains("Sepete ekle", html);
        Assert.DoesNotContain("Lightweight wired mouse.", html);
    }

    [Fact]
    public async Task EnglishDetails_ShowsEnglishContentAndButton()
    {
        var response = await _client.GetAsync("/en-US/product/mouse-lite");

        response.EnsureSuccessStatusCode();
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains("lang=\"en\"", html);
        Assert.Contains("Lightweight wired mouse.", html);
        Assert.Contains("Category: Computers", html);
        Assert.Contains("Add to cart", html);
        Assert.DoesNotContain("Hafif yapılı", html);
        Assert.DoesNotContain("AddToCart", html);
    }

    [Fact]
    public async Task EnglishDetails_ShowsEnglishProductNameForOtherProduct()
    {
        var response = await _client.GetAsync("/en-US/product/kulaklik-x-100");

        response.EnsureSuccessStatusCode();
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains("Headphones X-100", html);
        Assert.DoesNotContain("Kulaklık X-100", html);
    }

    [Fact]
    public async Task LegacyDetailsUrl_RedirectsPermanentlyToCanonicalUrl()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/en-US/Products/Details?slug=kulaklik-x-100");

        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal("/en-US/product/kulaklik-x-100", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task LegacyDetailsUrl_UnknownSlug_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/tr-TR/Products/Details?slug=olmayan-urun");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
