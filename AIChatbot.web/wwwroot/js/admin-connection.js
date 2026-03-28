(function () {
    'use strict';

    const API_BASE = '/api/supersetup/V1/setup-engine/connection';

    const sqlAuthOption = document.getElementById('sqlAuthOption');
    const windowsAuthOption = document.getElementById('windowsAuthOption');
    const sqlAuthRadio = document.getElementById('sqlAuth');
    const windowsAuthRadio = document.getElementById('windowsAuth');
    const credentialsRow = document.getElementById('credentialsRow');
    const testBtn = document.getElementById('testBtn');
    const saveBtn = document.getElementById('saveBtn');
    const statusMessage = document.getElementById('statusMessage');

    // Auth toggle
    function updateAuthUI(mode) {
        if (mode === 'SqlAuth') {
            sqlAuthOption.classList.add('selected');
            windowsAuthOption.classList.remove('selected');
            credentialsRow.classList.remove('hidden-smooth');
        } else {
            windowsAuthOption.classList.add('selected');
            sqlAuthOption.classList.remove('selected');
            credentialsRow.classList.add('hidden-smooth');
        }
    }

    sqlAuthOption.addEventListener('click', () => {
        sqlAuthRadio.checked = true;
        updateAuthUI('SqlAuth');
    });

    windowsAuthOption.addEventListener('click', () => {
        windowsAuthRadio.checked = true;
        updateAuthUI('WindowsAuth');
    });

    updateAuthUI('WindowsAuth');

    // Build payload matching API schema exactly
    function getPayload() {
        const existingId = document.getElementById('connectionId').value.trim();
        const authMode = document.querySelector('input[name="authMode"]:checked').value;

        return {
            id: existingId || '3fa85f64-5717-4562-b3fc-2c963f66afa6',
            serverName: document.getElementById('serverName').value.trim(),
            databaseName: document.getElementById('databaseName').value.trim(),
            authMode: authMode,
            username: document.getElementById('username').value.trim(),
            password: document.getElementById('password').value,
            trustCertificate: document.getElementById('trustCertificate').checked,
            connectionTimeout: parseInt(document.getElementById('connectionTimeout').value) || 0,
            isActive: true
        };
    }

    function validate(payload) {
        if (!payload.serverName) return 'Server Name is required.';
        if (!payload.databaseName) return 'Database Name is required.';
        if (payload.authMode === 'SqlAuth') {
            if (!payload.username) return 'Username is required for SQL Authentication.';
            if (!payload.password) return 'Password is required for SQL Authentication.';
        }
        return null;
    }

    function showStatus(type, message) {
        const icons = { success: '✅', error: '❌', info: 'ℹ️' };
        statusMessage.className = `status-message ${type}`;
        statusMessage.textContent = `${icons[type] || ''} ${message}`;
        statusMessage.classList.remove('hidden');
        setTimeout(() => statusMessage.classList.add('hidden'), 5000);
    }

    function setLoading(btn, loading) {
        btn.disabled = loading;
        btn.querySelector('.btn-text').style.display = loading ? 'none' : '';
        btn.querySelector('.btn-spinner').classList.toggle('hidden', !loading);
    }

    // Test Connection
    testBtn.addEventListener('click', async () => {
        const payload = getPayload();
        const err = validate(payload);
        if (err) { showStatus('error', err); return; }

        setLoading(testBtn, true);
        showStatus('info', 'Testing connection...');

        try {
            const res = await fetch(API_BASE + '/test', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (res.ok) {
                showStatus('success', 'Connection successful! Database is reachable.');
            } else {
                const data = await res.json().catch(() => ({}));
                showStatus('error', data.message || `Test failed (${res.status}).`);
            }
        } catch (e) {
            showStatus('error', 'Network error. Could not reach the server.');
        } finally {
            setLoading(testBtn, false);
        }
    });

    // Save Connection
    saveBtn.addEventListener('click', async () => {
        const payload = getPayload();
        const err = validate(payload);
        if (err) { showStatus('error', err); return; }

        setLoading(saveBtn, true);

        try {
            const res = await fetch(API_BASE, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (res.ok) {
                const data = await res.json().catch(() => ({}));
                if (data.id) document.getElementById('connectionId').value = data.id;
                showStatus('success', 'Connection saved successfully!');
            } else {
                const data = await res.json().catch(() => ({}));
                showStatus('error', data.message || `Save failed (${res.status}).`);
            }
        } catch (e) {
            showStatus('error', 'Network error. Could not save the connection.');
        } finally {
            setLoading(saveBtn, false);
        }
    });
})();