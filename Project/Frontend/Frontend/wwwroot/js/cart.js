// wwwroot/js/cart.js

document.addEventListener("DOMContentLoaded", () => {
    let errorMessages = [];

    // ErrorMessage Modal
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

    // Increase/Decrease Quantity
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
                        cart.items.forEach(it => {
                            const qtySpan = document.getElementById(`qty-${it.cartItemId}`);
                            const subtotalCell = document.getElementById(`subtotal-${it.cartItemId}`);
                            const statusBadge = document.getElementById(`status-${it.cartItemId}`);

                            if (qtySpan) qtySpan.textContent = it.quantity;
                            if (subtotalCell) {
                                subtotalCell.textContent = it.isAvailable
                                    ? (it.price * it.quantity).toLocaleString() + " đ"
                                    : "0 đ";
                            }

                            const errMsg = it.errorMessage || it.ErrorMessage || "";
                            if (statusBadge) {
                                statusBadge.className = "badge " + (it.isAvailable ? "bg-success" : "bg-secondary");
                                statusBadge.textContent = it.isAvailable ? "Còn hàng" : errMsg;
                            }

                            if (it.cartItemId == cartItemId && errMsg) {
                                document.getElementById("modalErrorMessage").innerHTML =
                                    `<p><strong>${it.productName || it.ProductName}:</strong> ${errMsg}</p>`;

                                const modal = new bootstrap.Modal(document.getElementById("errorModal"));
                                modal.show();
                            }
                        });

                        document.getElementById("totalItems").innerHTML =
                            `<strong>Tổng sản phẩm:</strong> ${cart.totalItems}`;
                        document.getElementById("totalPrice").innerHTML =
                            `<strong>Tổng tiền:</strong> ${cart.totalPrice.toLocaleString()} đ`;
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

    // Delete Item
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
                        location.reload();
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

    // Chọn tất cả
    document.getElementById("selectAll")?.addEventListener("change", function () {
        document.querySelectorAll(".product-checkbox").forEach(cb => {
            cb.checked = this.checked;
        });
    });
});
