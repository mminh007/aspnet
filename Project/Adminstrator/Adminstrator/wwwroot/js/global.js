// wwwroot/js/global.js
console.log("[global.js] loaded at", new Date().toISOString());

window.showAlert = function (message, type = "success") {
    // tạo container nếu chưa có
    let container = document.querySelector(".custom-alert-container");
    if (!container) {
        container = document.createElement("div");
        container.className = "custom-alert-container";
        document.body.appendChild(container);
    }

    // tạo alert
    const el = document.createElement("div");
    el.className = `custom-alert ${type}`;
    el.innerHTML = `
        <div class="alert-content">${message}</div>
        <button class="alert-close" aria-label="Close">&times;</button>
    `;
    container.appendChild(el);

    // hiệu ứng hiện
    requestAnimationFrame(() => el.classList.add("show"));

    // tự đóng sau 3s
    const timer = setTimeout(close, 5000);

    // cho phép bấm đóng
    el.querySelector(".alert-close").addEventListener("click", close);

    function close() {
        clearTimeout(timer);
        el.classList.remove("show");
        setTimeout(() => el.remove(), 220);
    }
};
