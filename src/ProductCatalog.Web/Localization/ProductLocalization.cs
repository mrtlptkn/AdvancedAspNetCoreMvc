using System.Globalization;
using ProductCatalog.Data.Entities;

namespace ProductCatalog.Web.Localization;

// Ürün metinlerini aktif arayüz diline göre seçer. İngilizce alan boşsa Türkçe değer kullanılır.
public static class ProductLocalization
{
    public static string LocalizedName(this Product product)
        => Pick(product.NameEn, product.Name);

    public static string LocalizedCategory(this Product product)
        => Pick(product.CategoryEn, product.Category);

    public static string LocalizedDescription(this Product product)
        => Pick(product.DescriptionEn, product.Description);

    private static string Pick(string? english, string turkish)
    {
        var isEnglish = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en";
        return isEnglish && !string.IsNullOrWhiteSpace(english) ? english : turkish;
    }
}
