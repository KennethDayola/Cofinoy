document.addEventListener("DOMContentLoaded", () => {
    const logoutModal = document.getElementById("logoutConfirmModal");
    const cancelBtn = document.getElementById("cancelLogoutBtn");
    const confirmBtn = document.getElementById("confirmLogoutBtn");
    const logoutForm = document.getElementById("logoutForm");
    const openLogoutBtn = document.getElementById("openLogoutModalBtn");

    // Prevent any other click events from triggering the modal
    if (openLogoutBtn) {
        openLogoutBtn.addEventListener("click", (e) => {
            e.preventDefault();
            e.stopPropagation();
            logoutModal.classList.add("active");
            document.body.style.overflow = "hidden";
        });
    }

    const closeModal = () => {
        logoutModal.classList.remove("active");
        document.body.style.overflow = "auto";
    };

    if (cancelBtn) {
        cancelBtn.addEventListener("click", (e) => {
            e.preventDefault();
            closeModal();
        });
    }

    if (confirmBtn) {
        confirmBtn.addEventListener("click", (e) => {
            e.preventDefault();
            logoutForm.submit();
        });
    }
});