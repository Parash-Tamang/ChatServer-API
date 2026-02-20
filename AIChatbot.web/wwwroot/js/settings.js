async function loadUserDetails() {
    try {
        let res = await fetch("/Auth/UserDetails", {
            credentials: "include"
        });

        if (!res.ok) throw "unauthorized";

        let data = await res.json();

        document.getElementById("userInfo").innerHTML = `
            <div><b>Name:</b> ${data.firstName} ${data.lastName}</div>
            <div><b>Email:</b> ${data.email}</div>
            <div><b>Phone:</b> ${data.phone}</div>
        `;

    } catch {
        document.getElementById("userInfo").innerHTML = "Failed to load user";
    }
}

async function logout() {
    await fetch("/Auth/Logout", { method: "POST" });
    window.location.href = "/Auth/Login";
}

loadUserDetails();
