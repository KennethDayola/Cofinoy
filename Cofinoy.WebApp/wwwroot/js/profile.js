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
});
