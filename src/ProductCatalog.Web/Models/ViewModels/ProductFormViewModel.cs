using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Web.Models.ViewModels;

public class ProductFormViewModel
{
    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori zorunludur.")]
    public string Category { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Açıklama en fazla 300 karakter olabilir.")]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "Fiyat 0'dan büyük olmalı.")]
    public decimal Price { get; set; }
}
