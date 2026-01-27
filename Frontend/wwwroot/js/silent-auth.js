// silent-auth.js
$(function () {

    (async function () {

        AuthService.refreshToken().then(() => {

            window.location.href = "/Chat/Index"
            console.log("silent auth success");
        })
            .catch((e) => {
                console.log( e + " : silent auth unsuccessful"); 
            })
    })();
});