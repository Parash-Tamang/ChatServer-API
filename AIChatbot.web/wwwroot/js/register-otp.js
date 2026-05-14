// register-otp.js

document.addEventListener('DOMContentLoaded', function () {

    const emailInput   = document.getElementById('emailInput');
    const sendOtpRow   = document.getElementById('sendOtpRow');
    const otpSection   = document.getElementById('otpSection');
    const verifiedIcon = document.getElementById('verifiedIcon');

    // ── Email Input Validation ───────────────────────────
    window.onEmailInput = function () {
        const valid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(emailInput.value.trim());
        sendOtpRow.style.display = valid ? 'block' : 'none';
        otpSection.style.display = 'none';
    };

    // ── Send OTP ─────────────────────────────────────────
    window.sendOtp = async function (resend = false) {
        const email = emailInput.value.trim();
        const link  = document.getElementById('sendOtpLink');

        if (link) link.textContent = 'Sending...';

        try {
            const res  = await fetch('/Auth/SendRegisterOtp', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email })
            });
            const data = await res.json();

            if (!data.success) {
                toastr.error(data.error || 'Failed to send OTP.');
                return;
            }

            document.getElementById('emailDisplay').textContent = email;
            toastr.success(resend ? 'OTP resent!' : 'OTP sent to ' + email);
            sendOtpRow.style.display = 'none';
            otpSection.style.display = 'block';
            document.getElementById('otpInput').focus();

        } catch {
            toastr.error('Something went wrong. Please try again.');
        } finally {
            if (link) link.textContent = 'Send OTP';
        }
    };

    // ── Load Roles ────────────────────────────────────────
    async function loadRoles() {
        const token  = document.getElementById('hToken').value;
        const res    = await fetch(`/Auth/GetRoles?token=${token}`);
        const roles  = await res.json();
        const select = document.getElementById('roleSelect');

        select.innerHTML = '<option value="">Select role</option>';
        roles.forEach(role => {
            const opt       = document.createElement('option');
            opt.value       = role;
            opt.textContent = role;
            select.appendChild(opt);
        });

        select.disabled         = false;
        select.style.cursor     = 'pointer';
        select.style.background = '#fff';
        select.style.color      = '#111';
    }

    // ── Verify OTP ────────────────────────────────────────
    window.verifyOtp = async function () {
        const otp   = document.getElementById('otpInput').value.trim();
        const email = emailInput.value.trim();

        if (otp.length !== 6) {
            toastr.error('Please enter the complete 6-digit OTP.');
            return;
        }

        const btn       = document.getElementById('verifyBtn');
        btn.disabled    = true;
        btn.textContent = 'Verifying...';

        try {
            const res  = await fetch('/Auth/VerifyRegisterOtp', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, otp })
            });
            const data = await res.json();

            if (!data.success) {
                toastr.error(data.error || 'Invalid or expired OTP.');
                return;
            }

            document.getElementById('hToken').value = data.registerToken;
            toastr.success('Email verified!');

            otpSection.style.display      = 'none';
            sendOtpRow.style.display      = 'none';
            verifiedIcon.style.display    = 'block';
            emailInput.readOnly           = true;
            emailInput.style.paddingRight = '36px';

            await loadRoles();

        } catch {
            toastr.error('Something went wrong. Please try again.');
        } finally {
            btn.disabled    = false;
            btn.textContent = 'Verify';
        }
    };

});