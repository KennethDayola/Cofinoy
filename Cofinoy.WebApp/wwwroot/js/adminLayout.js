
document.addEventListener("DOMContentLoaded", () => {
    const logoutModal = document.getElementById("logoutConfirmModal");
    const cancelBtn = document.getElementById("cancelLogoutBtn");
    const confirmBtn = document.getElementById("confirmLogoutBtn");

    const logoutForm = document.getElementById("logoutForm");
    const openLogoutBtn = document.getElementById("openLogoutModalBtn");

    openLogoutBtn.addEventListener("click", () => {
        logoutModal.classList.add("active");
        document.body.style.overflow = "hidden";
    });

    const closeModal = () => {
        logoutModal.classList.remove("active");
        document.body.style.overflow = "auto";
    };

    cancelBtn.addEventListener("click", closeModal);

    confirmBtn.addEventListener("click", () => {
        logoutForm.submit();
    });
});
