document.addEventListener("DOMContentLoaded", () => {

    if (!window.toastr) return;

    // Toastr configuration
    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-right",
        timeOut: 4000
    };

    const { success, error, info, warning } = document.body.dataset;

    if (success?.trim()) toastr.success(success);
    if (error?.trim()) toastr.error(error);
    if (info?.trim()) toastr.info(info);
    if (warning?.trim()) toastr.warning(warning);

});