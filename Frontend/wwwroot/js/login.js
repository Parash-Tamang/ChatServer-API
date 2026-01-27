$("#loginForm").on("submit", function (e)
{
    e.preventDefault(); // stop form submit

    // Disable button while logging in
    let btn = $("#loginBtn");
    if (btn.prop("disabled"))
    {
        return;
    }

    let email = $("#email").val().trim();
    let password = $("#password").val().trim();

    let errors = []
    let emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailRegex.test(email))
    {
        errors.push("Enter a valid email address.");
    }

    if (!password)
    {
         errors.push("Password is required");
    }

     btn.prop("disabled", true);
        $.ajax({
            url: `${window.env.API_BASE_URL}/Auth/login`,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                email: email,
                password: password,
            }),
            contentType: "application/json",
            xhrFields: {
                withCredentials: true // 🔑 REQUIRED
            },

            success: function (res) {
                btn.prop("disabled", false).text("Logged In");
                console.log(res);

                if (res.success) {

                    localStorage.setItem("accessToken", res.data.accessToken);
                    localStorage.setItem("isLoggedIn", "true");

                    toastr.success("Logged Successfully!");
                    setTimeout(() => {
                        window.location.href = "/Chat/Index";
                    }, 500);
                } else {
                    toastr.error(res.message || "Login failed!");
                }
            },
            error: function (xhr) {
                btn.prop("disabled", false).text("Get Started");
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
