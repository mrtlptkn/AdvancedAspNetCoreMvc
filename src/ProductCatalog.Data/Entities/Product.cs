namespace ProductCatalog.Data.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }

    // İsteğe bağlı İngilizce içerik. Boşsa arayüz Türkçe alanları (Name, Category, Description) kullanır.
    public string? NameEn { get; set; }
    public string? CategoryEn { get; set; }
    public string? DescriptionEn { get; set; }
}
