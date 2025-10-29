let countdownInterval;
let remainingTime = 600; // 10 phút = 600 giây
let email = "";

// --- Lưu trang trước khi vào trang Register ---
if (!localStorage.getItem("preRegisterUrl")) {
    localStorage.setItem("preRegisterUrl", document.referrer || window.location.origin);
}

function startCountdown() {
    clearInterval(countdownInterval);
    countdownInterval = setInterval(() => {
        remainingTime--;
        const minutes = Math.floor(remainingTime / 60);
        const seconds = remainingTime % 60;
        document.getElementById("countdown").textContent =
            `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

        if (remainingTime <= 0) {
            clearInterval(countdownInterval);
            document.getElementById("resendBtn").disabled = false;
            document.getElementById("countdownText").textContent = "Mã đã hết hạn!";
        }
    }, 1000);
}

// Khi user nhập xong 1 ô thì tự động nhảy sang ô tiếp theo
document.querySelectorAll(".code-input").forEach((input, index, arr) => {
    input.addEventListener("input", (e) => {
        if (e.target.value && index < arr.length - 1) {
            arr[index + 1].focus();
        }
    });
});

// Sau khi đăng ký thành công, hiện modal xác minh
document.addEventListener("DOMContentLoaded", () => {
    if (typeof registerEmail !== "undefined" && registerEmail) {
        email = registerEmail;
        const verifyEmailElem = document.getElementById("verifyEmailAddress");
        if (verifyEmailElem) {
            verifyEmailElem.textContent = email;
            const modalEl = document.getElementById('verifyEmailModal');
            const modal = new bootstrap.Modal(modalEl);
            modal.show();
            startCountdown();
        }
    }
});

// Gửi lại mã
document.getElementById("resendBtn").addEventListener("click", async () => {
    document.getElementById("resendBtn").disabled = true;
    try {
        const res = await fetch('/Authentication/register-resend-code', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email })
        });

        if (res.ok) {
            remainingTime = 600;
            startCountdown();
            showAlert("Mã xác minh mới đã được gửi!", "success");
            //alert("Mã xác minh mới đã được gửi!");
        } else {
            showAlert("Gửi lại mã thất bại!", "error");
            //alert("Gửi lại mã thất bại!");
            document.getElementById("resendBtn").disabled = false;
        }
    } catch (err) {
        console.error(err);
        showAlert("Lỗi hệ thống khi gửi lại mã!", "error");
        //alert("Lỗi hệ thống khi gửi lại mã!");
    }
});

// Xác thực mã
document.getElementById("verifyBtn").addEventListener("click", async () => {
    const code = Array.from(document.querySelectorAll(".code-input"))
        .map(i => i.value)
        .join("");

    if (code.length !== 6) {
        showAlert("Vui lòng nhập đủ 6 chữ số!", "error");
        //alert("Vui lòng nhập đủ 6 chữ số!");
        return;
    }

    try {
        const res = await fetch('/Authentication/regisster-verify', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, code })
        });

        if (res.ok) {
            showAlert("Xác minh thành công! Đang chuyển hướng...", "success");
            //alert("Xác minh thành công! Đang chuyển hướng...");

            const redirectTo =
                localStorage.getItem("preRegisterUrl") ||
                (typeof returnUrl !== "undefined" && returnUrl) ||
                "/Home/Index";

            localStorage.removeItem("preRegisterUrl");
            window.location.href = redirectTo;
        } else {
            const err = await res.text();
            showAlert("Xác minh thất bại: " + err, "error");
            //alert("Xác minh thất bại: " + err);
        }
    } catch (err) {
        console.error(err);
        showAlert("Lỗi hệ thống khi xác minh!", "error");
        //alert("Lỗi hệ thống khi xác minh!");
    }
});
