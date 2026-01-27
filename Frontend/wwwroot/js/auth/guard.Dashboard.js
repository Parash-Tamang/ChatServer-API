$.ajax({
    url: `${window.env.API_BASE_URL}/data`,
    type: "GET",
  
    success: function (data) {
        console.log("Dashboard data:", data);
    },
    error: function (xhr) {
        if (xhr.status === 401) {
            console.log(xhr);
            window.location.replace("/Account/Login");
        }
    }
});

