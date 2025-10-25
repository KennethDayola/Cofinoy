$(document).ready(function () {
    // Toast notification helper
    function showToast(message, type = 'info') {
        // Bootstrap 5 toast (assuming you have toast container in layout)
        const toastHtml = `
            <div class="toast align-items-center text-white bg-${type === 'success' ? 'success' : type === 'danger' ? 'danger' : 'info'} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        // If you don't have a toast container, use alert as fallback
        if ($('#toastContainer').length === 0) {
            alert(message);
        } else {
            $('#toastContainer').append(toastHtml);
            const toastElement = $('#toastContainer .toast').last()[0];
            const toast = new bootstrap.Toast(toastElement, { delay: 3000 });
            toast.show();

            // Remove toast element after it's hidden
            $(toastElement).on('hidden.bs.toast', function () {
                $(this).remove();
            });
        }
    }

    // Validate email format
    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    // --- Personal Info Form ---
    $('#personalForm').on('submit', function (e) {
        e.preventDefault();

        // Get form values
        const firstName = $('#personalForm input[name="FirstName"]').val().trim();
        const lastName = $('#personalForm input[name="LastName"]').val().trim();
        const nickname = $('#personalForm input[name="Nickname"]').val().trim();
        const birthDate = $('#personalForm input[name="BirthDate"]').val();
        const phoneNumber = $('#personalForm input[name="PhoneNumber"]').val().trim();
        const email = $('#personalForm input[name="Email"]').val().trim();

        // Validation
        if (!firstName || !lastName || !nickname || !email) {
            showToast('Please fill in all required fields (First Name, Last Name, Nickname, Email).', 'danger');
            return;
        }

        if (!isValidEmail(email)) {
            showToast('Please enter a valid email address.', 'danger');
            return;
        }

        // Prepare data
        const model = {
            FirstName: firstName,
            LastName: lastName,
            Nickname: nickname,
            BirthDate: birthDate || null,
            PhoneNumber: phoneNumber || null,
            Email: email
        };

        // Disable submit button to prevent double submission
        const submitBtn = $(this).find('button[type="submit"]');
        submitBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Saving...');

        $.ajax({
            url: '/Account/UpdatePersonalInfo',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function (res) {
                submitBtn.prop('disabled', false).html('Save Changes');

                if (res.success) {
                    showToast(res.message || 'Personal information updated successfully!', 'success');

                    // Close modal
                    $('#editPersonalModal').modal('hide');

                    // Reload page after short delay to show updated info
                    setTimeout(function () {
                        location.reload();
                    }, 1000);
                } else {
                    showToast(res.message || 'Failed to update personal information.', 'danger');
                }
            },
            error: function (xhr, status, error) {
                submitBtn.prop('disabled', false).html('Save Changes');
                console.error('Error updating personal info:', error);
                showToast('An error occurred while updating personal information. Please try again.', 'danger');
            }
        });
    });

    // --- Address Form ---
    $('#addressForm').on('submit', function (e) {
        e.preventDefault();

        // Get form values
        const country = $('#addressForm input[name="Country"]').val().trim();
        const city = $('#addressForm input[name="City"]').val().trim();
        const postalCode = $('#addressForm input[name="postalCode"]').val().trim();

        // At least one field should be filled
        if (!country && !city && !postalCode) {
            showToast('Please fill in at least one address field.', 'danger');
            return;
        }

        // Prepare data
        const model = {
            Country: country || null,
            City: city || null,
            postalCode: postalCode || null
        };

        // Disable submit button
        const submitBtn = $(this).find('button[type="submit"]');
        submitBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Saving...');

        $.ajax({
            url: '/Account/UpdateAddress',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function (res) {
                submitBtn.prop('disabled', false).html('Save Changes');

                if (res.success) {
                    showToast(res.message || 'Address updated successfully!', 'success');

                    // Close modal
                    $('#editAddressModal').modal('hide');

                    // Reload page after short delay
                    setTimeout(function () {
                        location.reload();
                    }, 1000);
                } else {
                    showToast(res.message || 'Failed to update address.', 'danger');
                }
            },
            error: function (xhr, status, error) {
                submitBtn.prop('disabled', false).html('Save Changes');
                console.error('Error updating address:', error);
                showToast('An error occurred while updating address. Please try again.', 'danger');
            }
        });
    });

    // --- Change Password Form ---
    $('#changePasswordForm').on('submit', function (e) {
        e.preventDefault();

        // Get form values
        const currentPassword = $('#changePasswordForm input[name="CurrentPassword"]').val();
        const newPassword = $('#changePasswordForm input[name="NewPassword"]').val();
        const confirmPassword = $('#changePasswordForm input[name="ConfirmPassword"]').val();

        // Validation
        if (!currentPassword || !newPassword || !confirmPassword) {
            showToast('Please fill in all password fields.', 'danger');
            return;
        }

        if (newPassword.length < 6) {
            showToast('New password must be at least 6 characters long.', 'danger');
            return;
        }

        if (newPassword !== confirmPassword) {
            showToast('New passwords do not match.', 'danger');
            return;
        }

        if (currentPassword === newPassword) {
            showToast('New password must be different from current password.', 'danger');
            return;
        }

        // Prepare data
        const model = {
            CurrentPassword: currentPassword,
            NewPassword: newPassword,
            ConfirmPassword: confirmPassword
        };

        // Disable submit button
        const submitBtn = $(this).find('button[type="submit"]');
        submitBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Updating...');

        $.ajax({
            url: '/Account/ChangePassword',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function (res) {
                submitBtn.prop('disabled', false).html('Update Password');

                if (res.success) {
                    showToast(res.message || 'Password changed successfully!', 'success');

                    // Clear form
                    $('#changePasswordForm')[0].reset();

                    // Close modal
                    $('#changePasswordModal').modal('hide');
                } else {
                    showToast(res.message || 'Failed to change password.', 'danger');
                }
            },
            error: function (xhr, status, error) {
                submitBtn.prop('disabled', false).html('Update Password');
                console.error('Error changing password:', error);
                showToast('An error occurred while changing password. Please try again.', 'danger');
            }
        });
    });

    // Clear form errors when modal is closed
    $('.modal').on('hidden.bs.modal', function () {
        $(this).find('form')[0].reset();
        $(this).find('.is-invalid').removeClass('is-invalid');
        $(this).find('.invalid-feedback').remove();
    });
});