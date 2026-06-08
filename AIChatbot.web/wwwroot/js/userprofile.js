// wwwroot/js/userprofile.js

function openUserProfileModal() {
    const modalEl = document.getElementById('userProfileModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);

    function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    // Reset state
    document.getElementById('profile-loading').classList.remove('d-none');
    document.getElementById('profile-content').classList.add('d-none');
    document.getElementById('profile-error').classList.add('d-none');

    modal.show();

    fetch('/Settings/UserProfile')
        .then(res => {
            if (!res.ok) return res.json().then(e => { throw new Error(e.message); });
            return res.json();
        })
        .then(data => {
            const first = data.firstName || '';
            const last = data.lastName || '';
            const initials = (first.charAt(0) + last.charAt(0)).toUpperCase() || '?';
            const role = data.roles?.length ? data.roles[0] : 'User';

            setText('profile-initials', initials);
            setText('profile-fullname', `${first} ${last}`.trim());
            setText('profile-role', role);
            setText('profile-firstname', first || 'N/A');
            setText('profile-lastname', last || 'N/A');
            setText('profile-email', data.email || 'N/A');
            setText('profile-phone', data.phone || 'N/A');
            setText('profile-role-value', role);

            document.getElementById('profile-loading').classList.add('d-none');
            document.getElementById('profile-content').classList.remove('d-none');
        })
        .catch(err => {
            document.getElementById('profile-loading').classList.add('d-none');
            document.getElementById('profile-error-msg').textContent = err.message;
            document.getElementById('profile-error').classList.remove('d-none');
        });
}