// auth.service.js
const AuthService = {

    getAccessToken() {
        return localStorage.getItem("accessToken");
    },

    setAccessToken(token) {
        localStorage.setItem("accessToken", token);
    },

    async refreshToken() {
        try {
            const response = await $.ajax({
                url: `${window.env.API_BASE_URL}/Auth/refresh-token`,
                type: "POST",
                xhrFields: { withCredentials: true }, // ✅ Send refresh token cookie
                contentType: "application/json"
            });

            if (response.success && response.data?.accessToken) {
                const newToken = response.data.accessToken;
                this.setAccessToken(newToken);
                console.log("✅ Access token refreshed successfully");
                return newToken;
            } else {
                throw new Error("Invalid refresh response");
            }

        } catch (error) {
            console.error("❌ Token refresh failed:", error);
            throw error;
        }
    },

    clear() {
        localStorage.removeItem("accessToken");
        // Clear any other auth data
    },

    isAuthenticated() {
        return !!this.getAccessToken();
    }
};