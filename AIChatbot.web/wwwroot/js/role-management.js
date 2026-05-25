// role-management.js

let currentRoleId = null;
let currentRoleName = null;
let pendingDeleteRoleId = null;
let pendingDeleteUserId = null;
let allUsersCache = [];

// ─── Panel Switching ───────────────────────────────────────────────────────

// All panel/card IDs in one place — add new ones here only
const ALL_PANELS = ['create', 'list', 'users', 'assign-db', 'view-access'];
const PANEL_CARD_MAP = {
    'create': 'card-create',
    'list': 'card-list',
    'users': 'card-members',
    'assign-db': 'card-assign-db',
    'view-access': 'card-view-access'
};
function switchPanel(name) {
    ALL_PANELS.forEach(p => {
        document.getElementById('panel-' + p).style.display = 'none';
        const cardId = PANEL_CARD_MAP[p];
        if (cardId) document.getElementById(cardId)?.classList.remove('active');
    });

    document.getElementById('panel-' + name).style.display = 'block';

    const activeCard = PANEL_CARD_MAP[name];
    if (activeCard) document.getElementById(activeCard)?.classList.add('active');

    if (name === 'list') loadRoles();
    if (name === 'assign-db') loadAssignPanel();
    if (name === 'view-access') loadViewAccessPanel();
}

// ─── Load Roles ────────────────────────────────────────────────────────────

function loadRoles() {
    showRolesLoading();

    $.ajax({
        url: '/Admin/GetRoles',
        method: 'GET',
        success: function (res) {
            if (res.success && res.data) renderRoles(res.data);
            else showRolesEmpty();
        },
        error: function () {
            showRolesEmpty();
            toastr.error('Failed to load roles. Please try again.');
        }
    });
}

function renderRoles(roles) {
    const grid = document.getElementById('roles-grid');
    const badge = document.getElementById('roles-count-badge');
    const label = document.getElementById('roles-count-label');

    badge.textContent = roles.length;
    label.textContent = roles.length + ' role' + (roles.length !== 1 ? 's' : '') + ' configured';

    if (roles.length === 0) { showRolesEmpty(); return; }

    grid.innerHTML = roles.map(role => `
        <div class="rm-role-card"
             onclick="openUsersPanel('${escHtml(role.roleId)}', '${escHtml(role.roleName)}')">
            <div class="rm-role-card-top">
                <span class="rm-role-name">${escHtml(role.roleName)}</span>
            </div>
            <div class="rm-role-meta">
                <i class="bi bi-people"></i> Click to view members
            </div>
            <div class="rm-role-card-actions">
                <button class="rm-btn-ghost"
                    onclick="event.stopPropagation();
                             openUsersPanel('${escHtml(role.roleId)}', '${escHtml(role.roleName)}')">
                    <i class="bi bi-people"></i> Members
                </button>
                <button class="rm-btn-danger"
                    onclick="event.stopPropagation();
                             openDeleteRoleModal('${escHtml(role.roleId)}', '${escHtml(role.roleName)}')">
                    <i class="bi bi-trash"></i> Delete
                </button>
            </div>
        </div>
    `).join('');

    document.getElementById('roles-loading').style.display = 'none';
    document.getElementById('roles-empty').style.display = 'none';
    grid.style.display = 'grid';
}

function showRolesLoading() {
    document.getElementById('roles-loading').style.display = 'block';
    document.getElementById('roles-empty').style.display = 'none';
    document.getElementById('roles-grid').style.display = 'none';
}

function showRolesEmpty() {
    document.getElementById('roles-loading').style.display = 'none';
    document.getElementById('roles-empty').style.display = 'block';
    document.getElementById('roles-grid').style.display = 'none';
}

// ─── Create Role ───────────────────────────────────────────────────────────

