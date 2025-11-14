$(document).ready(function () {
    // --- Personal Info Form ---
    $('#personalForm').on('submit', function (e) {
        e.preventDefault();
        var model = {
            FirstName: $('#personalForm input[name="FirstName"]').val(),
            LastName: $('#personalForm input[name="LastName"]').val(),
            Nickname: $('#personalForm input[name="Nickname"]').val(),
            BirthDate: $('#personalForm input[name="BirthDate"]').val(),
            PhoneNumber: $('#personalForm input[name="PhoneNumber"]').val(),
            Email: $('#personalForm input[name="Email"]').val()
        };
        $.ajax({
            url: '/Account/UpdatePersonalInfo',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function (res) {
                if (res.success) location.reload();
                else alert(res.message);
            },
            error: function () {
                alert('Error updating personal info.');
            }
        });
    });

    // --- Address Form ---
    $('#addressForm').on('submit', function (e) {
        e.preventDefault();
        var model = {
            Country: $('#addressForm input[name="Country"]').val(),
            City: $('#addressForm input[name="City"]').val(),
            postalCode: $('#addressForm input[name="postalCode"]').val()
        };
        $.ajax({
            url: '/Account/UpdateAddress',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function (res) {
                if (res.success) location.reload();
                else alert(res.message);
            },
            error: function () {
                alert('Error updating address.');
            }
        });
    });

   
    $('#changePasswordForm').on('submit', function (e) {
        e.preventDefault();

        const currentPassword = $('#currentPassword').val();
        const newPassword = $('#newPassword').val();
        const confirmPassword = $('#confirmPassword').val();

    
        const currentPasswordError = $('#currentPasswordError');
        const newPasswordError = $('#newPasswordError');
        const confirmPasswordError = $('#confirmPasswordError');
        const passwordLengthError = $('#passwordLengthError');

       
        hideError(currentPasswordError);
        hideError(newPasswordError);
        hideError(confirmPasswordError);
        hideError(passwordLengthError);

        let hasError = false;

       
        if (newPassword.length < 6) {
            showError(passwordLengthError);
            hasError = true;
        }

       
        if (newPassword === currentPassword && newPassword.length > 0) {
            showError(newPasswordError);
            hasError = true;
        }

       
        if (newPassword !== confirmPassword) {
            showError(confirmPasswordError);
            hasError = true;
        }

        
        if (!hasError) {
            $.ajax({
                url: '/Account/ChangePassword',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    CurrentPassword: currentPassword,
                    NewPassword: newPassword,
                    ConfirmPassword: confirmPassword
                }),
                success: function (data) {
                    if (data.success) {
                        showToast(data.message, 'success');
                        $('#changePasswordForm')[0].reset();
                        $('#changePasswordModal').modal('hide');
                    } else {
                        
                        showToast(data.message, 'error');
                    }
                },
                error: function () {
                    showToast('Something went wrong while changing password.', 'error');
                }
            });
        }
    });

  
    $('#newPassword').on('input', function () {
        const currentPassword = $('#currentPassword').val();
        const newPassword = $(this).val();
        const newPasswordError = $('#newPasswordError');
        const passwordLengthError = $('#passwordLengthError');

        
        if (newPassword === currentPassword && currentPassword.length > 0) {
            showError(newPasswordError);
        } else {
            hideError(newPasswordError);
        }

       
        if (newPassword.length > 0 && newPassword.length < 6) {
            showError(passwordLengthError);
        } else {
            hideError(passwordLengthError);
        }
    });

    
    $('#confirmPassword').on('input', function () {
        const newPassword = $('#newPassword').val();
        const confirmPassword = $(this).val();
        const confirmPasswordError = $('#confirmPasswordError');

        if (confirmPassword.length > 0 && newPassword !== confirmPassword) {
            showError(confirmPasswordError);
        } else {
            hideError(confirmPasswordError);
        }
    });
});


function showError(element) {
    if (element.length) {
        element.removeClass('d-none');
    }
}

function hideError(element) {
    if (element.length) {
        element.addClass('d-none');
    }
}


function showToast(message, type) {
   
    $('.profile-toast').remove();

   
    const icon = type === 'success' ? '✓' : '✕';

    const toast = $('<div class="profile-toast"></div>')
        .addClass(type)
        .html(`
            <span class="profile-toast-icon">${icon}</span>
            <span class="profile-toast-message">${message}</span>
        `);

   
    $('body').append(toast);

    
    setTimeout(function () {
        toast.addClass('slide-out');
        setTimeout(function () {
            toast.remove();
        }, 300);
    }, 3000);
}

document.addEventListener("DOMContentLoaded", () => {
    const logoutModal = document.getElementById("logoutConfirmModal");
    const cancelBtn = document.getElementById("cancelLogoutBtn");
    const confirmBtn = document.getElementById("confirmLogoutBtn");
    const closeBtn = document.getElementById("closeLogoutModal");

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
    closeBtn.addEventListener("click", closeModal);

    confirmBtn.addEventListener("click", () => {
        logoutForm.submit();
    });
});
