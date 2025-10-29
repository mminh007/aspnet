document.addEventListener("DOMContentLoaded", () => {
    let errorMessages = [];

    // =========================
    // 1. Hiển thị errorMessage khi load
    // =========================
    if (window.cartItems) {
        cartItems.forEach(item => {
            if (!item.isAvailable && item.errorMessage) {
                errorMessages.push(`${item.productName}: ${item.errorMessage}`);
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
    // 2. Tăng / Giảm số lượng
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

                        // nếu sp này có lỗi thì show modal
                        const current = cart.items.find(it => it.cartItemId === cartItemId);
                        if (current && current.errorMessage) {
                            document.getElementById("modalErrorMessage").innerHTML =
                                `<p><strong>${current.productName}:</strong> ${current.errorMessage}</p>`;
                            const modal = new bootstrap.Modal(document.getElementById("errorModal"));
                            modal.show();
                        }

                        cartItems = cart.items; // sync lại
                    }
                } else {
                    qtySpan.textContent = qtySpan.getAttribute("data-old");
                    showAlert("❌ Lỗi cập nhật giỏ hàng.", "error")
                    //alert("❌ Lỗi cập nhật giỏ hàng.");
                }
            } catch (err) {
                qtySpan.textContent = qtySpan.getAttribute("data-old");
                console.error("Network error:", err);
                showAlert("❌ Có lỗi mạng. Thử lại sau.", "error")
                //alert("❌ Có lỗi mạng. Thử lại sau.");
            } finally {
                allBtns.forEach(b => b.disabled = false);
            }
        });
    });

    // =========================
    // 3. Xóa sản phẩm
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
                    location.reload();
                } else {
                    const result = await response.json();
                    showAlert("❌ " + (result.message || "Không thể xóa sản phẩm."), "error")
                    //alert("❌ " + (result.message || "Không thể xóa sản phẩm."));
                }
            } catch (err) {
                console.error("Delete error:", err);
                showAlert("❌ Có lỗi mạng khi xóa sản phẩm.", "error")
                //alert("❌ Có lỗi mạng khi xóa sản phẩm.");
            }
        });
    });

    // =========================
    // 4. Check giỏ hàng trước khi tạo đơn
    // =========================
    const checkoutForm = document.getElementById("checkoutForm");
    if (checkoutForm) {
        checkoutForm.addEventListener("submit", async (e) => {
            e.preventDefault();

            const userId = checkoutForm.querySelector("input[name='userId']").value;

            try {
                const response = await fetch(`/Order/get-cart?id=${userId}`);
                if (response.ok) {
                    const result = await response.json();
                    const newCart = result.data;

                    if (newCart && newCart.items) {
                        const invalidItems = newCart.items.filter(it => !it.isAvailable);

                        if (invalidItems.length > 0) {
                            document.getElementById("modalErrorMessage").innerHTML =
                                invalidItems.map(it => `<p><strong>${it.productName}</strong>: ${it.errorMessage || "Ngừng bán"}</p>`).join("");

                            const modal = new bootstrap.Modal(document.getElementById("errorModal"));
                            modal.show();
                            return; // ❌ chặn submit
                        }
                    }

                    // Nếu ok thì submit form thật
                    HTMLFormElement.prototype.submit.call(checkoutForm);
                } else {
                    showAlert("❌ Lỗi khi kiểm tra giỏ hàng mới nhất.", "error")
                    //alert("❌ Lỗi khi kiểm tra giỏ hàng mới nhất.");
                }
            } catch (err) {
                console.error("Check cart error:", err);
                showAlert("❌ Có lỗi mạng khi lấy giỏ hàng.", "error")
                //alert("❌ Có lỗi mạng khi lấy giỏ hàng.");
            }
        });
    }

    // =========================
    // 5. Chọn tất cả
    // =========================
    document.getElementById("selectAll")?.addEventListener("change", function () {
        document.querySelectorAll(".product-checkbox").forEach(cb => {
            cb.checked = this.checked;
        });
    });

    // =========================
    // 6. Update UI giỏ hàng
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
                statusBadge.textContent = it.isAvailable ? "Còn hàng" : (it.errorMessage || "Ngừng bán");
            }

            if (!it.isAvailable) {
                const checkbox = document.querySelector(`input[value="${it.productId}"]`);
                if (checkbox) {
                    checkbox.checked = false;
                    checkbox.disabled = true; // disable luôn
                }
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