function createRole() {
    const name = document.getElementById('input-role-name').value.trim();
    const alertBox = document.getElementById('create-alert');
    const btn = document.getElementById('btn-create-role');

    if (!name) {
        showAlert(alertBox, 'error', '<i class="bi bi-exclamation-circle"></i> Role name is required.');
        return;
    }

    btn.disabled = true;
    btn.innerHTML = '<span class="rm-spinner" style="width:16px;height:16px;border-width:2px;margin:0 4px 0 0;"></span> Creating...';
    hideAlert(alertBox);

    $.ajax({
        url: '/Admin/CreateRole',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ roleName: name }),
        success: function (res) {
            if (res.success) {
                showAlert(alertBox, 'success', '<i class="bi bi-check-circle"></i> ' + res.message);
                document.getElementById('input-role-name').value = '';
                document.getElementById('input-role-desc').value = '';
                document.getElementById('roles-count-label').textContent = 'Refresh to see new role';
                toastr.success(res.message);
            } else {
                showAlert(alertBox, 'error', '<i class="bi bi-exclamation-circle"></i> ' + res.message);
            }
        },
        error: function () {
            showAlert(alertBox, 'error', '<i class="bi bi-exclamation-circle"></i> An error occurred. Please try again.');
        },
        complete: function () {
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-plus-lg"></i> Create Role';
        }
    });
}

// ─── Delete Role ───────────────────────────────────────────────────────────

function openDeleteRoleModal(roleId, roleName) {
    pendingDeleteRoleId = roleId;
    document.getElementById('modal-role-name').textContent = '"' + roleName + '"';
    document.getElementById('modal-delete-role').style.display = 'flex';
}

function confirmDeleteRole() {
    if (!pendingDeleteRoleId) return;

    const btn = document.getElementById('btn-confirm-delete-role');
    btn.disabled = true;
    btn.innerHTML = '<span class="rm-spinner" style="width:14px;height:14px;border-width:2px;margin:0 4px 0 0;"></span> Deleting...';

    $.ajax({
        url: '/Admin/DeleteRole',
        method: 'DELETE',
        contentType: 'application/json',
        data: JSON.stringify({ roleId: pendingDeleteRoleId }),
        success: function (res) {
            closeModal('modal-delete-role');
            if (res.success) { toastr.success(res.message); loadRoles(); }
            else toastr.error(res.message);
        },
        error: function () {
            closeModal('modal-delete-role');
            toastr.error('Failed to delete role. Please try again.');
        },
        complete: function () {
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-trash"></i> Delete Role';
            pendingDeleteRoleId = null;
        }
    });
}

// ─── Load Users in Role ────────────────────────────────────────────────────

function openUsersPanel(roleId, roleName) {
    currentRoleId = roleId;
    currentRoleName = roleName;
    switchPanel('users');

    document.getElementById('users-role-name').textContent = roleName;
    document.getElementById('users-search').value = '';
    showUsersLoading();

    $.ajax({
        url: '/Admin/GetUsersInRole',
        method: 'GET',
        data: { roleId },
        success: function (res) {
            if (res.success && res.data) { allUsersCache = res.data; renderUsers(res.data); }
            else { allUsersCache = []; showUsersEmpty(); }
        },
        error: function () {
            showUsersEmpty();
            toastr.error('Failed to load members. Please try again.');
        }
    });
}

function renderUsers(users) {
    const list = document.getElementById('users-list');
    const badge = document.getElementById('users-count-badge');

    badge.textContent = users.length;

    if (users.length === 0) { showUsersEmpty(); return; }

    list.innerHTML = users.map(u => {
        const initials = getInitials(u.firstName, u.lastName);
        const fullName = ((u.firstName || '') + ' ' + (u.lastName || '')).trim() || 'Unknown';
        return `
            <div class="rm-user-row" id="user-row-${escHtml(u.userId)}">
                <div class="rm-avatar">${escHtml(initials)}</div>
                <div class="rm-user-info">
                    <div class="rm-user-name">${escHtml(fullName)}</div>
                    <div class="rm-user-email">${escHtml(u.email || '')}</div>
                </div>
                <button class="rm-btn-danger"
                    onclick="openDeleteUserModal('${escHtml(u.userId)}', '${escHtml(fullName)}')">
                    <i class="bi bi-person-x"></i> Delete
                </button>
            </div>
        `;
    }).join('');

    document.getElementById('users-loading').style.display = 'none';
    document.getElementById('users-empty').style.display = 'none';
    list.style.display = 'flex';
}

