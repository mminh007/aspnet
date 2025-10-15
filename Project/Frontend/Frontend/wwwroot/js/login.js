let countdownInterval;
let remainingTime = 600; // 10 phút
let email = "";

// Khởi động countdown
function startCountdown() {
    clearInterval(countdownInterval);
    countdownInterval = setInterval(() => {
        remainingTime--;
        const minutes = Math.floor(remainingTime / 60);
        const seconds = remainingTime % 60;
        const countdownElem = document.getElementById("countdown");
        const textElem = document.getElementById("countdownText");
        const resendBtn = document.getElementById("resendBtn");

        if (countdownElem) {
            countdownElem.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        }

        if (remainingTime <= 0) {
            clearInterval(countdownInterval);
            if (resendBtn) resendBtn.disabled = false;
            if (textElem) textElem.textContent = "Mã đã hết hạn!";
        }
    }, 1000);
}

// DOM loaded
document.addEventListener("DOMContentLoaded", () => {
    // ✅ Đọc từ biến global đã được set trong View
    const showVerifyModal = window.showVerifyModal || false;
    email = window.unverifiedEmail || "";

    console.log("showVerifyModal:", showVerifyModal); // Debug
    console.log("email:", email); // Debug

    // Nếu cần mở modal verify
    if (showVerifyModal && email) {
        const modalEl = document.getElementById('verifyEmailModal');
        const emailElem = document.getElementById("verifyEmailAddress");
        if (emailElem) emailElem.textContent = email;

        if (modalEl) {
            const modal = new bootstrap.Modal(modalEl);
            modal.show();
            startCountdown();
        }
    }

    // Tự động nhảy sang ô kế tiếp khi nhập mã
    document.querySelectorAll(".code-input").forEach((input, index, arr) => {
        input.addEventListener("input", (e) => {
            if (e.target.value && index < arr.length - 1) arr[index + 1].focus();
        });
    });

    // Nút gửi lại mã
    const resendBtn = document.getElementById("resendBtn");
    if (resendBtn) {
        resendBtn.addEventListener("click", async () => {
            resendBtn.disabled = true;
            try {
                const res = await fetch('/Authentication/login-resend-code', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email })
                });

                if (res.ok) {
                    remainingTime = 600;
                    startCountdown();
                    alert("✅ Mã xác minh mới đã được gửi!");
                } else {
                    alert("⚠️ Gửi lại mã thất bại!");
                    resendBtn.disabled = false;
                }
            } catch (err) {
                console.error("Resend error:", err);
                alert("❌ Lỗi hệ thống khi gửi lại mã!");
                resendBtn.disabled = false;
            }
        });
    }

    // Nút xác minh mã
    const verifyBtn = document.getElementById("verifyBtn");
    if (verifyBtn) {
        verifyBtn.addEventListener("click", async () => {
            const code = Array.from(document.querySelectorAll(".code-input"))
                .map(i => i.value)
                .join("");

            if (code.length !== 6) {
                alert("⚠️ Vui lòng nhập đủ 6 chữ số!");
                return;
            }

            try {
                const res = await fetch('/Authentication/login-verify', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email, code })
                });

                if (res.ok) {
                    alert("🎉 Xác minh thành công! Đang chuyển hướng...");
                    const redirectTo = localStorage.getItem("preLoginUrl") || "/Home/Index";
                    localStorage.removeItem("preLoginUrl");
                    window.location.href = redirectTo;
                } else {
                    const err = await res.text();
                    alert("❌ Xác minh thất bại: " + err);
                }
            } catch (err) {
                console.error("Verify error:", err);
                alert("❌ Lỗi hệ thống khi xác minh!");
            }
        });
    }
});
