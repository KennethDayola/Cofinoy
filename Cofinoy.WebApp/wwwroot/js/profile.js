$(document).ready(function () {

    $('#personalForm').on('submit', function (e) {
        e.preventDefault();

        $('#personalForm .text-danger').text('');

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
                    showToast(res.message, 'error');
                }
            },
            error: function () {
                showToast('Error updating personal info.', 'error');
            }
        });
    });

    $('#addressForm').on('submit', function (e) {
        e.preventDefault();

        $('#addressForm .text-danger').text('');

        var model = {
            Country: $('#addressForm input[name="Country"]').val(),
            City: $('#addressForm input[name="City"]').val(),
            PostalCode: $('#addressForm input[name="postalCode"]').val()
        };

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
                    showToast(res.message, 'error');
                }
            },
            error: function () {
                showToast('Error updating address.', 'error');
            }
        });
    });

    $('#changePasswordForm').on('submit', function (e) {
        e.preventDefault();

        const model = {
            CurrentPassword: $('#currentPassword').val(),
            NewPassword: $('#newPassword').val(),
            ConfirmPassword: $('#confirmPassword').val()
        };

        $('.text-danger').text('');

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

    $('#editPersonalModal, #editAddressModal, #changePasswordModal').on('hidden.bs.modal', function () {
        const form = $(this).find('form')[0];
        if (form) form.reset();
        $(this).find('.text-danger').text('');
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