function filterUsers() {
    const query = document.getElementById('users-search').value.toLowerCase();
    const filtered = allUsersCache.filter(u => {
        const name = ((u.firstName || '') + ' ' + (u.lastName || '')).toLowerCase();
        const email = (u.email || '').toLowerCase();
        return name.includes(query) || email.includes(query);
    });
    renderUsers(filtered);
}

function showUsersLoading() {
    document.getElementById('users-loading').style.display = 'block';
    document.getElementById('users-empty').style.display = 'none';
    document.getElementById('users-list').style.display = 'none';
}

function showUsersEmpty() {
    document.getElementById('users-loading').style.display = 'none';
    document.getElementById('users-empty').style.display = 'block';
    document.getElementById('users-list').style.display = 'none';
}

// ─── Delete User ───────────────────────────────────────────────────────────

function openDeleteUserModal(userId, userName) {
    pendingDeleteUserId = userId;
    document.getElementById('modal-user-name').textContent = '"' + userName + '"';
    document.getElementById('modal-delete-user').style.display = 'flex';
}

function confirmDeleteUser() {
    if (!pendingDeleteUserId) return;

    const btn = document.getElementById('btn-confirm-delete-user');
    btn.disabled = true;
    btn.innerHTML = '<span class="rm-spinner" style="width:14px;height:14px;border-width:2px;margin:0 4px 0 0;"></span> Deleting...';

    $.ajax({
        url: '/Admin/DeleteUser',
        method: 'DELETE',
        contentType: 'application/json',
        data: JSON.stringify({ userId: pendingDeleteUserId }),
        success: function (res) {
            closeModal('modal-delete-user');
            if (res.success) {
                allUsersCache = allUsersCache.filter(u => u.userId !== pendingDeleteUserId);
                renderUsers(allUsersCache);
                toastr.success(res.message);
            } else {
                toastr.error(res.message);
            }
        },
        error: function () {
            closeModal('modal-delete-user');
            toastr.error('Failed to delete user. Please try again.');
        },
        complete: function () {
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-trash"></i> Delete User';
            pendingDeleteUserId = null;
        }
    });
}

// ─── Assign DB to Role ─────────────────────────────────────────────────────

function loadAssignPanel() {
    const roleSelect = document.getElementById('assign-role-select');
    const connSelect = document.getElementById('assign-conn-select');

    // reset
    roleSelect.innerHTML = '<option value="">— loading roles —</option>';
    connSelect.innerHTML = '<option value="">— loading connections —</option>';
    roleSelect.disabled = true;
    connSelect.disabled = true;
    document.getElementById('assign-preview').style.display = 'none';

    // fetch roles and connections in parallel
    Promise.all([
        fetch('/Admin/GetRoles').then(r => r.json()),
        fetch('/Admin/GetConnections').then(r => r.json())
    ])
        .then(([rolesRes, connsRes]) => {
            // populate roles
            if (rolesRes.success && rolesRes.data?.length) {
                roleSelect.innerHTML = '<option value="">— select a role —</option>' +
                    rolesRes.data.map(r =>
                        `<option value="${escHtml(r.roleId)}">${escHtml(r.roleName)}</option>`
                    ).join('');
            } else {
                roleSelect.innerHTML = '<option value="">— no roles found —</option>';
            }

            // populate connections
            if (connsRes.success && connsRes.data?.length) {
                connSelect.innerHTML = '<option value="">— select a connection —</option>' +
                    connsRes.data.map(c =>
                        `<option value="${escHtml(c.id)}">${escHtml(c.name)}</option>`
                    ).join('');
            } else {
                connSelect.innerHTML = '<option value="">— no connections found —</option>';
            }
        })
        .catch(() => {
            roleSelect.innerHTML = '<option value="">— failed to load —</option>';
            connSelect.innerHTML = '<option value="">— failed to load —</option>';
            toastr.error('Could not load data. Please try again.');
        })
        .finally(() => {
            roleSelect.disabled = false;
            connSelect.disabled = false;
        });
}

