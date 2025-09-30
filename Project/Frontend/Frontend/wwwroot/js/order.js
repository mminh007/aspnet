document.addEventListener("DOMContentLoaded", () => {
    let currentOrderId = null;

    document.querySelectorAll(".view-details").forEach(btn => {
        btn.addEventListener("click", () => {
            currentOrderId = btn.dataset.orderId;

            document.getElementById("modalOrderId").textContent = btn.dataset.orderId;
            document.getElementById("modalOrderDate").textContent = btn.dataset.orderDate;
            document.getElementById("modalOrderStatus").textContent = btn.dataset.orderStatus;
            document.getElementById("modalOrderTotal").textContent = btn.dataset.orderTotal;

            const items = JSON.parse(btn.dataset.orderItems);
            const itemsContainer = document.getElementById("modalOrderItems");
            itemsContainer.innerHTML = "";
            items.forEach(it => {
                itemsContainer.innerHTML += `
                    <li class="mb-2 d-flex align-items-center">
                        <img src="${it.ProductImage}" alt="${it.ProductName}" 
                             style="width:40px; height:40px; object-fit:cover;" />
                        <span class="ms-2">${it.ProductName} (x${it.Quantity}) - ${Number(it.Price).toLocaleString()} đ</span>
                    </li>`;
            });
        });
    });

    document.getElementById("btnCancelOrder").addEventListener("click", async () => {
        if (!currentOrderId) return;
        if (!confirm("Bạn có chắc muốn hủy đơn hàng này?")) return;

        try {
            const response = await fetch(`/Order/DeleteOrder?orderId=${currentOrderId}`, { method: "DELETE" });
            if (response.ok) {
                alert("Đơn hàng đã được hủy.");
                location.reload();
            } else {
                alert("Không thể hủy đơn hàng.");
            }
        } catch (err) {
            console.error("Cancel error:", err);
            alert("Có lỗi xảy ra.");
        }
    });
});
