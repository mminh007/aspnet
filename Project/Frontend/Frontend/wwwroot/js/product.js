document.addEventListener("DOMContentLoaded", () => {
    const floatingBtn = document.querySelector(".floating-cart-btn");
    const userId = floatingBtn?.dataset.userId || "";
    const storeId = floatingBtn?.dataset.storeId || "";
    

    //if (!userId || !storeId) {
    //    console.error("❌ Missing userId or storeId");
    //    console.log("⚠️ Script stopped early because userId or storeId missing:", { userId, storeId });

    //    return;
    //}

    const isLoggedIn = typeof window.isLoggedIn === "boolean" ? window.isLoggedIn : false;
    const loginUrl = window.loginUrl || "/Authentication/login";

    if (!userId || !storeId) {
        console.warn("⚠️ Missing userId/storeId (có thể do chưa login). Vẫn tiếp tục để chặn và chuyển sang login khi người dùng bấm +.");
    }

    // =============================
    // 0. Initialize badges
    // =============================
    function updateBadges(totalItems, countItemsInStore) {
        const cartBadgeFloating = document.getElementById("cartBadgeFloating");
        if (cartBadgeFloating) {
            cartBadgeFloating.textContent = countItemsInStore;
            cartBadgeFloating.dataset.totalItems = totalItems;

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
    // 0.1 Custom Alert
    // =============================
    //function showAlert(message, type = "success") {
    //    // tạo container nếu chưa có
    //    let container = document.querySelector(".custom-alert-container");
    //    if (!container) {
    //        container = document.createElement("div");
    //        container.className = "custom-alert-container";
    //        document.body.appendChild(container);
    //    }

    //    // tạo alert
    //    const el = document.createElement("div");
    //    el.className = `custom-alert ${type}`;
    //    el.innerHTML = `
    //        <div class="alert-content">${message}</div>
    //        <button class="alert-close" aria-label="Close">&times;</button>
    //        `;
    //    container.appendChild(el);

    //    // show
    //    requestAnimationFrame(() => el.classList.add("show"));

    //    // auto close sau 3s
    //    const timer = setTimeout(close, 3000);

    //    // đóng khi bấm nút
    //    el.querySelector(".alert-close").addEventListener("click", close);

    //    function close() {
    //        clearTimeout(timer);
    //        el.classList.remove("show");
    //        setTimeout(() => el.remove(), 220);
    //    }
    //}


    // =============================
    // 0.2 Load Provinces & Wards (dropdown custom, không dùng <select>)
    // =============================
    let VN_PROVINCES = [];
    let VN_WARDS = [];

    async function loadLocations() {
        try {
            const [pRes, wRes] = await Promise.all([
                fetch("/data/vn-provinces.json"),
                fetch("/data/vn-wards.json")
            ]);
            if (!pRes.ok || !wRes.ok) throw new Error("Cannot load provinces/wards data");

            VN_PROVINCES = await pRes.json();
            VN_WARDS = await wRes.json();

            const provinceList = document.getElementById("provinceList");
            const wardList = document.getElementById("wardList");
            if (!provinceList || !wardList) return;

            // render tỉnh
            provinceList.innerHTML = VN_PROVINCES.map(
                p => `<li><button type="button" class="dropdown-item" data-code="${p.code}" data-name="${p.name}">${p.name}</button></li>`
            ).join("");

            // chọn tỉnh
            provinceList.addEventListener("click", e => {
                const item = e.target.closest(".dropdown-item");
                if (!item) return;
                const code = item.dataset.code;
                const name = item.dataset.name;

                document.getElementById("ship-province").value = code;
                document.getElementById("provinceText").textContent = name;

                // reset ward
                document.getElementById("ship-ward").value = "";
                document.getElementById("wardText").textContent = "-- Chọn Phường/Xã --";
                document.getElementById("wardBtn").disabled = false;

                const wards = VN_WARDS.filter(w => String(w.provinceCode) === String(code));
                wardList.innerHTML = wards.map(
                    w => `<li><button type="button" class="dropdown-item" data-code="${w.code}" data-name="${w.name}">${w.name}</button></li>`
                ).join("");
            });

            // chọn phường/xã
            wardList.addEventListener("click", e => {
                const item = e.target.closest(".dropdown-item");
                if (!item) return;
                const code = item.dataset.code;
                const name = item.dataset.name;

                document.getElementById("ship-ward").value = code;
                document.getElementById("wardText").textContent = name;
            });
        } catch (e) {
            console.error("Load locations error:", e);
        }
    }
    loadLocations();


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

            console.log("✅ Added result:", JSON.stringify(result, null, 2));
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
            const res = await fetch(`/Order/update-qty?buyer=${userId}&id=${cartItemId}&store=${storeId}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ productId, quantity, cartItemId })
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
            } else {
                // ⚠️ Fallback: Tính lại từ DOM sau khi xóa
                const currentBadge = document.getElementById("cartBadgeFloating");
                const oldTotal = parseInt(currentBadge?.dataset.totalItems || 0);
                const oldStore = parseInt(currentBadge?.textContent || 0);

                // Lấy quantity của item vừa xóa
                //const deletedQty = parseInt(document.getElementById(`qty-modal-${productId}`)?.textContent || 0);

                //totalItems = Math.max(0, oldTotal - deletedQty);
                //storeItems = Math.max(0, oldStore - deletedQty);

                totalItems = Math.max(0, oldTotal - 1);
                storeItems = Math.max(0, oldStore - 1);
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

        //const btnInDom = cardContainer?.querySelector('button.increase, button.decrease');
        //const cartItemId = btnInDom?.dataset.cartItemId;
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

                const mob = document.getElementById(`cart-actions-mobile-${productId}`);
                if (mob) {
                    mob.innerHTML = `
                    <div class="d-flex align-items-center">
                      <button class="btn btn-sm btn-light border decrease"
                              data-product-id="${productId}" data-store-id="${storeId}"
                              data-cart-item-id="${newCartItemId}">-</button>
                      <span class="mx-2 fw-bold" id="qty-card-mobile-${productId}">1</span>
                      <button class="btn btn-sm btn-light border increase"
                              data-product-id="${productId}" data-store-id="${storeId}"
                              data-cart-item-id="${newCartItemId}">+</button>
                    </div>`;
                }

                const modalRow = document.querySelector(`#modal-body-item-list #qty-modal-${productId}`)?.closest(".border-bottom");
                if (modalRow) {
                    modalRow.querySelectorAll(`button.increase, button.decrease`).forEach(btn => {
                        btn.dataset.cartItemId = newCartItemId;
                    });
                }

                

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

                // Cập nhật nút ở mobile card
                const mob = document.getElementById(`cart-actions-mobile-${productId}`);
                if (mob) {
                    mob.innerHTML = `
                        <button class="btn btn-primary btn-sm add-to-cart"
                                data-product-id="${productId}" data-store-id="${storeId}">+</button>`;
                }
                updateCartTotal();
            }
        } else {
            const result = await updateQuantity(productId, storeId, userId, currentQty, cartItemId);
            if (result && qtyEl) {
                qtyEl.textContent = currentQty;

                const mobileQtyEl = document.getElementById(`qty-card-mobile-${productId}`);
                if (mobileQtyEl) mobileQtyEl.textContent = currentQty;

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

        console.log('[AUTH CHECK]', { userId, isLoggedIn, loginUrl });
        console.log('[LOGIN REDIRECT]', {
            href: window.location.href,
            relative: window.location.pathname + window.location.search + window.location.hash
        });

        if (!userId) {
            e.preventDefault();
            showAlert("Bạn hãy đăng nhập trước khi mua hàng", "warning"); 
            //alert("Bạn hãy đăng nhập trước khi mua hàng");
            const ret = encodeURIComponent(
                window.location.pathname + window.location.search + window.location.hash
            );
            setTimeout(() => {
                window.location.href = `${loginUrl}?returnUrl=${ret}`;
            }, 2000);
            
            return;
        }

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

        // Khi modal show lên thì load lại dữ liệu tỉnh/phường
        cartModal.addEventListener("show.bs.modal", () => {
            if (!VN_PROVINCES.length) {
                loadLocations();
            }
        });

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
            const fullname = (document.getElementById("ship-fullname")?.value || "").trim();
            const phone = (document.getElementById("ship-phone")?.value || "").trim();
            const addressLine = (document.getElementById("ship-addressline")?.value || "").trim();
            const provinceCode = document.getElementById("ship-province")?.value || "";
            const wardCode = document.getElementById("ship-ward")?.value || "";
            const note = (document.getElementById("ship-note")?.value || "").trim();

            const provinceName = VN_PROVINCES.find(p => p.code === provinceCode)?.name || "";
            const wardName = VN_WARDS.find(w => w.code === wardCode)?.name || "";

            const fullAddress = [addressLine, wardName, provinceName].filter(Boolean).join(", ");

            const phoneOk = /^(\+?\d{1,3}[- ]?)?\d{9,11}$/.test(phone);
            if (!fullname || !phone || !addressLine || !provinceCode || !wardCode) {
                showErrorModal("<p>Vui lòng nhập đầy đủ <strong>tên</strong>, <strong>điện thoại</strong>, <strong>địa chỉ</strong>, <strong>Tỉnh</strong> và <strong>Phường</strong>.</p>");
                return;
            }
            if (!phoneOk) {
                showErrorModal("<p>Số điện thoại không hợp lệ.</p>");
                return;
            }

            const res = await fetch(`/Order/get-cart?id=${userId}`);
            if (!res.ok) {
                showAlert("❌ Lỗi khi lấy giỏ hàng.");
                //alert("❌ Lỗi khi kiểm tra giỏ hàng.");
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
                //checkoutForm.innerHTML = `<input type="hidden" name="userId" value="${userId}">`;

                let userIdInput = checkoutForm.querySelector('input[name="userId"]');
                if (!userIdInput) {
                    userIdInput = document.createElement("input");
                    userIdInput.type = "hidden";
                    userIdInput.name = "userId";
                    checkoutForm.appendChild(userIdInput);
                }
                userIdInput.value = userId;

                items.forEach(it => {
                    const hidden = document.createElement("input");
                    hidden.type = "hidden";
                    hidden.name = "selectedProducts";
                    hidden.value = it.productId;
                    checkoutForm.appendChild(hidden);
                });

                const provinceName = VN_PROVINCES.find(p => p.code === (document.getElementById("ship-province")?.value || ""))?.name || "";
                const wardName = VN_WARDS.find(w => w.code === (document.getElementById("ship-ward")?.value || ""))?.name || "";
                const addrLine = (document.getElementById("ship-addressline")?.value || "").trim();
                const fullAddress = [addrLine, wardName, provinceName].filter(Boolean).join(", ");

                document.getElementById("ship-fulladdress").value = fullAddress;

                const fd = new FormData(checkoutForm);
                console.log("== FormData Preview ==");
                for (const [k, v] of fd.entries()) {
                    console.log(k, v);
                }

                checkoutForm.submit();
            } else {
                window.location.href = "/Order/CreateOrder";
            }
        } catch (err) {
            console.error("Check cart error:", err);
            showAlert("❌ Có lỗi mạng khi lấy giỏ hàng.");
            //alert("❌ Có lỗi mạng khi lấy giỏ hàng.");
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
            showAlert(html.replace(/<[^>]*>/g, ""), "error");
            //alert(html.replace(/<[^>]*>/g, ""));
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
