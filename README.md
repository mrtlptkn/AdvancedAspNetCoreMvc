# ProductCatalog — Genel İskelet

ASP.NET Core MVC (.NET 8) ile mini ürün kataloğu. Amaç: TDD, ileri MVC teknikleri ve AJAX konularını tek bir projede örneklemek. Konuların hangi dosyada olduğu için `docs/KONU-HARITASI.md` dosyasına bakın.

## Klasör yapısı

```
ProductCatalog/
├── ProductCatalog.sln
├── README.md
├── docs/
│   └── KONU-HARITASI.md
├── src/
│   ├── ProductCatalog.Data/          # Entity'ler ve repository (şimdilik bellek içi)
│   ├── ProductCatalog.Services/      # İş kuralları, önbellek ve bağımlılık belirteci
│   └── ProductCatalog.Web/           # MVC: Controller, View, Filter, Resources, wwwroot
└── tests/
    └── ProductCatalog.Tests/         # xUnit + Moq birim testleri, routing testleri
```

## Çalıştırma

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/ProductCatalog.Web
```

## Notlar

- Proje henüz derlenip test edilmedi (geliştirme ortamında .NET SDK yoktu).
- Harici betikler (jQuery, jQuery Validation) CDN'den yüklenir; üretimde `wwwroot`'a alınmalıdır.
- Data Annotations hata mesajları henüz Türkçe; yerelleştirilmesi sonraki adım.
- Sepet session'da JSON olarak saklanıyor (ürün ID ve adet). Fiyatlar katalogdan okunuyor.
- Repository bellek içinde; EF Core ile veritabanına taşınması sonraki adım.
