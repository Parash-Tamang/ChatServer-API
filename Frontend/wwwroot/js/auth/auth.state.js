// auth.state.js
// ================================
// Global Auth State (In-Memory)
// ================================

(function (window) {

    window.AuthState = {
        accessToken: null,
        isLoggedIn: false,
        isRefreshing: false,
        refreshQueue: []
    };

})(window);
