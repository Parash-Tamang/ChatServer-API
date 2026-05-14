// role-management.js

let currentRoleId = null;
let currentRoleName = null;
let pendingDeleteRoleId = null;
let pendingDeleteUserId = null;
let allUsersCache = [];

// ─── Panel Switching ───────────────────────────────────────────────────────

function switchPanel(name) {
    ['create', 'list', 'users'].forEach(p => {
        document.getElementById('panel-' + p).style.display = 'none';
        document.getElementById('card-' + p)?.classList.remove('active');
    });

    document.getElementById('panel-' + name).style.display = 'block';

    if (name === 'create') document.getElementById('card-create').classList.add('active');
    if (name === 'list') {
        document.getElementById('card-list').classList.add('active');
        loadRoles();
    }
    if (name === 'users') document.getElementById('card-members').classList.add('active');
}

// ─── Load Roles ────────────────────────────────────────────────────────────

function loadRoles() {
    showRolesLoading();

    $.ajax({
        url: '/Admin/GetRoles',
        method: 'GET',
        success: function (res) {
            if (res.success && res.data) {
                renderRoles(res.data);
            } else {
                showRolesEmpty();
            }
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

    if (roles.length === 0) {
        showRolesEmpty();
        return;
    }

    grid.innerHTML = roles.map(role => `
        <div class="rm-role-card" onclick="openUsersPanel('${escHtml(role.roleId)}', '${escHtml(role.roleName)}')">
            <div class="rm-role-card-top">
                <span class="rm-role-name">${escHtml(role.roleName)}</span>
            </div>
            <div class="rm-role-meta">
                <i class="bi bi-people"></i> Click to view members
            </div>
            <div class="rm-role-card-actions">
                <button class="rm-btn-ghost"
                    onclick="event.stopPropagation(); openUsersPanel('${escHtml(role.roleId)}', '${escHtml(role.roleName)}')">
                    <i class="bi bi-people"></i> Members
                </button>
                <button class="rm-btn-danger"
                    onclick="event.stopPropagation(); openDeleteRoleModal('${escHtml(role.roleId)}', '${escHtml(role.roleName)}')">
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
            if (res.success) {
                toastr.success(res.message);
                loadRoles();
            } else {
                toastr.error(res.message);
            }
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
        data: { roleId: roleId },
        success: function (res) {
            if (res.success && res.data) {
                allUsersCache = res.data;
                renderUsers(res.data);
            } else {
                allUsersCache = [];
                showUsersEmpty();
            }
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

    if (users.length === 0) {
        showUsersEmpty();
        return;
    }

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
                // Remove from cache and re-render
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

// ─── Modal ─────────────────────────────────────────────────────────────────

function closeModal(id) {
    document.getElementById(id).style.display = 'none';
}

// Close modal on overlay click
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

function hideAlert(el) {
    el.style.display = 'none';
}

function getInitials(first, last) {
    const f = first ? first[0] : '';
    const l = last ? last[0] : '';
    return (f + l).toUpperCase() || '?';
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

// ─── Init ──────────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', function () {
    // Default to list panel and load roles
    switchPanel('list');
});