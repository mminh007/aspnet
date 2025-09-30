@section Scripts {
    <script>
        document.addEventListener("DOMContentLoaded", () => {
            // click (+) lần đầu
            document.querySelectorAll(".add-to-cart").forEach(btn => {
                btn.addEventListener("click", async () => {
                    const productId = btn.dataset.productId;
                    const storeId = btn.dataset.storeId;
                    const userId = "@userId";

                    if (!userId || userId === "0") {
                        const currentUrl = window.location.pathname + window.location.search;
                        window.location.href = `/Authentication/Login?returnUrl=${encodeURIComponent(currentUrl)}`;
                        return;
                    }

                    const dto = { storeId, productId, quantity: 1 };

                    try {
                        const response = await fetch(`/Product/AddProductToCart?buyer=${userId}`, {
                            method: "POST",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify(dto)
                        });

                        if (response.ok) {
                            const result = await response.json();

                            // thay đổi UI thành (- qty +)
                            const container = document.getElementById(`cart-actions-${productId}`);
                            container.innerHTML = `
                                <div class="d-flex justify-content-end align-items-center">
                                    <button class="btn btn-sm btn-light border decrease"
                                            data-product-id="${productId}" data-store-id="${storeId}">-</button>
                                    <span class="mx-2 fw-bold quantity-value" id="qty-${productId}">1</span>
                                    <button class="btn btn-sm btn-light border increase"
                                            data-product-id="${productId}" data-store-id="${storeId}">+</button>
                                </div>`;

                            // cập nhật badge
                            const cartBadge = document.getElementById("cartBadge");
                            if (cartBadge) {
                                cartBadge.textContent = result.countItems;
                                cartBadge.classList.remove("bg-secondary");
                                cartBadge.classList.add("bg-danger");
                            }

                            const cartBadgeFloating = document.getElementById("cartBadgeFloating");
                            if (cartBadgeFloating) {
                                cartBadgeFloating.textContent = result.countItemsInStore; // ✅ lấy từ server
                                cartBadgeFloating.classList.remove("bg-secondary");
                                cartBadgeFloating.classList.add("bg-danger");
                            }

                            bindQuantityButtons(productId, storeId, userId);
                        } else {
                            alert("❌ Lỗi thêm giỏ hàng");
                        }
                    } catch (err) {
                        console.error("Network error:", err);
                    }
                });
            });


            // bind lại nút + -
            function bindQuantityButtons(productId, storeId, userId) {
                const container = document.getElementById(`cart-actions-${productId}`);
                const qtySpan = document.getElementById(`qty-${productId}`);

                container.querySelectorAll(".increase, .decrease").forEach(btn => {
                    btn.addEventListener("click", async () => {
                        let currentQty = parseInt(qtySpan.textContent);

                        if (btn.classList.contains("increase")) {
                            currentQty++;
                        } else if (btn.classList.contains("decrease") && currentQty > 1) {
                            currentQty--;
                        } else if (btn.classList.contains("decrease") && currentQty === 1) {
                            //nếu giảm về 0 thì quay lại nút (+)
                            container.innerHTML = `
                                <button class="btn btn-primary btn-sm add-to-cart"
                                        data-product-id="${productId}" data-store-id="${storeId}">+</button>`;
                            container.querySelector(".add-to-cart").addEventListener("click", () => {
                                document.querySelector(`#cart-actions-${productId} .add-to-cart`).click();
                            });
                            return;
                        }

                        //disable tránh spam
                        btn.disabled = true;
                        qtySpan.textContent = "...";

                        try {
                            const response = await fetch(`/Order/UpdateQuantity?userId=${userId}&productId=${productId}`, {
                                method: "PUT",
                                headers: { "Content-Type": "application/json" },
                                body: JSON.stringify({ productId, quantity: currentQty })
                            });

                            if (response.ok) {
                                const result = await response.json();
                                qtySpan.textContent = currentQty;

                                //cập nhật badge
                                const cartBadge = document.getElementById("cartBadge");
                                if (cartBadge) cartBadge.textContent = result.data.totalItems;

                                const cartBadgeFloating = document.getElementById("cartBadgeFloating");
                                if (cartBadgeFloating) {
                                    cartBadgeFloating.textContent = result.countItemsInStore; // ✅
                                    cartBadgeFloating.classList.remove("bg-secondary");
                                    cartBadgeFloating.classList.add("bg-danger");
                                }
                            } else {
                                qtySpan.textContent = currentQty; rollback
                                alert("❌ Lỗi cập nhật số lượng");
                            }
                        } catch (err) {
                            console.error("Network error:", err);
                            qtySpan.textContent = currentQty;
                        } finally {
                            btn.disabled = false;
                        }
                    });
                });
            }
        });
    </script>

    <script>
                // Smooth scroll khi click vào category
        document.querySelectorAll("#categoryList .category-link").forEach(link => {
            link.addEventListener("click", () => {
                const targetId = link.getAttribute("data-target");
                const targetEl = document.getElementById(targetId);
                if (targetEl) {
                    targetEl.scrollIntoView({ behavior: "smooth", block: "start" });

                    // highlight category đang chọn
                    document.querySelectorAll("#categoryList .category-link")
                        .forEach(el => el.classList.remove("active"));
                    link.classList.add("active");
                }
            });
        });

    </script>
}
