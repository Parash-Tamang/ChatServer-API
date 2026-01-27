// ================================
// Global Toastr Configuration Helper
// This file runs once and sets
// Toastr behaviour for the entire app
// ================================
(function () {

    // Safety check: make sure toastr is loaded before configuring
    if (typeof toastr === "undefined") {
        console.error("Toastr library is not loaded!");
        return;
    }

    toastr.options = {

        closeButton: true,
        debug: false,
        newestOnTop: true,
        progressBar: true,
        positionClass: "toast-top-right",
        preventDuplicates: true,
        onclick: null,

        showDuration: 200,      // 🔽 Faster show
        hideDuration: 150,      // 🔽 MUCH faster disappear
        timeOut: 4000,          // 🔽 Shorter lifetime
        extendedTimeOut: 1000,  // 🔽 Faster hover close

        showEasing: "swing",
        hideEasing: "linear",
        showMethod: "fadeIn",
        hideMethod: "fadeOut",
        autoDismiss: true
    };
})();
