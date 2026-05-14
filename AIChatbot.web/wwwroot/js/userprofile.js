// wwwroot/js/userprofile.js

function openUserProfileModal() {
    const modalEl = document.getElementById('userProfileModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);

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

            document.getElementById('profile-initials').textContent = initials;
            document.getElementById('profile-fullname').textContent = `${first} ${last}`.trim();
            document.getElementById('profile-role').textContent = role;
            document.getElementById('profile-firstname').textContent = first || 'N/A';
            document.getElementById('profile-lastname').textContent = last || 'N/A';
            document.getElementById('profile-email').textContent = data.email || 'N/A';
            document.getElementById('profile-phone').textContent = data.phone || 'N/A';

            document.getElementById('profile-loading').classList.add('d-none');
            document.getElementById('profile-content').classList.remove('d-none');
        })
        .catch(err => {
            document.getElementById('profile-loading').classList.add('d-none');
            document.getElementById('profile-error-msg').textContent = err.message;
            document.getElementById('profile-error').classList.remove('d-none');
        });
}