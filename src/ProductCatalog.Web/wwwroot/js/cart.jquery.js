// Sepet işlemleri (jQuery sürümü): sayfayı yenilemeden ekleme ve çıkarma (AJAX).
// cart.js ile aynı davranışı sağlar. Event delegation sayesinde AJAX ile yenilenen liste içindeki butonlar da çalışır.
$(function () {
    // Layout'taki antiforgery token'ı; her POST isteğinde header ile gönderilir
    function antiForgeryToken() {
        return $("input[name='__RequestVerificationToken']").val() || "";
    }

    function postJson(url, body) {
        return $.ajax({
            url: url,
            method: "POST",
            contentType: "application/json",
            dataType: "json",
            headers: { "RequestVerificationToken": antiForgeryToken() },
            data: JSON.stringify(body)
        });
    }

    function showError(message) {
        alert($("body").data("cartError") + " " + message);
    }

    $(document).on("click", ".add-to-cart", function () {
        const productId = Number($(this).data("productId"));

        postJson($("body").data("cartAddUrl"), { productId: productId, quantity: 1 })
            .done(function (data) {
                $("#cart-count").text(data.count);
            })
            .fail(function (xhr) {
                showError(`Sunucu hatası: ${xhr.status}`);
            });
    });

    $(document).on("click", ".remove-from-cart", function () {
        const button = $(this);
        const productId = Number(button.data("productId"));

        postJson($("body").data("cartRemoveUrl"), { productId: productId })
            .done(function (data) {
                $("#cart-count").text(data.count);

                const totalElement = $("#cart-total");
                if (totalElement.length) totalElement.text(data.total);

                // Satırı kaldır; sepet boşaldıysa boş sepet mesajı için sayfayı yenile
                button.closest("tr").remove();
                if (data.count === 0) window.location.reload();
            })
            .fail(function (xhr) {
                showError(`Sunucu hatası: ${xhr.status}`);
            });
    });
});
