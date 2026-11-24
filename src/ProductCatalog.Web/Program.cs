using System.Globalization;
using System.IO.Compression;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using ProductCatalog.Data.Repositories;
using ProductCatalog.Services;
using ProductCatalog.Web.Filters;
using ProductCatalog.Web.Localization;
using ProductCatalog.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC ve özel filtre
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<RequestTimingFilter>();
});

// Önbellek (ProductService kullanır)
builder.Services.AddMemoryCache();

// Sıkıştırma: HTTPS'te dinamik sayfalar sıkıştırılmaz (BREACH riski: gizli token + yansıyan girdi).
// HTTPS'te yalnızca statik dosyalar (/css, /js) sıkıştırılır; bkz. app.UseWhen aşağıda.
builder.Services.AddResponseCompression(options =>
{
  options.EnableForHttps = true; // Sadece UseWhen dalında çalışır; dinamik HTTPS yanıtları bu dala girmez
  options.Providers.Add<BrotliCompressionProvider>();
  options.Providers.Add<GzipCompressionProvider>();
  options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json","application/html" });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
  options.Level = CompressionLevel.Fastest;
});

// Yerelleştirme: TR varsayılan, EN destekli. Metinler Resources/SharedResource*.resx içinde.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = CultureRoutes.SupportedCultures
        .Select(c => new CultureInfo(c))
        .ToList();

    options.DefaultRequestCulture = new RequestCulture(CultureRoutes.DefaultCulture);
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;

    // URL'deki dil öneki (/en-US/...) çerez ve varsayılandan önce okunur.
    options.RequestCultureProviders.Insert(0, new RouteCultureProvider());
});

// Sepet için session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor(); // IHttpContextAccessor erişim için bu servisi DI eklemeniz lazım. 

// Katmanlar (DI)
builder.Services.AddSingleton<ProductChangeNotifier>();
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
// Sıkıştırma: HTTP'de her yanıt; HTTPS'te yalnızca statik dosyalar.
// Dinamik HTTPS sayfaları (gizli token içerenler) BREACH riskine karşı sıkıştırılmaz.
//app.UseWhen(
//    ctx => !ctx.Request.IsHttps || IsStaticPath(ctx.Request.Path),
//    branch => branch.UseResponseCompression());

app.UseResponseCompression();

app.UseStaticFiles();
app.UseMiddleware<CultureRedirectMiddleware>();
app.UseRouting();
app.UseRequestLocalization();
app.UseSession();

// Hata sayfası dil öneki olmadan çalışır (ExceptionHandler bu yolu yeniden çalıştırır)
app.MapControllerRoute(
    name: "error",
    pattern: "Home/Error",
    defaults: new { controller = "Home", action = "Error" });

app.MapControllerRoute(
    name: "default",
    pattern: "{culture:regex(^(tr-TR|en-US)$)}/{controller=Home}/{action=Index}/{id?}");

static bool IsStaticPath(PathString path)
    => path.StartsWithSegments("/css") || path.StartsWithSegments("/js");

app.Run();

// Entegrasyon testleri (WebApplicationFactory) için
public partial class Program { }
