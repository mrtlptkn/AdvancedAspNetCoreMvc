namespace ProductCatalog.Web.Models.ViewModels;

// Dil değiştirici için: aktif kültür ve dil değişince dönülecek sayfa (yol + sorgu).
public record LanguageSwitcherViewModel(string CurrentCulture, string ReturnUrl);
