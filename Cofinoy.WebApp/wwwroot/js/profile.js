$(document).ready(function () {
    // Store original values for resetting
    let originalPersonalInfo = {};
    let originalAddress = {};

    // --- Capture original values when modals open ---
    $('#editPersonalModal').on('show.bs.modal', function () {
        originalPersonalInfo = {
            FirstName: $('#personalForm input[name="FirstName"]').val(),
            LastName: $('#personalForm input[name="LastName"]').val(),
            Nickname: $('#personalForm input[name="Nickname"]').val(),
            BirthDate: $('#personalForm input[name="BirthDate"]').val(),
            Email: $('#personalForm input[name="Email"]').val(),
            PhoneNumber: $('#personalForm input[name="PhoneNumber"]').val()
        };
        $('#personalForm .text-danger').text('');
    });

    $('#editAddressModal').on('show.bs.modal', function () {
        originalAddress = {
            Country: $('#addressForm input[name="Country"]').val(),
            City: $('#addressForm input[name="City"]').val(),
            PostalCode: $('#addressForm input[name="postalCode"]').val()
        };
        $('#addressForm .text-danger').text('');
    });

    $('#changePasswordModal').on('show.bs.modal', function () {
        $('#changePasswordForm')[0].reset();
        $('#changePasswordForm .text-danger').text('');
    });

    // --- Reset forms when modals are closed without saving ---
    $('#editPersonalModal').on('hidden.bs.modal', function () {
        $('#personalForm input[name="FirstName"]').val(originalPersonalInfo.FirstName);
        $('#personalForm input[name="LastName"]').val(originalPersonalInfo.LastName);
        $('#personalForm input[name="Nickname"]').val(originalPersonalInfo.Nickname);
        $('#personalForm input[name="BirthDate"]').val(originalPersonalInfo.BirthDate);
        $('#personalForm input[name="Email"]').val(originalPersonalInfo.Email);
        $('#personalForm input[name="PhoneNumber"]').val(originalPersonalInfo.PhoneNumber);
        $('#personalForm .text-danger').text('');
    });

    $('#editAddressModal').on('hidden.bs.modal', function () {
        $('#addressForm input[name="Country"]').val(originalAddress.Country);
        $('#addressForm input[name="City"]').val(originalAddress.City);
        $('#addressForm input[name="postalCode"]').val(originalAddress.PostalCode);
        $('#addressForm .text-danger').text('');
    });

    $('#changePasswordModal').on('hidden.bs.modal', function () {
        $('#changePasswordForm')[0].reset();
        $('#changePasswordForm .text-danger').text('');
    });

    // --- Real-time validation for Personal Info fields ---
    $('#personalForm input[name="FirstName"]').on('input', function () {
        const value = $(this).val();
        const errorElement = $(this).next('.text-danger');

        if (value.length > 50) {
            errorElement.text('First name cannot exceed 50 characters.');
        } else {
            errorElement.text('');
        }
    });

    $('#personalForm input[name="LastName"]').on('input', function () {
        const value = $(this).val();
        const errorElement = $(this).next('.text-danger');

        if (value.length > 50) {
            errorElement.text('Last name cannot exceed 50 characters.');
        } else {
            errorElement.text('');
        }
    });

    $('#personalForm input[name="Nickname"]').on('input', function () {
        const value = $(this).val();
        const errorElement = $(this).next('.text-danger');

        if (!value) {
            errorElement.text('Nickname is required.');
        } else if (value.length > 50) {
            errorElement.text('Nickname cannot exceed 50 characters.');
        } else {
            errorElement.text('');
        }
    });

    $('#personalForm input[name="Email"]').on('input', function () {
        const value = $(this).val();
        const errorElement = $(this).next('.text-danger');

        if (!value) {
            errorElement.text('Email is required.');
        } else if (value.length > 100) {
            errorElement.text('Email cannot exceed 100 characters.');
        } else if (!isValidEmail(value)) {
            errorElement.text('Email must contain "@" followed by a domain with an extension (e.g., example@domain.com).');
        } else {
            errorElement.text('');
        }
    });

    $('#personalForm input[name="PhoneNumber"]').on('input', function () {
        const value = $(this).val();
        const errorElement = $(this).next('.text-danger');

        if (value && !/^\d+$/.test(value)) {
            errorElement.text('Phone number must contain only digits.');
        } else if (value.length > 20) {
            errorElement.text('Phone number cannot exceed 20 characters.');
        } else {
            errorElement.text('');
        }
    });

    // --- Real-time validation for Address fields ---
    $('#addressForm input[name="Country"]').on('input', function () {
        const value = $(this).val();
        const errorElement = $(this).next('.text-danger');

        if (value.length > 50) {
            errorElement.text('Country cannot exceed 50 characters.');
        } else {
            errorElement.text('');
        }
    });

    $('#addressForm input[name="City"]').on('input', function () {
        const value = $(this).val();
        const errorElement = $(this).next('.text-danger');

        if (value.length > 50) {
            errorElement.text('City cannot exceed 50 characters.');
        } else {
            errorElement.text('');
        }
    });

    $('#addressForm input[name="postalCode"]').on('input', function () {
        const value = $(this).val();
        const errorElement = $(this).next('.text-danger');

        if (value.length > 20) {
            errorElement.text('Postal code cannot exceed 20 characters.');
        } else {
            errorElement.text('');
        }
    });

    // --- Personal Info Form ---
    $('#personalForm').on('submit', function (e) {
        e.preventDefault();

        $('#personalForm .text-danger').text('');

        var model = {
            FirstName: $('#personalForm input[name="FirstName"]').val().trim(),
            LastName: $('#personalForm input[name="LastName"]').val().trim(),
            Nickname: $('#personalForm input[name="Nickname"]').val().trim(),
            BirthDate: $('#personalForm input[name="BirthDate"]').val() || null,
            PhoneNumber: $('#personalForm input[name="PhoneNumber"]').val().trim(),
            Email: $('#personalForm input[name="Email"]').val().trim()
        };

        // Client-side validation
        let hasError = false;

        // Nickname validation
        if (!model.Nickname) {
            $('#personalForm input[name="Nickname"]').next('.text-danger').text('Nickname is required.');
            hasError = true;
        } else if (model.Nickname.length > 50) {
            $('#personalForm input[name="Nickname"]').next('.text-danger').text('Nickname cannot exceed 50 characters.');
            hasError = true;
        }

        // FirstName validation
        if (model.FirstName && model.FirstName.length > 50) {
            $('#personalForm input[name="FirstName"]').next('.text-danger').text('First name cannot exceed 50 characters.');
            hasError = true;
        }

        // LastName validation
        if (model.LastName && model.LastName.length > 50) {
            $('#personalForm input[name="LastName"]').next('.text-danger').text('Last name cannot exceed 50 characters.');
            hasError = true;
        }

        // Email validation
        if (!model.Email) {
            $('#personalForm input[name="Email"]').next('.text-danger').text('Email is required.');
            hasError = true;
        } else if (model.Email.length > 100) {
            $('#personalForm input[name="Email"]').next('.text-danger').text('Email cannot exceed 100 characters.');
            hasError = true;
        } else if (!isValidEmail(model.Email)) {
            $('#personalForm input[name="Email"]').next('.text-danger').text('Email must contain "@" followed by a domain with an extension (e.g., example@domain.com).');
            hasError = true;
        }

        // PhoneNumber validation
        if (model.PhoneNumber) {
            if (!/^\d+$/.test(model.PhoneNumber)) {
                $('#personalForm input[name="PhoneNumber"]').next('.text-danger').text('Phone number must contain only digits.');
                hasError = true;
            } else if (model.PhoneNumber.length > 20) {
                $('#personalForm input[name="PhoneNumber"]').next('.text-danger').text('Phone number cannot exceed 20 characters.');
                hasError = true;
            }
        }

        if (hasError) {
            return;
        }

        $.ajax({
            url: '/Account/UpdatePersonalInfo',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function (res) {
                if (res.success) {
                    showToast(res.message, 'success');
                    $('#editPersonalModal').modal('hide');
                    setTimeout(() => location.reload(), 1000);
                } else if (res.errors) {
                    for (const key in res.errors) {
                        const input = $(`#personalForm input[name="${key}"]`);
                        if (input.length) {
                            input.next('.text-danger').text(res.errors[key][0]);
                        } else if (key === "General") {
                            showToast(res.errors[key][0], 'error');
                        }
                    }
                } else {
                    showToast(res.message || 'Error updating personal info.', 'error');
                }
            },
            error: function () {
                showToast('Error updating personal info.', 'error');
            }
        });
    });

    // --- Address Form ---
    $('#addressForm').on('submit', function (e) {
        e.preventDefault();

        $('#addressForm .text-danger').text('');

        var model = {
            Country: $('#addressForm input[name="Country"]').val().trim(),
            City: $('#addressForm input[name="City"]').val().trim(),
            PostalCode: $('#addressForm input[name="postalCode"]').val().trim()
        };

        // Client-side validation for length
        let hasError = false;

        if (model.Country && model.Country.length > 50) {
            $('#addressForm input[name="Country"]').next('.text-danger').text('Country cannot exceed 50 characters.');
            hasError = true;
        }

        if (model.City && model.City.length > 50) {
            $('#addressForm input[name="City"]').next('.text-danger').text('City cannot exceed 50 characters.');
            hasError = true;
        }

        if (model.PostalCode && model.PostalCode.length > 20) {
            $('#addressForm input[name="postalCode"]').next('.text-danger').text('Postal code cannot exceed 20 characters.');
            hasError = true;
        }

        if (hasError) {
            return;
        }

        $.ajax({
            url: '/Account/UpdateAddress',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function (res) {
                if (res.success) {
                    showToast(res.message, 'success');
                    $('#editAddressModal').modal('hide');
                    setTimeout(() => location.reload(), 1000);
                } else if (res.errors) {
                    for (const key in res.errors) {
                        const input = $(`#addressForm input[name="${key}"]`);
                        if (input.length) {
                            input.next('.text-danger').text(res.errors[key][0]);
                        } else if (key === "General") {
                            showToast(res.errors[key][0], 'error');
                        }
                    }
                } else {
                    showToast(res.message || 'Error updating address.', 'error');
                }
            },
            error: function () {
                showToast('Error updating address.', 'error');
            }
        });
    });

    // --- Change Password Form ---
    $('#changePasswordForm').on('submit', function (e) {
        e.preventDefault();

        const model = {
            CurrentPassword: $('#currentPassword').val(),
            NewPassword: $('#newPassword').val(),
            ConfirmPassword: $('#confirmPassword').val()
        };

        $('#changePasswordForm .text-danger').text('');

        $.ajax({
            url: '/Account/ChangePassword',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function (data) {
                if (data.success) {
                    $('#changePasswordForm')[0].reset();
                    $('#changePasswordModal').modal('hide');
                    showToast(data.message, 'success');
                } else {
                    if (data.errors) {
                        if (data.errors.CurrentPassword) {
                            $('#currentPassword').next('.text-danger').text(data.errors.CurrentPassword[0]);
                        }
                        if (data.errors.NewPassword) {
                            $('#newPassword').next('.text-danger').text(data.errors.NewPassword[0]);
                        }
                        if (data.errors.ConfirmPassword) {
                            $('#confirmPassword').next('.text-danger').text(data.errors.ConfirmPassword[0]);
                        }
                        if (data.errors.General) {
                            showToast(data.errors.General[0], 'error');
                        }
                    }
                }
            },
            error: function () {
                showToast('Something went wrong while changing password.', 'error');
            }
        });
    });

    // --- Real-time validation for password ---
    $('#newPassword').on('input', function () {
        const currentPassword = $('#currentPassword').val();
        const newPassword = $(this).val();
        const errorElement = $(this).next('.text-danger');

        if (newPassword === currentPassword && currentPassword.length > 0) {
            errorElement.text('New password cannot be the same as current password.');
        } else if (newPassword.length > 0 && newPassword.length < 8) {
            errorElement.text('Password must be at least 8 characters.');
        } else {
            errorElement.text('');
        }
    });

    $('#confirmPassword').on('input', function () {
        const newPassword = $('#newPassword').val();
        const confirmPassword = $(this).val();
        const errorElement = $(this).next('.text-danger');

        if (confirmPassword.length > 0 && newPassword !== confirmPassword) {
            errorElement.text('Passwords do not match.');
        } else {
            errorElement.text('');
        }
    });

    // --- Logout Modal ---
    $('#openLogoutModalBtn').on('click', function () {
        $('#logoutConfirmModal').fadeIn(300);
    });

    $('#cancelLogoutBtn').on('click', function () {
        $('#logoutConfirmModal').fadeOut(300);
    });

    // Close logout modal when clicking outside
    $('#logoutConfirmModal').on('click', function (e) {
        if ($(e.target).is('#logoutConfirmModal')) {
            $(this).fadeOut(300);
        }
    });
});

// Helper function to validate email with proper format
function isValidEmail(email) {
    // Email must have @ followed by domain and extension (e.g., user@domain.com)
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function showToast(message, type) {
    // Remove any existing toasts first
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