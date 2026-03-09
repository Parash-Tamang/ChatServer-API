/* ============================================
   LOGIN
============================================ */

async function loginUser() {

    // clear previous errors
    $("#loginEmailErr").text("");
    $("#loginPassErr").text("");
    $("#loginError").hide();

    const email = $("#email").val().trim();
    const password = $("#password").val().trim();

    let valid = true;

    if (!email) {
        $("#loginEmailErr").text("Enter email");
        valid = false;
    }

    if (!password) {
        $("#loginPassErr").text("Enter password");
        valid = false;
    }

//    if (!valid) return;

//    try {

//        let res = await fetch("/Auth/Login", {
//            method: "POST",
//            headers: { "Content-Type": "application/json" },
//            body: JSON.stringify({ email, password })
//        });

//        if (!res.ok) {
//            $("#loginError").text("Invalid email or password").show();
//            return;
//        }

//        window.location.href = "/Chat/Index";

//    } catch {
//        $("#loginError").text("Server error. Try again.").show();
//    }
//}



/* ============================================
   REGISTER
============================================ */

async function registerUser() {

    $(".error").text("");
    $("#regError").hide();

    let firstName = $("#fname").val().trim();
    let lastName = $("#lname").val().trim();
    let email = $("#regEmail").val().trim();
    let phone = $("#phone").val().trim();
    let password = $("#regPass").val();
    let confirmPassword = $("#confirmPass").val();

    let valid = true;

    // first name
    if (!firstName) {
        $("#fnameErr").text("Enter first name");
        valid = false;
    }

    // last name
    if (!lastName) {
        $("#lnameErr").text("Enter last name");
        valid = false;
    }

    // email
    let emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!email) {
        $("#emailErr").text("Enter email");
        valid = false;
    } else if (!emailRegex.test(email)) {
        $("#emailErr").text("Invalid email format");
        valid = false;
    }

    // phone (user types digits only)
    let phoneRegex = /^\d{10}$/;

    if (!phone) {
        $("#phoneErr").text("Enter phone number");
        valid = false;
    } else if (!phoneRegex.test(phone)) {
        $("#phoneErr").text("Enter valid 10 digit number");
        valid = false;
    }


    // password
    let passRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$/;

    if (!password) {
        $("#passErr").text("Enter password");
        valid = false;
    } else if (!passRegex.test(password)) {
        $("#passErr").text("8+ chars, upper, lower, number & special");
        valid = false;
    }

    // confirm
    if (!confirmPassword) {
        $("#confirmErr").text("Confirm your password");
        valid = false;
    } else if (password !== confirmPassword) {
        $("#confirmErr").text("Passwords do not match");
        valid = false;
    }

    if (!valid) return;

    // prefix +91
    let fullPhone = "+91" + phone;

    try {

        let res = await fetch("/Auth/Register", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                firstName,
                lastName,
                email,
                phone: fullPhone,
                password
            })
        });

        if (!res.ok) {
            $("#regError").text("Registration failed").show();
            return;
        }

        window.location.href = "/Chat";

    } catch {
        $("#regError").text("Server error").show();
    }
}



/* ============================================
   LIVE VALIDATION
============================================ */

document.addEventListener("DOMContentLoaded", function () {

    // EMAIL live
    $("#regEmail").on("input", function () {

        let val = $(this).val().trim();
        let regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (!val)
            $("#emailErr").text("Enter email");
        else if (!regex.test(val))
            $("#emailErr").text("Invalid email format");
        else
            $("#emailErr").text("");
    });

    // PHONE digits only
    $("#phone").on("input", function () {

        let digits = $(this).val().replace(/\D/g, "");
        $(this).val(digits);

        if (!digits)
            $("#phoneErr").text("Enter phone number");
        else if (digits.length !== 10)
            $("#phoneErr").text("Enter 10 digit number");
        else
            $("#phoneErr").text("");
    });

    // PASSWORD strength
    $("#regPass").on("input", function () {

        let pass = $(this).val();
        let regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$/;

        if (!pass)
            $("#passErr").text("Enter password");
        else if (!regex.test(pass))
            $("#passErr").text("8+ chars, upper, lower, number & special");
        else
            $("#passErr").text("");
    });

    // CONFIRM password
    $("#confirmPass").on("input", function () {

        if ($(this).val() !== $("#regPass").val())
            $("#confirmErr").text("Passwords do not match");
        else
            $("#confirmErr").text("");
    });

    // ENTER key handling
    $(document).on("keypress", function (e) {
        if (e.which === 13) {
            if ($("#email").length) loginUser();
            if ($("#fname").length) registerUser();
        }
    });

    // allow only digits for phone
    $("#phone").on("input", function () {
        this.value = this.value.replace(/\D/g, "").slice(0, 10);
    });

});
