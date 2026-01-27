$("#registerForm").on("submit", function (e) {
    e.preventDefault();

    let btn = $("#registerBtn");
    if (btn.prop("disabled")) {

        return;
    }  // ✅ Prevent double submit false

    // Read inputs
    let firstName = $("#firstName").val().trim();
    let lastName = $("#lastName").val().trim();
    let email = $("#email").val().trim();
    let phone = $("#phone").val().trim();
    let password = $("#password").val().trim();
    let confirmPassword = $("#confirmPassword").val().trim();

    let errors = [];

    // Name validation
    if (firstName.length < 3)
        errors.push("First name must be at least 3 characters.");
    if (lastName.length < 3)
        errors.push("Last name must be at least 3 characters.");

    // Email validation
    let emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email))
        errors.push("Enter a valid email address.");

    // Phone validation (optional)
    let phoneRegex = /^[0-9]{10}$/;
    if (phone && !phoneRegex.test(phone))
        errors.push("Phone number must be 10 digits.");

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

    if (password !== confirmPassword)
        errors.push("Passwords do not match.");

    // Display validation errors
    if (errors.length > 0) {
        errors.forEach(err => toastr.warning(err));
        return;
    }

    // Button loading state
    btn.prop("disabled", true);

    // AJAX request
    $.ajax({
        url: `${window.env.API_BASE_URL}/Auth/register`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            firstName: firstName,
            lastName: lastName,
            email: email,
            phone: phone,
            password: password,
            confirmPassword: confirmPassword
        }),

        success: function (res) {
            btn.prop("disabled", false).text("Register Now");
            console.log(res);

            if (res.success) {
                toastr.success("Successfully Registered!");
                setTimeout(() => {
                    window.location.href = "/Account/Login";
                }, 500);
            } else {
                toastr.error(res.message || "Registration failed!");
            }
        },
        error: function (xhr) {
            console.log(xhr.responseJSON);
            btn.prop("disabled", false).text("Register Now");

          
                if (xhr.responseJSON && xhr.responseJSON.errors) {
                    Object.values(xhr.responseJSON.errors)
                        .flat()
                        .forEach(err => {
                            toastr.error(err);
                        });

                }
                else if (xhr.responseJSON?.message) {
                    toastr.error(xhr.responseJSON.message);
                    return;
                }
               else {
                toastr.error("Server error. Please try again.");
            }
        }
    });
});