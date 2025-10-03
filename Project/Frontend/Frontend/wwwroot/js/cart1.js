document.addEventListener("DOMContentLoaded", () => {
    let errorMessages = [];

    // =========================
    // Hiển thị errorMessage ngay khi load page
    // =========================
    if (window.cartItems) {
        cartItems.forEach(item => {
            if (item.isAvailable && item.errorMessage) {
                errorMessages.push(item.errorMessage);
            }
        });
    }

    if (errorMessages.length > 0) {
        document.getElementById("modalErrorMessage").innerHTML =
            errorMessages.map((msg, idx) => `<p><strong>Lỗi ${idx + 1}:</strong> ${msg}</p>`).join("");

        const modal = new bootstrap.Modal(document.getElementById("errorModal"));
        modal.show();
    }

    // =========================
    // Tăng / Giảm số lượng
    // =========================
    document.querySelectorAll(".quantity-btn").forEach(btn => {
        btn.addEventListener("click", async () => {
            const userId = btn.dataset.userId;
            const cartItemId = btn.dataset.itemId;
            const productId = btn.dataset.productId;
            const qtySpan = document.getElementById(`qty-${cartItemId}`);
            let currentQty = parseInt(qtySpan.textContent);

            const parentDiv = btn.closest(".d-flex");
            const allBtns = parentDiv.querySelectorAll(".quantity-btn");
            allBtns.forEach(b => b.disabled = true);

            qtySpan.setAttribute("data-old", qtySpan.textContent);
            qtySpan.textContent = "...";

            if (btn.classList.contains("increase")) {
                currentQty++;
            } else if (btn.classList.contains("decrease") && currentQty > 1) {
                currentQty--;
            }

            try {
                const response = await fetch(`/Order/update-quantity?buyer=${userId}&item=${cartItemId}`, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        productId: productId,
                        quantity: currentQty
                    })
                });

                if (response.ok) {
                    const result = await response.json();
                    const cart = result.data;

                    if (cart && cart.items) {
                        updateCartUI(cart);

                        // nếu sản phẩm vừa thao tác có lỗi thì hiện modal
                        const current = cart.items.find(it => it.cartItemId === cartItemId);
                        if (current && current.errorMessage) {
                            document.getElementById("modalErrorMessage").innerHTML =
                                `<p><strong>${current.productName}:</strong> ${current.errorMessage}</p>`;
                            const modal = new bootstrap.Modal(document.getElementById("errorModal"));
                            modal.show();
                        }

                        // đồng bộ lại cartItems
                        cartItems = cart.items;
                    }
                } else {
                    qtySpan.textContent = qtySpan.getAttribute("data-old");
                    alert("❌ Lỗi cập nhật giỏ hàng.");
                }
            } catch (err) {
                qtySpan.textContent = qtySpan.getAttribute("data-old");
                console.error("Network error:", err);
                alert("❌ Có lỗi mạng. Thử lại sau.");
            } finally {
                allBtns.forEach(b => b.disabled = false);
            }
        });
    });

    // =========================
    // Xóa sản phẩm trong giỏ
    // =========================
    document.querySelectorAll(".delete-btn").forEach(btn => {
        btn.addEventListener("click", async () => {
            const userId = btn.dataset.userId;
            const cartItemId = btn.dataset.itemId;

            if (!confirm("Bạn có chắc muốn xóa sản phẩm này?")) return;

            try {
                const response = await fetch(`/Order/delete-item?buyer=${userId}&item=${cartItemId}`, {
                    method: "DELETE"
                });

                if (response.ok) {
                    const result = await response.json();
                    const cart = result.data;
                    if (cart && cart.items) {
                        location.reload(); // reload lại để render mới
                    }
                } else {
                    const result = await response.json();
                    alert("❌ " + (result.message || "Không thể xóa sản phẩm."));
                }
            } catch (err) {
                console.error("Delete error:", err);
                alert("❌ Có lỗi mạng khi xóa sản phẩm.");
            }
        });
    });

    // =========================
    // Check giỏ hàng mới nhất trước khi tạo đơn hàng
    // =========================
    const checkoutForm = document.getElementById("checkoutForm");
    if (checkoutForm) {
        checkoutForm.addEventListener("submit", async (e) => {
            console.log("🚀 Intercepted submit event!");
            e.preventDefault(); // chặn submit mặc định

            const userId = checkoutForm.querySelector("input[name='userId']").value;
            console.log("🔎 Đang kiểm tra giỏ hàng mới nhất... userId=", userId);

            try {
                const response = await fetch(`/Order/get-cart?id=${userId}`);
                if (response.ok) {
                    const result = await response.json();
                    const newCart = result.data;
                    console.log("📦 Cart mới nhất từ server:", newCart);

                    let hasChanges = false;

                    if (newCart && newCart.items) {
                        newCart.items.forEach(it => {
                            const old = cartItems.find(x => x.cartItemId === it.cartItemId);
                            if (!old) {
                                hasChanges = true; // có item mới
                            } else if (
                                old.price !== it.price ||
                                old.quantity !== it.quantity ||
                                old.isAvailable !== it.isAvailable ||
                                old.errorMessage !== it.errorMessage
                            ) {
                                hasChanges = true;
                            }
                        });
                    }

                    if (hasChanges) {
                        console.log("⚠️ Có thay đổi trong giỏ hàng, show modal...");
                        document.getElementById("modalErrorMessage").innerHTML =
                            `<p><strong>Một vài sản phẩm vừa thay đổi thông tin.</strong></p>
                             <p>Vui lòng kiểm tra lại giỏ hàng trước khi tiếp tục.</p>`;

                        const modal = new bootstrap.Modal(document.getElementById("errorModal"));
                        modal.show();

                        // Khi modal đóng -> render lại giỏ hàng từ newCart
                        document.getElementById("errorModal")
                            .addEventListener("hidden.bs.modal", () => {
                                updateCartUI(newCart);
                                cartItems = newCart.items;
                            }, { once: true });
                    } else {
                        console.log("✅ Giỏ hàng không thay đổi, tiếp tục submit...");
                        //checkoutForm.submit();
                        //checkoutForm.requestSubmit();
                        HTMLFormElement.prototype.submit.call(checkoutForm);
                    }
                } else {
                    alert("❌ Lỗi khi kiểm tra giỏ hàng mới nhất.");
                }
            } catch (err) {
                console.error("Check cart error:", err);
                alert("❌ Có lỗi mạng khi lấy giỏ hàng.");
            }
        });
    }

    // =========================
    // Chọn tất cả
    // =========================
    document.getElementById("selectAll")?.addEventListener("change", function () {
        document.querySelectorAll(".product-checkbox").forEach(cb => {
            cb.checked = this.checked;
        });
    });

    // =========================
    // Hàm update lại UI từ cart mới
    // =========================
    function updateCartUI(cart) {
        if (!cart || !cart.items) return;

        cart.items.forEach(it => {
            const qtySpan = document.getElementById(`qty-${it.cartItemId}`);
            if (qtySpan) qtySpan.textContent = it.quantity;

            const subtotal = document.getElementById(`subtotal-${it.cartItemId}`);
            if (subtotal) subtotal.textContent =
                it.isAvailable ? (it.price * it.quantity).toLocaleString() + " đ" : "0 đ";

            const statusBadge = document.getElementById(`status-${it.cartItemId}`);
            if (statusBadge) {
                statusBadge.className = "badge " + (it.isAvailable ? "bg-success" : "bg-secondary");
                statusBadge.textContent = it.isAvailable ? "Còn hàng" : (it.errorMessage || "");
            }

            if (!it.isAvailable) {
                const checkbox = document.querySelector(`input[value="${it.productId}"]`);
                if (checkbox) checkbox.checked = false;
            }

            // render errorMessage dưới tên sp
            const nameCell = document.querySelector(`#status-${it.cartItemId}`).closest("td.text-start");
            if (nameCell) {
                let errSpan = nameCell.querySelector(".error-msg");
                if (it.errorMessage) {
                    if (!errSpan) {
                        errSpan = document.createElement("p");
                        errSpan.className = "text-danger error-msg mt-1 mb-0";
                        nameCell.appendChild(errSpan);
                    }
                    errSpan.textContent = it.errorMessage;
                } else if (errSpan) {
                    errSpan.remove();
                }
            }
        });

        document.getElementById("totalItems").innerHTML =
            `<strong>Tổng sản phẩm:</strong> ${cart.totalItems}`;
        document.getElementById("totalPrice").innerHTML =
            `<strong>Tổng tiền:</strong> ${cart.totalPrice.toLocaleString()} đ`;
    }
});
