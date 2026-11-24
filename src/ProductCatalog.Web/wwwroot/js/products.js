 // Ürün listesini sayfa yenilemeden günceller (AJAX ile kısmi görünüm yenileme).
// Filtre formu gönderilince ya da yazarken yalnızca #product-list bölümü değişir.
(function () {
    const form = document.getElementById("filter-form");
    const list = document.getElementById("product-list");
    if (!form || !list) return;

    const listUrl = form.dataset.listUrl;
    const errorText = form.dataset.errorText;
    const searchInput = form.querySelector("input[name='search']");
    const categoryInput = form.querySelector("input[name='category']");

    async function refreshList() {
        const params = new URLSearchParams(new FormData(form)).toString();

        try {
            const response = await fetch(`${listUrl}?${params}`, {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });

            if (!response.ok) {
                throw new Error(`Sunucu hatası: ${response.status}`);
            }

            list.innerHTML = await response.text();
            console.log('partialView Result',list.innerHTML);
            history.replaceState(null, "", `${window.location.pathname}?${params}`);
        } catch (error) {
          console.log("error",error)
            list.textContent = errorText;
        }
    }

    function debounce(fn, delay) {
        let timer;
        return (...args) => {
            clearTimeout(timer);
            timer = setTimeout(() => fn(...args), delay);
        };
    }

    form.addEventListener("submit", (event) => {
        event.preventDefault();
        refreshList();
    });

    // Canlı arama: yazdıkça sonuçlar güncellenir (300 ms gecikmeli)
    searchInput.addEventListener("input", debounce(refreshList, 300));
    categoryInput.addEventListener("change", refreshList);
})();
