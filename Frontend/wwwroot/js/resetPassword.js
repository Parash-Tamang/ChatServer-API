$("#resetPasswordForm").on("submit", function (e) {
    e.preventDefault(); // stop form submit


    // Disable button while logging in
    let btn = $("#resetPasswordBtn");
    if (btn.prop("disabled")) {
        return;
    }
    const urlParams = new URLSearchParams(window.location.search);

    const email = urlParams.get("email");
    const token = urlParams.get("token");
    let password = $("#password").val().trim();
    let errors = []
  
    // Password rules
    if (password.length < 8)
        errors.push("Password must be at least 8 characters.");
    if (!/[A-Z]/.test(password))
        errors.push("Password must include at least 1 uppercase letter.");
    if (!/[a-z]/.test(password))
        errors.push("Password must include at least 1 lowercase letter.");
    if (!/[0-9]/.test(password))
        errors.push("Password must include at least 1 number.");
    if (!/[!@#$%^&]/.test(password))
        errors.push("Password must include a special character (!@#$%^&).");

    // Display validation errors
    if (errors.length > 0) {
        errors.forEach(err => toastr.warning(err));
        return;
    }

    console.log(password);
    btn.prop("disabled", true);
    $.ajax({
        url: `${window.env.API_BASE_URL}/Auth/reset-password`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            email: email,
            token:token,
            password: password
        }),

        success: function (res) {
            btn.prop("disabled", false).text("Reset Password");
            console.log(res);

            if (res.success) {
                toastr.success("Password Reset Successfully!");
                setTimeout(() => {
                    window.location.href = "/Account/Login";
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
