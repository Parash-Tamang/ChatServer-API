// auth.service.js
// ================================
// Auth Logic Layer
// ================================

(function (window, $) {

    const AuthService = {

        setAccessToken(token) {
            AuthState.accessToken = token;
            console.log(AuthState.accessToken);
        },

        getAccessToken() {
            return AuthState.accessToken;
        },

        setLoggedIn(value) {
            
            AuthState.isLoggedIn = value;
            console.log("User Logged In");
        },

        clear() {
            AuthState.accessToken = null;
            AuthState.isRefreshing = false;
            AuthState.refreshQueue = [];
        },
        refreshToken() {
            return new Promise((resolvePromise, rejectPromise) => {

                if (AuthState.isRefreshing) {
                    AuthState.refreshQueue.push({
                        resolve: resolvePromise,
                        reject: rejectPromise
                    });
                    return;
                }

                AuthState.isRefreshing = true;
                console.log("Refreshing New Access Token");

                $.ajax({
                    url: `${window.env.API_BASE_URL}/Auth/refresh-token`,
                    type: "POST",
                    xhrFields: { withCredentials: true },

                    success: (res) => {
                        if (!res?.data?.accessToken) {
                            this._rejectAll();
                            rejectPromise("No access token");
                            return;
                        }
                       localStorage.setItem("accessToken",res.data.accessToken);
                       this.setAccessToken(res.data.accessToken);
                       this._resolveAll(res.data.accessToken);
                        resolvePromise(res.data.accessToken);
                    },

                    error: () => {
                        this._rejectAll();
                        rejectPromise(new Error("Refresh token invalid"));
                    },

                    complete: () => {
                        AuthState.isRefreshing = false;
                    }
                });
            });
        },

        restoreAccessToken() {
            const token = localStorage.getItem("accessToken");
            if (token) {
                AuthState.accessToken = token;
                return true;
            }
            return false;
        },
        _resolveAll(token) {
            AuthState.refreshQueue.forEach(p => p.resolve(token));
            AuthState.refreshQueue = [];
        },

        _rejectAll() {
            AuthState.refreshQueue.forEach(p => p.reject());
            AuthState.refreshQueue = [];
        }
    };

    window.AuthService = AuthService;

})(window, jQuery);
