document.addEventListener('DOMContentLoaded', () => {
    const boxes = document.querySelectorAll('.otp-box');

    boxes.forEach((box, i) => {
        box.addEventListener('focus', () => {
            box.style.borderColor = '#333';
            box.style.boxShadow = '0 0 0 3px rgba(0,0,0,0.08)';
            box.style.background = '#fff';
        });

        box.addEventListener('blur', () => {
            box.style.borderColor = box.value ? '#333' : '#ddd';
            box.style.boxShadow = 'none';
            box.style.background = '#fafafa';
        });

        box.addEventListener('input', () => {
            box.value = box.value.replace(/[^0-9]/g, '');
            if (box.value && i < boxes.length - 1) boxes[i + 1].focus();
            syncHidden();
            checkAutoSubmit();
        });

        box.addEventListener('keydown', e => {
            if (e.key === 'Backspace' && !box.value && i > 0) boxes[i - 1].focus();
        });

        box.addEventListener('paste', e => {
            const pasted = (e.clipboardData || window.clipboardData)
                .getData('text').replace(/\D/g, '');
            if (pasted.length === 6) {
                boxes.forEach((b, idx) => b.value = pasted[idx] || '');
                syncHidden();
                boxes[5].focus();
                e.preventDefault();
                checkAutoSubmit();
            }
        });
    });

    function syncHidden() {
        document.getElementById('otpValue').value =
            [...boxes].map(b => b.value).join('');
    }

    function checkAutoSubmit() {
        const full = [...boxes].every(b => b.value.length === 1);
        if (full) document.getElementById('verifyBtn').click();
    }

    boxes[0]?.focus();
});