function updateAssignPreview() {
    const roleSelect = document.getElementById('assign-role-select');
    const connSelect = document.getElementById('assign-conn-select');
    const preview = document.getElementById('assign-preview');
    const previewTxt = document.getElementById('assign-preview-text');

    if (roleSelect.value && connSelect.value) {
        const roleName = roleSelect.options[roleSelect.selectedIndex].text;
        const connName = connSelect.options[connSelect.selectedIndex].text;
        previewTxt.innerHTML =
            `<strong>${escHtml(roleName)}</strong> will get access to <strong>${escHtml(connName)}</strong>`;
        preview.style.display = 'flex';
    } else {
        preview.style.display = 'none';
    }
}

function assignRoleToDb() {
    const roleId = document.getElementById('assign-role-select').value;
    const connectionId = document.getElementById('assign-conn-select').value;

    if (!roleId || !connectionId) {
        toastr.warning('Please select both a role and a database connection.');
        return;
    }

    const btn = document.getElementById('btn-assign-db');
    btn.disabled = true;
    btn.innerHTML = '<span class="rm-spinner" style="width:16px;height:16px;border-width:2px;margin:0 4px 0 0;"></span> Assigning...';

    $.ajax({
        url: '/Admin/AssignRoleToConnection',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ roleId, connectionId }),
        success: function (res) {
            if (res.success) {
                toastr.success(res.message);
                // reset form
                document.getElementById('assign-role-select').value = '';
                document.getElementById('assign-conn-select').value = '';
                document.getElementById('assign-preview').style.display = 'none';
            } else {
                toastr.error(res.message);
            }
        },
        error: function () {
            toastr.error('Something went wrong. Please try again.');
        },
        complete: function () {
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-database-check"></i> Confirm Assignment';
        }
    });
}

// ─── Modal ─────────────────────────────────────────────────────────────────

function closeModal(id) {
    document.getElementById(id).style.display = 'none';
}

document.addEventListener('click', function (e) {
    ['modal-delete-role', 'modal-delete-user'].forEach(id => {
        const el = document.getElementById(id);
        if (e.target === el) closeModal(id);
    });
});

// ─── Helpers ───────────────────────────────────────────────────────────────

function showAlert(el, type, html) {
    el.className = 'rm-alert ' + type;
    el.innerHTML = html;
    el.style.display = 'flex';
}

function hideAlert(el) { el.style.display = 'none'; }

function getInitials(first, last) {
    return ((first ? first[0] : '') + (last ? last[0] : '')).toUpperCase() || '?';
}

function escHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

// ─── View / Edit Role DB Access ────────────────────────────────────────────

function loadViewAccessPanel() {
    const roleSelect = document.getElementById('view-role-select');
    roleSelect.innerHTML = '<option value="">— loading roles —</option>';
    roleSelect.disabled = true;

    document.getElementById('view-access-loading').style.display = 'none';
    document.getElementById('view-access-empty').style.display = 'none';
    document.getElementById('view-access-list').style.display = 'none';

    fetch('/Admin/GetRoles')
        .then(r => r.json())
        .then(res => {
            if (res.success && res.data?.length) {
                roleSelect.innerHTML = '<option value="">— select a role —</option>' +
                    res.data.map(r =>
                        `<option value="${escHtml(r.roleId)}">${escHtml(r.roleName)}</option>`
                    ).join('');
            } else {
                roleSelect.innerHTML = '<option value="">— no roles found —</option>';
            }
        })
        .catch(() => {
            roleSelect.innerHTML = '<option value="">— failed to load —</option>';
            toastr.error('Could not load roles. Please try again.');
        })
        .finally(() => { roleSelect.disabled = false; });
}

