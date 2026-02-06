// ajax.interceptor.js
// ================================
// Global AJAX Auth Interceptor with Refresh Token Handling
// ================================

(function () {
    let isRefreshing = false;
    let failedQueue = [];

    const processQueue = (error, token = null) => {
        failedQueue.forEach(prom => {
            if (error) {
                prom.reject(error);
            } else {
                prom.resolve(token);
            }
        });
        failedQueue = [];
    };

    // ✅ Add Authorization header to all requests
    $.ajaxSetup({
        beforeSend: function (xhr, settings) {
            // Skip auth header for refresh token endpoint
            if (settings.url.includes("/Auth/refresh-token")) {
                return;
            }

            const token = localStorage.getItem("accessToken");
            if (token) {
                xhr.setRequestHeader("Authorization", "Bearer " + token);
            }
        }
    });

    // ✅ Handle 401 errors with token refresh
    $(document).ajaxError(function (event, xhr, settings) {
        // Only handle 401 Unauthorized
        if (xhr.status !== 401) return;

        // Don't intercept refresh token endpoint itself
        if (settings.url.includes("/Auth/refresh-token")) {
            AuthService.clear();
            window.location.replace("/Account/Login");
            return;
        }

        // Don't retry already retried requests
        if (settings._isRetry) {
            AuthService.clear();
            window.location.replace("/Account/Login");
            return;
        }

        const originalRequest = settings;

        // ✅ If already refreshing, queue this request
        if (isRefreshing) {
            return new Promise((resolve, reject) => {
                failedQueue.push({ resolve, reject });
            })
                .then(token => {
                    originalRequest.headers = originalRequest.headers || {};
                    originalRequest.headers["Authorization"] = "Bearer " + token;
                    originalRequest._isRetry = true;
                    return $.ajax(originalRequest);
                })
                .catch(err => {
                    return Promise.reject(err);
                });
        }

        // ✅ Start refresh process
        isRefreshing = true;

        AuthService.refreshToken()
            .then(newToken => {
                // Update the failed request with new token
                originalRequest.headers = originalRequest.headers || {};
                originalRequest.headers["Authorization"] = "Bearer " + newToken;
                originalRequest._isRetry = true;

                // Process all queued requests
                processQueue(null, newToken);

                // Retry the original request
                return $.ajax(originalRequest);
            })
            .catch(error => {
                processQueue(error, null);
                AuthService.clear();
                window.location.replace("/Account/Login");
            })
            .finally(() => {
                isRefreshing = false;
            });
    });

})();