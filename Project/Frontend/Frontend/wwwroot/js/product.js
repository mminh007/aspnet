document.addEventListener("DOMContentLoaded", () => {
    const floatingBtn = document.querySelector(".floating-cart-btn");
    const userId = floatingBtn?.dataset.userId || "";
    const storeId = floatingBtn?.dataset.storeId || "";

    if (!userId || !storeId) {
        console.error("❌ Missing userId or storeId");
        return;
    }

    // =============================
    // 0. Initialize badges
    // =============================
    function updateBadges(totalItems, countItemsInStore) {
        const cartBadgeFloating = document.getElementById("cartBadgeFloating");
        if (cartBadgeFloating) {
            cartBadgeFloating.textContent = countItemsInStore;
            cartBadgeFloating.classList.toggle("bg-danger", countItemsInStore > 0);
            cartBadgeFloating.classList.toggle("bg-secondary", countItemsInStore === 0);
        }
    }

    function initializeCartBadges() {
        const floatingBadge = document.getElementById("cartBadgeFloating");
        const totalItems = parseInt(floatingBadge?.dataset.totalItems || 0);
        const storeItems = parseInt(floatingBadge?.textContent || 0);
        updateBadges(totalItems, storeItems);
    }
    initializeCartBadges();

    // =============================
    // 1. API Functions
    // =============================
    async function addToCart(productId, storeId, userId) {
        try {
            const res = await fetch(`/Product/AddProductToCart?buyer=${userId}`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ productId, storeId, quantity: 1 })
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const result = await res.json();

            console.log("✅ Added:", result);
            updateBadges(result.countItems || 0, result.countItemsInStore || 0);
            return result;
        } catch (err) {
            console.error("❌ Add error:", err);
            return null;
        }
    }

    async function updateQuantity(productId, storeId, userId, quantity, cartItemId) {
        try {
            // đổi query param: buyer thay vì item=userId
            const res = await fetch(`/Order/update-quantity?buyer=${userId}&store=${storeId}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ productId, quantity })
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const result = await res.json();

            console.log("✅ Updated:", result);

            // nếu backend chưa trả countItems, tự tính từ result.data
            let totalItems = 0, storeItems = 0;
            if (result.countItems !== undefined) {
                totalItems = result.countItems;
                storeItems = result.countItemsInStore;
            } else if (Array.isArray(result.data?.items)) {
                totalItems = result.data.items.reduce((s, i) => s + (i.quantity || 0), 0);
                storeItems = result.data.items
                    .filter(i => String(i.storeId) === String(storeId))
                    .reduce((s, i) => s + (i.quantity || 0), 0);
            }
            updateBadges(totalItems, storeItems);

            return result;
        } catch (err) {
            console.error("❌ Update error:", err);
            return null;
        }
    }

    async function removeItem(cartItemId, storeId, userId) {
        try {
            const res = await fetch(`/Order/delete-item?buyer=${userId}&item=${cartItemId}`, {
                method: "DELETE"
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const result = await res.json();

            console.log("🗑 Removed:", result);

            let totalItems = 0, storeItems = 0;
            if (Array.isArray(result.data?.items)) {
                totalItems = result.data.items.reduce((s, i) => s + (i.quantity || 0), 0);
                storeItems = result.data.items
                    .filter(i => String(i.storeId) === String(storeId))
                    .reduce((s, i) => s + (i.quantity || 0), 0);
            }
            updateBadges(totalItems, storeItems);

            return result;
        } catch (err) {
            console.error("❌ Remove error:", err);
            return null;
        }
    }

    // =============================
    // 2. Update Cart Total in Modal
    // =============================
    function updateCartTotal() {
        const modalItems = document.querySelectorAll("#modal-body-item-list .d-flex");
        let total = 0;

        modalItems.forEach(item => {
            const qtySpan = item.querySelector("[id^='qty-modal-']");
            const priceEl = item.querySelector("[data-price]");
            const price = parseInt(priceEl?.dataset.price || "0", 10);
            const qty = parseInt(qtySpan?.textContent || 0);
            total += price * qty;
        });

        const totalEl = document.getElementById("cart-total");
        if (totalEl) totalEl.textContent = `${total.toLocaleString()} đ`;
    }

    // =============================
    // 3. UI Helpers
    // =============================
    function resetCardToAddButton(productId, storeId) {
        const cardContainer = document.getElementById(`cart-actions-${productId}`);
        if (cardContainer) {
            cardContainer.innerHTML = `
                <button class="btn btn-primary btn-sm add-to-cart"
                        data-action="add"
                        data-product-id="${productId}"
                        data-store-id="${storeId}">+</button>`;
        }
    }

    function removeItemFromModal(productId) {
        const modalRow = document.querySelector(`#modal-body-item-list #qty-modal-${productId}`)?.closest(".border-bottom");
        if (modalRow) {
            modalRow.remove();
            updateCartTotal();
            const remainingItems = document.querySelectorAll("#modal-body-item-list .border-bottom");
            if (remainingItems.length === 0) {
                document.getElementById("modal-body-item-list").innerHTML =
                    `<p class="text-center text-muted">Giỏ hàng trống</p>`;
                document.getElementById("cart-total").textContent = "0 đ";
            }
        }
    }

    // =============================
    // 4. Quantity Handlers
    // =============================
    async function handleCardQuantity(productId, storeId, cartItemId, isIncrease, isAdd = false) {
        const cardContainer = document.getElementById(`cart-actions-${productId}`);
        const qtyEl = document.getElementById(`qty-card-${productId}`);
        let currentQty = parseInt(qtyEl?.textContent || 0);

        if (isAdd) {
            const result = await addToCart(productId, storeId, userId);
            if (result) {
                const newItem = result.cartStore?.items?.find(i => i.productId === productId);
                const newCartItemId = newItem?.cartItemId || "";
                cardContainer.innerHTML = `
                    <div class="d-flex justify-content-end align-items-center">
                        <button class="btn btn-sm btn-light border decrease"
                                data-product-id="${productId}"
                                data-store-id="${storeId}"
                                data-cart-item-id="${newCartItemId}">-</button>
                        <span class="mx-2 fw-bold qty-display" id="qty-card-${productId}">1</span>
                        <button class="btn btn-sm btn-light border increase"
                                data-product-id="${productId}"
                                data-store-id="${storeId}"
                                data-cart-item-id="${newCartItemId}">+</button>
                    </div>`;
                const modalQtyEl = document.getElementById(`qty-modal-${productId}`);
                if (modalQtyEl) {
                    modalQtyEl.textContent = "1";
                    updateCartTotal();
                }
            }
            return;
        }

        currentQty = isIncrease ? currentQty + 1 : currentQty - 1;
        if (currentQty <= 0) {
            const result = await removeItem(cartItemId, storeId, userId);
            if (result && result.statusCode === 200) {
                removeItemFromModal(productId);
                resetCardToAddButton(productId, storeId);
                updateCartTotal();
            }
        } else {
            const result = await updateQuantity(productId, storeId, userId, currentQty, cartItemId);
            if (result && qtyEl) {
                qtyEl.textContent = currentQty;
                const modalQtyEl = document.getElementById(`qty-modal-${productId}`);
                if (modalQtyEl) modalQtyEl.textContent = currentQty;
                updateCartTotal();
            }
        }
    }

    async function handleModalQuantity(productId, storeId, cartItemId, isIncrease) {
        const qtyEl = document.getElementById(`qty-modal-${productId}`);
        let currentQty = parseInt(qtyEl?.textContent || 0);

        currentQty = isIncrease ? currentQty + 1 : currentQty - 1;
        if (currentQty <= 0) {
            const result = await removeItem(cartItemId, storeId, userId);
            if (result && result.statusCode === 200) {
                removeItemFromModal(productId);
                resetCardToAddButton(productId, storeId);
                updateCartTotal();
            }
        } else {
            const result = await updateQuantity(productId, storeId, userId, currentQty, cartItemId);
            if (result && qtyEl) {
                qtyEl.textContent = currentQty;
                const cardQtyEl = document.getElementById(`qty-card-${productId}`);
                if (cardQtyEl) cardQtyEl.textContent = currentQty;
                updateCartTotal();
            }
        }
    }

    // =============================
    // 5. Event Listeners
    // =============================
    document.addEventListener("click", (e) => {
        const btn = e.target.closest(".add-to-cart, .increase, .decrease");
        if (!btn) return;
        const productId = btn.dataset.productId;
        const sId = btn.dataset.storeId;
        const cartItemId = btn.dataset.cartItemId;

        if (btn.classList.contains("add-to-cart")) {
            e.preventDefault();
            handleCardQuantity(productId, sId, null, false, true);
        } else if (btn.classList.contains("increase")) {
            e.preventDefault();
            handleCardQuantity(productId, sId, cartItemId, true);
        } else if (btn.classList.contains("decrease")) {
            e.preventDefault();
            handleCardQuantity(productId, sId, cartItemId, false);
        }
    });

    const cartModal = document.getElementById("cartItemModal");
    if (cartModal) {
        cartModal.addEventListener("click", (e) => {
            const btn = e.target.closest("#modal-body-item-list .increase, #modal-body-item-list .decrease");
            if (!btn) return;
            const productId = btn.dataset.productId;
            const sId = btn.dataset.storeId;
            const cartItemId = btn.dataset.cartItemId;

            if (btn.classList.contains("increase")) {
                e.preventDefault();
                handleModalQuantity(productId, sId, cartItemId, true);
            } else if (btn.classList.contains("decrease")) {
                e.preventDefault();
                handleModalQuantity(productId, sId, cartItemId, false);
            }
        });
    }

    // =============================
    // 6. Reload modal cart on open
    // =============================
    async function reloadModalCart() {
        try {
            const res = await fetch(`/Order/get-cart-in-store?buyer=${userId}&storeId=${storeId}`);
            if (res.ok) {
                const result = await res.json();
                console.log("🛒 Items in store:", result.data);
                renderCartItems(result.data || []);
            }
        } catch (err) {
            console.error("Network error:", err);
        }
    }
    if (cartModal) {
        cartModal.addEventListener("show.bs.modal", reloadModalCart);
    }

    function renderCartItems(items) {
        const container = document.getElementById("modal-body-item-list");
        let total = 0;
        if (!items || items.length === 0) {
            container.innerHTML = `<p class="text-center text-muted">Giỏ hàng trống</p>`;
            document.getElementById("cart-total").textContent = "0 đ";
            return;
        }

        //const activeItems = items.filter(item => item.isAvailable === true || item.isActive === true);

        //if (activeItems.length === 0) {
        //    container.innerHTML = `<p class="text-center text-muted">Giỏ hàng trống</p>`;
        //    document.getElementById("cart-total").textContent = "0 đ";
        //    return;
        //}

        let html = "";
        items.forEach(item => {
            total += item.isAvailable ? item.price * item.quantity : 0;
            html += `
                <div class="d-flex align-items-center border-bottom py-2">
                    <div class="flex-shrink-0">
                        <img src="${item.productImage}" alt="${item.productName}"
                             class="img-thumbnail" style="width:60px; height:60px; object-fit:cover;">
                    </div>
                    <div class="flex-grow-1 ms-3">
                        <h6 class="mb-1">${item.productName}</h6>
                        ${item.errorMessage ? `<p class="text-danger mb-1">${item.errorMessage}</p>` : ""}
                        <div class="fw-bold text-danger mt-1" data-price="${item.price}">
                            ${item.price.toLocaleString()} đ
                        </div>
                    </div>
                    <div class="d-flex align-items-center">
                        <button class="btn btn-sm btn-outline-secondary decrease"
                                data-product-id="${item.productId}"
                                data-cart-item-id="${item.cartItemId}"
                                data-store-id="${item.storeId}">-</button>
                        <span class="mx-2 fw-bold" id="qty-modal-${item.productId}">${item.quantity}</span>
                        <button class="btn btn-sm btn-outline-secondary increase"
                                data-product-id="${item.productId}"
                                data-cart-item-id="${item.cartItemId}"
                                data-store-id="${item.storeId}">+</button>
                    </div>
                </div>`;
        });
        container.innerHTML = html;
        document.getElementById("cart-total").textContent = `${total.toLocaleString()} đ`;
    }

    // =============================
    // 7. Checkout
    // =============================
    const checkoutBtn = document.getElementById("btnCheckout");
    if (checkoutBtn) {
        checkoutBtn.addEventListener("click", handleCheckoutClick);
    }

    async function handleCheckoutClick() {
        checkoutBtn.disabled = true;
        const originalHtml = checkoutBtn.innerHTML;
        checkoutBtn.innerHTML = "Đang kiểm tra...";

        try {
            const res = await fetch(`/Order/get-cart?id=${userId}`);
            if (!res.ok) {
                alert("❌ Lỗi khi kiểm tra giỏ hàng.");
                return;
            }
            const result = await res.json();
            const cart = result?.data || { items: [] };
            const allItems = Array.isArray(cart.items) ? cart.items : [];
            const items = allItems.filter(it => it.storeId == storeId);

            renderCartItems(items);

            if (items.length === 0) {
                showErrorModal("<p><strong>Không có sản phẩm trong giỏ hàng.</strong></p>");
                return;
            }
            const invalidItems = items.filter(it => !it.isAvailable);
            if (invalidItems.length > 0) {
                const html = invalidItems.map(it =>
                    `<p><strong>${it.productName}</strong>: ${it.errorMessage || "Ngừng bán"}</p>`
                ).join("");
                showErrorModal(html);
                return;
            }

            const checkoutForm = document.getElementById("checkoutForm");
            if (checkoutForm) {
                // clear input cũ
                checkoutForm.querySelectorAll('input[name="selectedProducts"]').forEach(n => n.remove());
                checkoutForm.innerHTML = `<input type="hidden" name="userId" value="${userId}">`;
                items.forEach(it => {
                    const hidden = document.createElement("input");
                    hidden.type = "hidden";
                    hidden.name = "selectedProducts";
                    hidden.value = it.productId;
                    checkoutForm.appendChild(hidden);
                });
                checkoutForm.submit();
            } else {
                window.location.href = "/Order/CreateOrder";
            }
        } catch (err) {
            console.error("Check cart error:", err);
            alert("❌ Có lỗi mạng khi lấy giỏ hàng.");
        } finally {
            checkoutBtn.disabled = false;
            checkoutBtn.innerHTML = originalHtml;
        }
    }

    function showErrorModal(html) {
        const msgEl = document.getElementById("modalErrorMessage");
        const modalEl = document.getElementById("errorModal");
        if (msgEl && modalEl && typeof bootstrap !== "undefined" && bootstrap.Modal) {
            msgEl.innerHTML = html;
            new bootstrap.Modal(modalEl).show();
        } else {
            alert(html.replace(/<[^>]*>/g, ""));
        }
    }

    // =============================
    // 8. Scroll to Category
    // =============================
    document.querySelectorAll("#categoryList .category-link").forEach(link => {
        link.addEventListener("click", () => {
            const targetId = link.getAttribute("data-target");
            const targetEl = document.getElementById(targetId);
            if (targetEl) {
                targetEl.scrollIntoView({ behavior: "smooth", block: "start" });
                document.querySelectorAll("#categoryList .category-link")
                    .forEach(el => el.classList.remove("active"));
                link.classList.add("active");
            }
        });
    });
});
