var DatabaseConnection = (function () {

    // ─── Init ────────────────────────────────────────────────────────────────

    function init() {
        bindStaticEvents();
        bindRowEvents();
    }

    // ─── Event Binding ───────────────────────────────────────────────────────

    function bindStaticEvents() {
        $('#saveEditBtn').on('click', handleSaveModal);
        $('#openAddConnectionBtn').on('click', openAddModal);
        $('#saveConnectionBtn').on('click', handleSaveConnection);
    }

    function openAddModal() {
        $('#editId').val('');
        $('#editServer').val('');
        $('#editDatabase').val('');
        $('#editUsername').val('');
        $('#editPassword').val('');
        $('#editTimeout').val('30');
        $('#editTrust').prop('checked', true);
        $('#editModal .modal-title').text('Add New Connection');
        $('#saveEditBtn').text('Test & Save Connection');
        const modalEl = document.getElementById('editModal');
        const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
        modal.show();
    }

    async function handleSaveModal() {
        const id = $('#editId').val();
        if (id && id.trim() !== '') {
            await saveEdit();
            return;
        }
        await saveAddFromModal();
    }

    async function saveAddFromModal() {
        const serverName = $('#editServer').val().trim();
        const databaseName = $('#editDatabase').val().trim();
        const username = $('#editUsername').val().trim();
        const password = $('#editPassword').val().trim();
        const timeout = parseInt($('#editTimeout').val(), 10);

        if (!serverName) {
            showToast('Server name is required ❌', 'danger', true);
            $('#editServer').focus();
            return;
        }

        if (!databaseName) {
            showToast('Database name is required ❌', 'danger', true);
            $('#editDatabase').focus();
            return;
        }

        if (!username) {
            showToast('Username is required ❌', 'danger', true);
            $('#editUsername').focus();
            return;
        }

        if (!password) {
            showToast('Password is required ❌', 'danger', true);
            $('#editPassword').focus();
            return;
        }

        if (!Number.isFinite(timeout) || timeout < 0) {
            showToast('Timeout must be a valid number ❌', 'danger', true);
            $('#editTimeout').focus();
            return;
        }

        const payload = {
            serverName: serverName,
            databaseName: databaseName,
            authMode: 'Sql',
            username: username,
            password: password,
            connectionTimeout: timeout,
            trustCertificate: $('#editTrust').is(':checked')
        };

        $('#saveEditBtn').prop('disabled', true).text('Saving...');
        showToast('Saving connection...', 'info', false);

        try {
            const res = await fetch('/Admin/SaveConnection', {
                method: 'POST',
                headers: jsonHeaders(),
                body: JSON.stringify(payload)
            });

            const contentType = res.headers.get('content-type') ?? '';

            if (contentType.includes('text/html')) {
                const rowHtml = await res.text();
                $('#connectionsTableBody').prepend(rowHtml);
                bindRowEvents();
                bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
                showToast('Connection added successfully ✅', 'success', true);
            } else {
                const data = await res.json();
                showToast(data.message ?? 'Failed to save connection ❌', 'danger', true);
            }
        } catch (err) {
            console.error('SaveConnection (modal) error:', err);
            showToast('Network error — could not reach server ❌', 'danger', true);
        } finally {
            $('#saveEditBtn').prop('disabled', false).text('Save Changes');
        }
    }

    // Separate so it can be re-called after new rows are injected
    function bindRowEvents() {
        $(document).off('change', '.active-switch').on('change', '.active-switch', handleActiveSwitch);
        $(document).off('click', '.kb-btn').on('click', '.kb-btn', handleKnowledgeBase);
        $(document).off('click', '.edit-btn').on('click', '.edit-btn', openEditModal);
        $(document).off('click', '.delete-btn').on('click', '.delete-btn', handleDeleteConnection);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

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
        const serverName = $('#ServerName').val().trim();
        const databaseName = $('#DatabaseName').val().trim();
        const username = $('#Username').val().trim();
        const password = $('#Password').val().trim();
        const timeout = parseInt($('#ConnectionTimeout').val(), 10);

        if (!serverName) {
            showToast('Server name is required ❌', 'danger', true);
            $('#ServerName').focus();
            return;
        }

        if (!databaseName) {
            showToast('Database name is required ❌', 'danger', true);
            $('#DatabaseName').focus();
            return;
        }

        if (!username) {
            showToast('Username is required ❌', 'danger', true);
            $('#Username').focus();
            return;
        }

        if (!password) {
            showToast('Password is required ❌', 'danger', true);
            $('#Password').focus();
            return;
        }

        if (!Number.isFinite(timeout) || timeout < 0) {
            showToast('Connection timeout must be a valid number ❌', 'danger', true);
            $('#ConnectionTimeout').focus();
            return;
        }

        const payload = {
            serverName: serverName,
            databaseName: databaseName,
            authMode: 'Sql',
            username: username,
            password: password,
            connectionTimeout: timeout,
            trustCertificate: $('#TrustServerCertificate').is(':checked')
        };

        //if (!payload.serverName || !payload.databaseName) {
        //    showToast('Server name and database name are required ❌', 'danger', true);
        //    return;
        //}

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
            const endpoint = isUpdate ? '/Admin/UpdateKB' : '/Admin/CreateKB';
            const res = await fetch(endpoint, {
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
        $('#editPassword').val('').attr('placeholder', 'Enter password');
        $('#editTimeout').val(this.dataset.timeout);
        $('#editTrust').prop('checked', this.dataset.trust === 'true');

        // FIX: reuse instance — prevents double-mount and aria-hidden bug
        const modalEl = document.getElementById('editModal');
        const modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
        modal.show();
    }

    async function saveEdit() {
        const serverName = $('#editServer').val().trim();
        const databaseName = $('#editDatabase').val().trim();
        const username = $('#editUsername').val().trim();
        const password = $('#editPassword').val().trim();
        const timeout = parseInt($('#editTimeout').val(), 10);

        if (!serverName) {
            showToast('Server name is required ❌', 'danger', true);
            $('#editServer').focus();
            return;
        }

        if (!databaseName) {
            showToast('Database name is required ❌', 'danger', true);
            $('#editDatabase').focus();
            return;
        }

        if (!username) {
            showToast('Username is required ❌', 'danger', true);
            $('#editUsername').focus();
            return;
        }

        if (!password) {
            showToast('Password is required ❌', 'danger', true);
            $('#editPassword').focus();
            return;
        }

        if (!Number.isFinite(timeout) || timeout < 0) {
            showToast('Timeout must be a valid number ❌', 'danger', true);
            $('#editTimeout').focus();
            return;
        }

        const payload = {
            id: $('#editId').val(),
            serverName: serverName,
            databaseName: databaseName,
            authMode: 'Sql',
            username: username,
            connectionTimeout: timeout,
            trustCertificate: $('#editTrust').is(':checked'),
            isActive: false
        };

        payload.password = password;

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