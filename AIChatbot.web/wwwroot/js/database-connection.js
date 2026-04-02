var DatabaseConnection = (function () {

    function init() {
        bindEvents();
        handleAuthToggle('input[name="AuthType"]', '#sqlCredentials');
        handleAuthToggle('input[name="editAuthMode"]', '#editSqlCredentials');
    }

    function bindEvents() {

        // Auth toggle (Add form)
        $('input[name="AuthType"]').on('change', function () {
            handleAuthToggle('input[name="AuthType"]', '#sqlCredentials');
        });

        // Auth toggle (Edit form)
        $('input[name="editAuthMode"]').on('change', function () {
            handleAuthToggle('input[name="editAuthMode"]', '#editSqlCredentials');
        });

        // Test connection
        $('#testBtn').on('click', testConnection);

        // Active switch
        $('.active-switch').on('change', handleActiveSwitch);

        // Edit button
        $('.edit-btn').on('click', openEditModal);

        // Save edit
        $('#saveEditBtn').on('click', saveEdit);
    }

    // ✅ Single unified toggle function
    function handleAuthToggle(selector, credentialsId) {
        const selected = document.querySelector(selector + ':checked');
        const creds = document.querySelector(credentialsId);

        if (!creds) return;

        if (selected?.value === 'Windows') {
            creds.style.display = 'none';
        } else {
            creds.style.display = 'block';
        }
    }

    function testConnection() {

        var data = {
            serverName: $('input[name="ServerName"]').val(),
            databaseName: $('input[name="DatabaseName"]').val(),
            username: $('input[name="Username"]').val(),
            password: $('input[name="Password"]').val(),
            authMode: $('input[name="AuthType"]:checked').val()
        };

        $.ajax({
            url: '/Database/TestConnection',
            type: 'POST',
            data: data,
            success: function () {
                alert('Connection Successful ✅');
            },
            error: function () {
                alert('Connection Failed ❌');
            }
        });
    }

    async function handleActiveSwitch() {

        if (!this.checked) {
            this.checked = true;
            return;
        }

        const id = this.dataset.id;

        const res = await fetch('/Admin/SetActive', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id })
        });

        const data = await res.json();

        if (data.success) {
            $('.active-switch').each(function () {
                this.checked = this.dataset.id === id;
            });
        } else {
            this.checked = false;
            alert('Failed to set active connection.');
        }
    }

    function openEditModal() {

        $('#editId').val(this.dataset.id);
        $('#editServer').val(this.dataset.server);
        $('#editDatabase').val(this.dataset.database);
        $('#editUsername').val(this.dataset.username);
        $('#editPassword').val(this.dataset.password);
        $('#editTimeout').val(this.dataset.timeout);
        $('#editTrust').prop('checked', this.dataset.trust === 'true');

        const authMode = this.dataset.authmode;
        $(`input[name="editAuthMode"][value="${authMode}"]`).prop('checked', true);

        handleAuthToggle('input[name="editAuthMode"]', '#editSqlCredentials');

        new bootstrap.Modal(document.getElementById('editModal')).show();
    }

    async function saveEdit() {

        const payload = {
            id: $('#editId').val(),
            serverName: $('#editServer').val(),
            databaseName: $('#editDatabase').val(),
            authMode: $('input[name="editAuthMode"]:checked').val(),
            username: $('#editUsername').val(),
            password: $('#editPassword').val(),
            connectionTimeout: parseInt($('#editTimeout').val()),
            trustCertificate: $('#editTrust').is(':checked'),
            isActive: false
        };

        const res = await fetch('/Admin/UpdateConnection', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const data = await res.json();

        if (data.success) {
            bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
            location.reload();
        } else {
            alert('Failed to update connection.');
        }
    }

    return {
        init: init
    };

})();


// ✅ Safe init
$(document).ready(function () {
    DatabaseConnection.init();
});