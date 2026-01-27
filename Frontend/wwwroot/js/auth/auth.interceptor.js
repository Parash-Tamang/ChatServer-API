// ajax.interceptor.js
// ================================
// Global AJAX Auth Interceptor
// ================================

/*$(function () {*/

    $.ajaxSetup({
        beforeSend: function (xhr) {
            const token = localStorage.getItem("accessToken");//AuthService.getAccessToken();
            if (token) {
                console.log("Authenticating Access Token");
                xhr.setRequestHeader("Authorization", "Bearer " + token);
            }
        }
    });

   $(document).ajaxError(function (event, xhr, settings) {

        // Only handle auth errors
        if (xhr.status !== 401) return;

        // Do NOT intercept refresh-token call itself
        if (settings.url.includes("/Auth/refresh-token")) return;

        // Prevent infinite retry
        if (settings._retry) return;
        settings._retry = true;

        AuthService.refreshToken()
            .then(newToken => {
                settings.headers = settings.headers || {};
                settings.headers["Authorization"] = "Bearer " + newToken;
                $.ajax(settings); // retry original request
            })
            .catch(() => {
                AuthService.clear();
                window.location.replace("/Account/Login");
            });
    });

//});
