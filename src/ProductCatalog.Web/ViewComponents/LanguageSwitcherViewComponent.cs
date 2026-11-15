using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Web.Models.ViewModels;

namespace ProductCatalog.Web.ViewComponents;

// TR | EN dil bağlantıları. Aktif dili vurgular; seçimde kullanıcı aynı sayfada yeni dilde kalır.
// Görünüm: Views/Shared/Components/LanguageSwitcher/Default.cshtml
public class LanguageSwitcherViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var request = HttpContext.Request;

        return View(new LanguageSwitcherViewModel(
            CultureInfo.CurrentUICulture.Name,
            $"{request.Path}{request.QueryString}"));
    }
}
