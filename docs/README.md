# ProductCatalog.Web — Tailwind CSS

Bu belge, uygulamanın arayüzüne **Tailwind CSS**'in nasıl eklendiğini, nasıl derlendiğini ve günlük geliştirmede nasıl kullanıldığını anlatır.

- Tailwind sürümü: **4.3.3** (`@tailwindcss/cli` ve `tailwindcss`, `package.json` içinde sabitlenmiştir)
- Yöntem: Node.js ile **yerel derleme**. CDN'deki `cdn.tailwindcss.com` betiği kullanılmadı; çünkü o betik üretim için önerilmez ve her sayfa yüklenişinde CSS'i tarayıcıda üretir.
- Çıktı: `wwwroot/css/tailwind.css` (küçültülmüş, tek dosya)

## Eklenen dosyalar

| Dosya | Amaç |
|---|---|
| `package.json` | Tailwind bağımlılıkları ve `build:css` / `watch:css` betikleri |
| `package-lock.json` | Bağımlılıkların kilitli sürümleri (`npm install` ile oluşur) |
| `Styles/tailwind.css` | Tailwind girdi dosyası; kaynak taraması ve ortak bileşen sınıfları burada |
| `wwwroot/css/tailwind.css` | Derlenmiş çıktı (**üretilir, elle düzenlenmez**) |
| `Views/Shared/_Layout.cshtml` | `tailwind.css` bağlantısı eklendi |
| `wwwroot/css/site.css` | Tailwind'e taşınan eski kurallar kaldırıldı; yalnızca doğrulama hata stili kaldı |

`node_modules/` klasörü de `npm install` ile oluşur. Bu klasör versiyon kontrolüne eklenmemelidir.

## Kurulum (sıfırdan nasıl yapıldı)

Aşağıdaki adımlar bir kez uygulanmıştır; tekrar etmeniz gerekmez.

1. Web projesi klasöründe Tailwind paketlerini kurun:
   ```bash
   cd src/ProductCatalog.Web
   npm install --save-dev --save-exact tailwindcss@4.3.3 @tailwindcss/cli@4.3.3
   ```
   `--save-exact` bayrağı sürümü `"4.3.3"` olarak yazar; `^` işareti eklenmez, böylece her kurulumda farklı bir sürüm inmez.

2. `package.json` içine derleme betiklerini ekleyin:
   ```json
   "scripts": {
     "build:css": "tailwindcss -i ./Styles/tailwind.css -o ./wwwroot/css/tailwind.css --minify",
     "watch:css": "tailwindcss -i ./Styles/tailwind.css -o ./wwwroot/css/tailwind.css --watch"
   }
   ```

3. `Styles/tailwind.css` dosyasını oluşturun. Tailwind'in hangi dosyalarda class aradığını `@source` ile belirtiriz:
   ```css
   @import "tailwindcss";

   @source "../Views";
   @source "../Controllers";
   @source "../wwwroot/js";

   @layer components {
     .btn-primary { @apply rounded-md bg-blue-600 px-4 py-2 font-medium text-white hover:bg-blue-700 disabled:opacity-50; }
     /* ... */
   }
   ```

4. CSS'i derleyin:
   ```bash
   npm run build:css
   ```

5. `_Layout.cshtml` içinde Tailwind dosyasını **`site.css`'ten önce** yükleyin:
   ```html
   <link rel="stylesheet" href="~/css/tailwind.css" asp-append-version="true" />
   <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
   ```

## Günlük kullanım

| Ne yapmak istiyorsunuz? | Komut |
|---|---|
| Projeyi ilk kez hazırlamak (Node.js gerekir) | `npm install` (klasör: `src/ProductCatalog.Web`) |
| Class değiştirdikten sonra CSS'i yeniden üretmek | `npm run build:css` |
| Geliştirirken CSS'i otomatik izlemek ve üretmek | `npm run watch:css` (ayrı bir terminalde açık bırakın) |
| Uygulamayı çalıştırmak | `dotnet run --project src/ProductCatalog.Web` |

> **Önemli:** `wwwroot/css/tailwind.css` dosyası repoda tutulur; böylece Node.js olmayan bir ortamda da uygulama çalışır. Ancak Razor view'larında yeni bir Tailwind class'ı kullandığınızda CSS'i **yeniden derlemeniz** gerekir. Aksi halde class tanımlı olmaz ve stil uygulanmaz.

## Ortak bileşen sınıfları

Tekrar eden stiller `Styles/tailwind.css` içinde `@layer components` altında tanımlıdır. View'larda doğrudan kullanılır:

| Sınıf | Kullanım |
|---|---|
| `btn-primary` | Ana eylem butonları (Filtrele, Kaydet, Sepete ekle) |
| `btn-secondary` | İkincil butonlar (Sepetten çıkar); renk için yardımcı class ile ezilebilir |
| `input` | Metin kutuları, arama kutusu, textarea |
| `link` | Uygulama içi bağlantılar |

Yeni bir ortak stil gerektiğinde önce bu dosyaya bileşen olarak ekleyin; aynı utility zincirini birden fazla view'a kopyalamayın.

## Yeni bir klasör eklerseniz

Tailwind yalnızca `@source` ile belirtilen klasörlerdeki class adlarını üretir. Örneğin yeni bir `Components/` klasörü açarsanız `Styles/tailwind.css` içine şunu ekleyin:

```css
@source "../Components";
```

Sonra `npm run build:css` çalıştırın.

## Sorun giderme

- **Stil uygulanmıyor:** `npm run build:css` çalıştırıp `wwwroot/css/tailwind.css` dosyasını yeniden üretin. Tarayıcıda sayfayı sert yenileyin (Ctrl+F5); `asp-append-version` sayesinde yeni dosya genellikle otomatik alınır.
- **`npm` bulunamadı:** Node.js LTS sürümünü kurun (bu proje Node 20 ile test edildi).
- **jQuery Validation hata mesajları kırmızı değil:** `site.css` içindeki `.field-validation-error` kuralı hâlâ duruyor; span'lere ayrıca `text-red-700` eklenmiştir.
- **Sürümü yükseltmek:** Yeni sürümü `npm view @tailwindcss/cli version` ile görün, `package.json` içinde sürümü değiştirip `npm install` ve `npm run build:css` çalıştırın. Tailwind v4 ile v3 arasında yapılandırma farkları vardır; yükseltmeden önce sürüm notlarını okuyun.