function loadRoleDbAccess() {
    const roleId = document.getElementById('view-role-select').value;

    document.getElementById('view-access-loading').style.display = 'none';
    document.getElementById('view-access-empty').style.display = 'none';
    document.getElementById('view-access-list').style.display = 'none';

    if (!roleId) return;

    document.getElementById('view-access-loading').style.display = 'block';

    fetch(`/Admin/GetRoleDbAccess?roleId=${encodeURIComponent(roleId)}`)
        .then(r => r.json())
        .then(res => {
            document.getElementById('view-access-loading').style.display = 'none';
            if (res.success && res.data?.length) {
                renderAccessItems(res.data);
            } else {
                document.getElementById('view-access-empty').style.display = 'block';
            }
        })
        .catch(() => {
            document.getElementById('view-access-loading').style.display = 'none';
            toastr.error('Could not load assignments. Please try again.');
        });
}

function renderAccessItems(items) {
    const container = document.getElementById('view-access-items');

    container.innerHTML = items.map(item => {
        const serverName = item.connection?.serverName || 'Unknown';
        const dbName = item.connection?.databaseName || 'Unknown';
        const connId = item.connectionId;
        const roleId = item.roleId;

        return `
        <div class="rm-access-row" id="access-row-${escHtml(item.id)}">
            <div class="rm-access-info">
                <div class="rm-access-db-icon">
                    <i class="bi bi-database"></i>
                </div>
                <div>
                    <div class="rm-access-db-name">${escHtml(serverName)} / ${escHtml(dbName)}</div>
                    <div class="rm-access-db-sub">Assigned connection</div>
                </div>
            </div>
            <div class="rm-access-edit">
                <select class="rm-input rm-access-select"
                        id="access-conn-select-${escHtml(item.id)}"
                        data-role-id="${escHtml(roleId)}"
                        data-row-id="${escHtml(item.id)}">
                    <option value="">— loading connections —</option>
                </select>
                <button class="rm-btn-primary rm-access-save-btn"
                        id="access-save-btn-${escHtml(item.id)}"
                        onclick="saveAccessChange('${escHtml(roleId)}', '${escHtml(item.id)}')">
                    <i class="bi bi-check-lg"></i> Save
                </button>
            </div>
        </div>`;
    }).join('');

    document.getElementById('view-access-list').style.display = 'block';

    // populate each connection dropdown
    fetch('/Admin/GetConnections')
        .then(r => r.json())
        .then(res => {
            if (!res.success || !res.data?.length) return;

            items.forEach(item => {
                const sel = document.getElementById(`access-conn-select-${item.id}`);
                if (!sel) return;
                sel.innerHTML = res.data.map(c =>
                    `<option value="${escHtml(c.id)}"
                        ${c.id === item.connectionId ? 'selected' : ''}>
                        ${escHtml(c.name)}
                    </option>`
                ).join('');
            });
        })
        .catch(() => toastr.error('Could not load connections.'));
}

function saveAccessChange(roleId, rowId) {
    const sel = document.getElementById(`access-conn-select-${rowId}`);
    const btn = document.getElementById(`access-save-btn-${rowId}`);
    const connectionId = sel?.value;

    if (!connectionId) {
        toastr.warning('Please select a connection first.');
        return;
    }

    btn.disabled = true;
    btn.innerHTML = '<span class="rm-spinner" style="width:14px;height:14px;border-width:2px;margin:0 4px 0 0;"></span> Saving...';

    $.ajax({
        url: '/Admin/AssignRoleToConnection',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ roleId, connectionId }),
        success: function (res) {
            if (res.success) {
                toastr.success('Database assignment updated successfully.');
            } else {
                toastr.error(res.message);
            }
        },
        error: function () {
            toastr.error('Something went wrong. Please try again.');
        },
        complete: function () {
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-check-lg"></i> Save';
        }
    });
}
// ─── Init ──────────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', function () {
    switchPanel('list');
});