// Sepet işlemleri: sayfayı yenilemeden ekleme ve çıkarma (AJAX + fetch).
// Event delegation sayesinde AJAX ile yenilenen liste içindeki butonlar da çalışır.

// Layout'taki antiforgery token'ı; her POST isteğinde header ile gönderilir
function antiForgeryToken() {
    return document.querySelector("input[name='__RequestVerificationToken']")?.value ?? "";
}

async function postJson(url, body) {
    const response = await fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": antiForgeryToken()
        },
        body: JSON.stringify(body)
    });

    if (!response.ok) {
        throw new Error(`Sunucu hatası: ${response.status}`);
    }

    return response.json();
}

document.addEventListener("click", async (event) => {
    const addButton = event.target.closest(".add-to-cart");
    const removeButton = event.target.closest(".remove-from-cart");

    if (!addButton && !removeButton) return;

    try {
        if (addButton) {
            const data = await postJson(document.body.dataset.cartAddUrl, {
                productId: Number(addButton.dataset.productId),
                quantity: 1
            });
            document.getElementById("cart-count").textContent = data.count;
        }

        if (removeButton) {
            const data = await postJson(document.body.dataset.cartRemoveUrl, {
                productId: Number(removeButton.dataset.productId)
            });

            document.getElementById("cart-count").textContent = data.count;

            const totalElement = document.getElementById("cart-total");
            if (totalElement) totalElement.textContent = data.total;

            // Satırı kaldır; sepet boşaldıysa boş sepet mesajı için sayfayı yenile
            removeButton.closest("tr")?.remove();
            if (data.count === 0) window.location.reload();
        }
    } catch (error) {
        alert(document.body.dataset.cartError + " " + error.message);
    }
});
