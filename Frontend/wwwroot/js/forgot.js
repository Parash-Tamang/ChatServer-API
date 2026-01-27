$("#forgotPasswordForm").on("submit", function (e) {
    e.preventDefault(); // stop form submit


    // Disable button while logging in
    let btn = $("#forgotPasswordBtn");
    if (btn.prop("disabled")) {
        return;
    }
    let email = $("#email").val().trim();
    let errors = []
    let emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email))
        errors.push("Enter a valid email address.");

    btn.prop("disabled", true);
    $.ajax({
        url: `${window.env.API_BASE_URL}/Auth/forgot-password`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            email: email,
        }),

        success: function (res) {
            btn.prop("disabled", false).text("Verified");
            console.log(res);

            if (res.success) {
                toastr.success(" Reset New password!");
                setTimeout(() => {
                    window.location.href = `/Account/ResetPassword?email=${email}&token=${res.data}`;
                }, 500);
            } else {
                toastr.error(res.message || "Verification failed!");
            }
        },
        error: function (xhr) {
            btn.prop("disabled", false).text("Verify");
            console.log(xhr.responseJSON);

            if (xhr.responseJSON && xhr.responseJSON.errors) {
                Object.values(xhr.responseJSON.errors)
                    .flat()
                    .forEach(err => {
                        toastr.error(err);
                    });
            }
            else if (xhr.responseJSON?.message) {
                console.log(xhr.responseJSON);
                toastr.error(xhr.responseJSON.message);
            }
            else {
                toastr.error("Server error. Please try again.");
            }

        }
    });
});
