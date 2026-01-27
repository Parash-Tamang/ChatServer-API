// ============================================
// GLOBAL VALIDATION TOAST + BUTTON LOCK HELPER
// Shows all errors as Toastr warnings
// Disables a button for X seconds
// ============================================

/**
 * Shows validation errors and disables a button temporarily
 * @param {string[]} errors - Array of error messages
 * @param {string|HTMLElement} buttonSelector - Button to disable
 * @param {number} timeoutMs - Lock time in milliseconds (default 3000)
 */window.showValidationErrorsWithLock = function (
    errors,
    buttonSelector,
    timeoutMs = 500
) {
    if (!errors || errors.length === 0) return;

    // ✅ Show all errors
    errors.forEach(err => toastr.warning(err));

    // ✅ Lock the button
    let btn = $(buttonSelector);
    btn.prop("disabled", true);

    // ✅ Unlock after timeout
    setTimeout(function () {
        btn.prop("disabled", false);
    }, timeoutMs);
};
