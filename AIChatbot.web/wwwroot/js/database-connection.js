var DatabaseConnection = (function () {

    // ─── Init ────────────────────────────────────────────────────────────────

    function init() {
        bindStaticEvents();
        bindRowEvents();
        handleAuthToggle('input[name="AuthMode"]', '#sqlCredentials');
        handleAuthToggle('input[name="editAuthMode"]', '#editSqlCredentials');
    }

    // ─── Event Binding ───────────────────────────────────────────────────────

    function bindStaticEvents() {
        $('input[name="AuthMode"]').on('change', function () {
            handleAuthToggle('input[name="AuthMode"]', '#sqlCredentials');
        });

        $('input[name="editAuthMode"]').on('change', function () {
            handleAuthToggle('input[name="editAuthMode"]', '#editSqlCredentials');
        });

        $('#saveEditBtn').on('click', saveEdit);
        $('#saveConnectionBtn').on('click', handleSaveConnection);
    }

    // Separate so it can be re-called after new rows are injected
    function bindRowEvents() {
        $(document).off('change', '.active-switch').on('change', '.active-switch', handleActiveSwitch);
        $(document).off('click', '.kb-btn').on('click', '.kb-btn', handleKnowledgeBase);
        $(document).off('click', '.edit-btn').on('click', '.edit-btn', openEditModal);
        $(document).off('click', '.delete-btn').on('click', '.delete-btn', handleDeleteConnection);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    function handleAuthToggle(selector, credentialsId) {
        const selected = document.querySelector(selector + ':checked');
        const creds = document.querySelector(credentialsId);
        if (!creds) return;
        creds.style.display = selected?.value === 'Windows' ? 'none' : 'block';
    }

    function getCsrfToken() {
        return document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? '';
    }

    function jsonHeaders() {
        return {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': getCsrfToken()
        };
    }

    // ─── Save New Connection ─────────────────────────────────────────────────

    async function handleSaveConnection() {
        const authMode = $('input[name="AuthMode"]:checked').val();
        const password = $('#Password').val();

        const payload = {
            serverName: $('#ServerName').val().trim(),
            databaseName: $('#DatabaseName').val().trim(),
            authMode: authMode,
            username: authMode === 'Sql' ? $('#Username').val().trim() : null,
            password: authMode === 'Sql' && password !== '' ? password : null,
            connectionTimeout: parseInt($('#ConnectionTimeout').val()) || 30,
            trustServerCertificate: $('#TrustServerCertificate').is(':checked')
        };

        if (!payload.serverName || !payload.databaseName) {
            showToast('Server name and database name are required ❌', 'danger', true);
            return;
        }

        $('#saveConnectionBtn').prop('disabled', true).text('Saving...');
        showToast('Saving connection...', 'info', false);

        try {
            const res = await fetch('/Admin/SaveConnection', {
                method: 'POST',
                headers: jsonHeaders(),
                body: JSON.stringify(payload)
            });

            const contentType = res.headers.get('content-type') ?? '';

            if (contentType.includes('text/html')) {
                // Controller returned a partial view row — append it
                const rowHtml = await res.text();
                $('#connectionsTableBody').append(rowHtml);
                bindRowEvents();
                clearAddForm();
                showToast('Connection saved successfully ✅', 'success', true);
            } else {
                const data = await res.json();
                showToast(data.message ?? 'Failed to save connection ❌', 'danger', true);
            }
        } catch (err) {
            console.error('SaveConnection error:', err);
            showToast('Network error — could not reach server ❌', 'danger', true);
        } finally {
            $('#saveConnectionBtn').prop('disabled', false).text('Save');
        }
    }

    function clearAddForm() {
        $('#ServerName').val('');
        $('#DatabaseName').val('');
        $('#Username').val('');
        $('#Password').val('');
        $('#ConnectionTimeout').val('30');
        $('#TrustServerCertificate').prop('checked', false);
        $('input[name="AuthMode"][value="Sql"]').prop('checked', true);
        handleAuthToggle('input[name="AuthMode"]', '#sqlCredentials');
    }

    // ─── Active Switch ───────────────────────────────────────────────────────

    async function handleActiveSwitch() {
        if (this.disabled) return;

        if (!this.checked) {
            this.checked = true;
            return;
        }

        const id = this.dataset.id;
        const $switch = $(this);

        $switch.prop('disabled', true);
        showToast('Activating connection...', 'info', false);

        try {
            const res = await fetch('/Admin/SetActive', {
                method: 'POST',
                headers: jsonHeaders(),
                body: JSON.stringify(id)
            });
            const data = await res.json();

            if (data.success) {
                $('.active-switch').each(function () {
                    this.checked = this.dataset.id === id;
                });
                showToast('Connection activated successfully ✅', 'success', true);
            } else {
                this.checked = false;
                showToast('Failed to activate connection ❌', 'danger', true);
            }
        } catch (err) {
            console.error('SetActive error:', err);
            this.checked = false;
            showToast('Network error — could not reach server ❌', 'danger', true);
        } finally {
            $switch.prop('disabled', false);
        }
    }

    // ─── Knowledge Base ──────────────────────────────────────────────────────

    async function handleKnowledgeBase() {
        const id = this.dataset.id;
        const isUpdate = this.dataset.action === 'update';
        const $btn = $(this);

        if (isUpdate) {
            const confirmed = confirm('Are you sure you want to update the knowledge base? This may take some time.');
            if (!confirmed) return;
        }

        $btn.prop('disabled', true).text(isUpdate ? 'Updating...' : 'Creating...');
        showToast(isUpdate ? 'Updating knowledge base...' : 'Creating knowledge base...', 'info', false);

        try {
            const res = await fetch('/Admin/UpdateKB', {
                method: 'POST',
                headers: jsonHeaders(),
                body: JSON.stringify(id)
            });
            const data = await res.json();

            if (data.success) {
                showToast(isUpdate ? 'Knowledge base updated ✅' : 'Knowledge base created ✅', 'success', true);
                setTimeout(() => location.reload(), 1500);
            } else {
                showToast(isUpdate ? 'Failed to update knowledge base ❌' : 'Failed to create knowledge base ❌', 'danger', true);
                $btn.prop('disabled', false).text(isUpdate ? 'Update KB' : 'Create KB');
            }
        } catch (err) {
            console.error('UpdateKB error:', err);
            showToast('Network error — could not reach server ❌', 'danger', true);
            $btn.prop('disabled', false).text(isUpdate ? 'Update KB' : 'Create KB');
        }
    }

    // ─── Edit Modal ──────────────────────────────────────────────────────────

    function openEditModal() {
        $('#editId').val(this.dataset.id);
        $('#editServer').val(this.dataset.server);
        $('#editDatabase').val(this.dataset.database);
        $('#editUsername').val(this.dataset.username);
        $('#editPassword').val('').attr('placeholder', '•••••••• (leave blank to keep current)');
        $('#editTimeout').val(this.dataset.timeout);
        $('#editTrust').prop('checked', this.dataset.trust === 'true');

        $(`input[name="editAuthMode"][value="${this.dataset.authmode}"]`).prop('checked', true);
        handleAuthToggle('input[name="editAuthMode"]', '#editSqlCredentials');

        // FIX: reuse instance — prevents double-mount and aria-hidden bug
        const modalEl = document.getElementById('editModal');
        const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
        modal.show();
    }

    async function saveEdit() {
        const newPassword = $('#editPassword').val();

        const payload = {
            id: $('#editId').val(),
            serverName: $('#editServer').val().trim(),
            databaseName: $('#editDatabase').val().trim(),
            authMode: $('input[name="editAuthMode"]:checked').val(),
            username: $('#editUsername').val().trim(),
            connectionTimeout: parseInt($('#editTimeout').val()) || 30,
            trustServerCertificate: $('#editTrust').is(':checked'),
            isActive: false
        };

        if (newPassword.trim() !== '') {
            payload.password = newPassword;
        }

        if (!payload.serverName || !payload.databaseName) {
            showToast('Server name and database name are required ❌', 'danger', true);
            return;
        }

        $('#saveEditBtn').prop('disabled', true).text('Saving...');
        showToast('Updating connection...', 'info', false);

        try {
            const res = await fetch('/Admin/UpdateConnection', {
                method: 'POST',
                headers: jsonHeaders(),
                body: JSON.stringify(payload)
            });
            const data = await res.json();

            // FIX: modal.hide() called AFTER fetch resolves — prevents ERR_NETWORK_IO_SUSPENDED
            if (data.success) {
                bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
                showToast('Connection updated successfully ✅', 'success', true);
                setTimeout(() => location.reload(), 1500);
            } else {
                showToast('Failed to update connection ❌', 'danger', true);
            }
        } catch (err) {
            console.error('UpdateConnection error:', err);
            showToast('Network error — could not reach server ❌', 'danger', true);
        } finally {
            $('#saveEditBtn').prop('disabled', false).text('Save Changes');
        }
    }

    // ─── Delete Connection ───────────────────────────────────────────────────

    async function handleDeleteConnection() {
        const id = this.dataset.id;
        const $row = $(this).closest('tr');
        const confirmed = confirm('Are you sure you want to delete this connection? This cannot be undone.');
        if (!confirmed) return;

        showToast('Deleting connection...', 'info', false);

        try {
            const res = await fetch('/Admin/DeleteConnection', {
                method: 'POST',
                headers: jsonHeaders(),
                body: JSON.stringify(id)
            });
            const data = await res.json();

            if (data.success) {
                // Remove row from DOM directly — no reload needed
                $row.fadeOut(400, function () { $(this).remove(); });
                showToast('Connection deleted successfully ✅', 'success', true);
            } else {
                showToast('Failed to delete connection ❌', 'danger', true);
            }
        } catch (err) {
            console.error('DeleteConnection error:', err);
            showToast('Network error — could not reach server ❌', 'danger', true);
        }
    }

    // ─── Toast ───────────────────────────────────────────────────────────────

    function showToast(message, type, autohide) {
        if (!window.toastr) return;
        if (type === 'success') toastr.success(message);
        else if (type === 'danger') toastr.error(message);
        else toastr.info(message);
    }

    // ─── Public API ──────────────────────────────────────────────────────────

    return { init };

})();

$(document).ready(function () {
    DatabaseConnection.init();
});