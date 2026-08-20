// ════════════════════════════════════════════════════════
//  RBAC Permission Set Creator
// ════════════════════════════════════════════════════════

const rbac = (() => {
    // State
    let state = {
        roleName: '',
        selectedSchema: null,
        permissions: {},
        savedPermissions: {},
        schemaSearch: '',
        tableSearch: '',
        connectionSearch: '',
        selectedConnectionId: null,
        connections: [],
        roleSearch: '',
        selectedRoleId: null,
        roles: [],
        roleLoading: false,
        schemas: [],
        schemaLoading: true,
        savedSectionCollapsed: false,
        isDarkMode: false
    };

    // ════════════════════════════════════════════════════════
    //  Initialization
    // ════════════════════════════════════════════════════════
    function init() {
        loadDarkMode();
        bindEvents();
        loadConnections();
        updatePayload();
    }

    // ════════════════════════════════════════════════════════
    //  Dark Mode
    // ════════════════════════════════════════════════════════
    function loadDarkMode() {
        // Do not read from localStorage; default to light mode unless toggled this session
        state.isDarkMode = false;
        applyTheme();
    }

    function applyTheme() {
        const html = document.documentElement;
        if (state.isDarkMode) {
            html.setAttribute('data-theme', 'dark');
        } else {
            html.removeAttribute('data-theme');
        }
    }

    function toggleDarkMode() {
        state.isDarkMode = !state.isDarkMode;
        applyTheme();
    }

    // ════════════════════════════════════════════════════════
    //  Event Binding
    // ════════════════════════════════════════════════════════
    function bindEvents() {
        document.getElementById('btn-dark-mode').addEventListener('click', toggleDarkMode);
        document.getElementById('connection-search').addEventListener('input', (e) => {
            state.connectionSearch = e.target.value;
            renderConnections();
        });
        document.getElementById('role-search').addEventListener('input', (e) => {
            state.roleSearch = e.target.value;
            renderRoles();
        });
        document.getElementById('schema-search').addEventListener('input', (e) => {
            state.schemaSearch = e.target.value;
            renderSchemas();
        });
        document.getElementById('table-search').addEventListener('input', (e) => {
            state.tableSearch = e.target.value;
            renderTables();
        });
        document.getElementById('btn-copy').addEventListener('click', copyPayload);
        document.getElementById('btn-send').addEventListener('click', sendPayload);
        document.getElementById('btn-copy-payload').addEventListener('click', copyPayload);
        document.getElementById('btn-save-all').addEventListener('click', saveAllTables);
    }

    // ════════════════════════════════════════════════════════
    //  Schema Rendering
    // ════════════════════════════════════════════════════════
    function renderConnections() {
        const filtered = state.connections.filter(c =>
            c.name.toLowerCase().includes(state.connectionSearch.toLowerCase())
        );

        const container = document.getElementById('connection-list');
        container.innerHTML = filtered.map(conn => `
            <button class="rbac-schema-item ${state.selectedConnectionId === conn.id ? 'active' : ''}"
                    data-connection-id="${conn.id}"
                    onclick="rbac.selectConnection('${conn.id}')">
                ${conn.name}
            </button>
        `).join('');

        if (filtered.length === 0) {
            container.innerHTML = '<span style="font-size: 0.875rem; color: var(--rbac-text-muted);">No databases found</span>';
        }
    }

    async function loadConnections() {
        try {
            const res = await fetch('/Admin/GetConnections');
            if (!res.ok) {
                throw new Error('Failed to load databases');
            }
            const json = await res.json();
            state.connections = Array.isArray(json.data) ? json.data : [];
            renderConnections();
            console.log('Connections loaded:', state.connections);
        } catch {
            state.connections = [];
            renderConnections();
        }
    }

    async function loadRoles(connectionId) {
        state.roles = [];
        state.roleSearch = '';
        state.selectedRoleId = null;
        state.roleLoading = true;
        renderRoles();

        if (!connectionId) {
            state.roleLoading = false;
            renderRoles();
            return;
        }

        try {
            const url = `/Admin/GetRolesByConnection?connectionId=${connectionId}`;
            console.log('Loading roles from:', url);
            const res = await fetch(url);
            console.log('Response status:', res.status, res.statusText);
            if (!res.ok) {
                const errorText = await res.text();
                console.error('API error response:', errorText);
                throw new Error(`Failed to load roles: ${res.status} ${res.statusText}`);
            }
            const json = await res.json();
            console.log('API response:', json);
            state.roles = Array.isArray(json.data) ? json.data : [];
            console.log('Roles loaded:', state.roles);
        } catch (err) {
            console.error('Error loading roles:', err);
            state.roles = [];
        } finally {
            state.roleLoading = false;
        }

        renderRoles();
    }

   async function loadSchemas(connectionId) {
    state.schemaLoading = true;
    renderSchemas(); // ← show "Loading schemas..." immediately

    try {
        const url = `/Admin/GetSchema?connectionId=${connectionId}`;
        const res = await fetch(url);
        if (!res.ok) {
            const errorText = await res.text();
            console.error('Schema API error:', errorText);
            throw new Error(`Failed to load schemas: ${res.status} ${res.statusText}`);
        }
        const json = await res.json();
        state.schemas = Array.isArray(json.data) ? json.data : [];
        console.log('Schemas loaded:', state.schemas.length, 'entries');
    } catch (err) {
        console.error('Error loading schemas:', err);
        state.schemas = [];
    } finally {
        state.schemaLoading = false;
        // Merge saved permissions after schema loads only if savedPermissions already present
        if (Object.keys(state.savedPermissions || {}).length > 0) {
            mergePermissions();
        } else {
            renderSchemas(); // ← now schemaLoading is false, renders actual data
        }
    }
}

    function loadSchemasForConnection() {
        if (!state.selectedConnectionId) {
            alert('Please select a database first (Step 0)');
            return;
        }
        // Require role selection before proceeding to Step 2
        if (!state.selectedRoleId) {
            alert('Please select a role first (Step 1)');
            return;
        }
        // Reset from Step 2 before reloading schema
        resetFromStep2();
        
        console.log('Loading schemas for connection:', state.selectedConnectionId);
        loadSchemas(state.selectedConnectionId);
    }

    function selectConnection(connectionId) {
    // clicking active connection deselects it
    if (state.selectedConnectionId === connectionId) {
        deselectConnection();
        return;
    }

    state.savedPermissions = {};
    // Reset roles and downstream state before switching databases
    resetFromStep1();
    state.selectedConnectionId = connectionId;
    // Ensure UI reflects cleared state
    renderConnections();
    renderRoles();
    renderSchemas();
    renderTables();
    renderPermissions();
    updatePayload();
    loadRoles(connectionId);
}

    // ========= Reset helper cascade =========
    function resetFromStep4() {
        state.permissions = {};
        renderPermissions();
        updatePayload();
    }

    function resetFromStep3() {
        state.tableSearch = '';
        const t = document.getElementById('table-search');
        if (t) t.value = '';
        resetFromStep4();
        renderTables();
        updateTableCount();
    }

    function resetFromStep2() {
        state.schemas = [];
        state.selectedSchema = null;
        state.schemaSearch = '';
        state.schemaLoading = false;
        resetFromStep3();
        renderSchemas();
        renderTables();
    }

    function resetFromStep1() {
        state.roles = [];
        state.selectedRoleId = null;
        state.roleSearch = '';
        state.savedPermissions = {};
        resetFromStep2();
        renderRoles();
    }

function deselectConnection() {
    state.selectedConnectionId = null;
    state.savedPermissions = {};
    // Reset everything starting from step 1
    resetFromStep1();
    renderConnections();
    updatePayload();
}

    function renderRoles() {
        const list = document.getElementById('role-list');
        if (!list) {
            console.warn('role-list element not found');
            return;
        }

        console.log('renderRoles called. selectedConnectionId:', state.selectedConnectionId, 'roleLoading:', state.roleLoading, 'roles count:', state.roles.length);

        if (!state.selectedConnectionId) {
            list.innerHTML = '<span style="font-size: 0.875rem; color: var(--rbac-text-muted);">Select a database first to view roles.</span>';
            return;
        }

        if (state.roleLoading) {
            list.innerHTML = '<span style="font-size: 0.875rem; color: var(--rbac-text-muted);">Loading roles...</span>';
            return;
        }

        const filtered = state.roles.filter(r =>
            r.roleName.toLowerCase().includes(state.roleSearch.toLowerCase()) ||
            r.roleId.toLowerCase().includes(state.roleSearch.toLowerCase())
        );

        console.log('Filtered roles:', filtered);

        list.innerHTML = filtered.map(role => `
            <button class="rbac-schema-item ${state.selectedRoleId === role.roleId ? 'active' : ''}"
                    data-role-id="${role.roleId}"
                    onclick="rbac.selectRole('${role.roleId}')">
                ${role.roleName}
            </button>
        `).join('');

        if (filtered.length === 0) {
            list.innerHTML = '<span style="font-size: 0.875rem; color: var(--rbac-text-muted);">No roles found for this database.</span>';
        }
    }

    function selectRole(roleId) {
        const role = state.roles.find(r => r.roleId === roleId);
        if (!role) return;

        state.selectedRoleId = roleId;
        state.roleName = role.roleName;
        state.savedPermissions = {};
        // Reset permissions area when selecting new role
        resetFromStep4();
        renderRoles();

        // Load saved permissions for this role/connection combination
        if (state.selectedConnectionId) {
            loadSavedPermissions(roleId, state.selectedConnectionId);
        }
    }

    async function loadSavedPermissions(roleId, connectionId) {
        try {
            const response = await fetch(`/Admin/GetRuntimePermissions?roleId=${encodeURIComponent(roleId)}&connectionId=${encodeURIComponent(connectionId)}`);
            if (!response.ok) {
                state.savedPermissions = {};
                return;
            }

            const json = await response.json();
            if (json?.data?.permissions && typeof json.data.permissions === 'object') {
                state.savedPermissions = mapRuntimePermissionsFromPayload(json.data.permissions);
            } else {
                const rows = Array.isArray(json.data) ? json.data : [];
                state.savedPermissions = mapRuntimePermissions(rows);
            }

            console.log('Loaded saved permissions:', state.savedPermissions);

            if (state.schemas && state.schemas.length > 0) {
                mergePermissions();
            }
        } catch (err) {
            console.error('Error loading saved permissions:', err);
            state.savedPermissions = {};
        }
    }

    function mapRuntimePermissionsFromPayload(payloadPermissions) {
        const result = {};

        Object.entries(payloadPermissions).forEach(([key, permission]) => {
            if (!permission || typeof permission !== 'object') {
                return;
            }

            result[key] = {
                reason: permission.reason ?? '',
                access_level: permission.access_level ?? 'unrestricted',
                required_filters: Array.isArray(permission.required_filters)
                    ? permission.required_filters.map(filter => ({
                        column: filter.column ?? '',
                        filter_type: filter.filter_type ?? 'id',
                        values: Array.isArray(filter.values)
                            ? filter.values.join(', ')
                            : filter.values ?? ''
                    }))
                    : [],
                isSaved: true,
                hasBeenSaved: true
            };
        });

        return result;
    }

    function mapRuntimePermissions(rows) {
        const result = {};

        rows.forEach(row => {
            if (!row || !row.schemaName || !row.tableName) {
                return;
            }

            const key = `${row.schemaName}.${row.tableName}`;
            if (!result[key]) {
                result[key] = {
                    reason: '',
                    access_level: 'unrestricted',
                    required_filters: [],
                    isSaved: true,
                    hasBeenSaved: true
                };
            }

            const filterType = row.filterType || '';
            if (filterType && filterType !== 'unrestricted') {
                result[key].access_level = 'filtered';
                result[key].required_filters.push({
                    column: row.columnName || '',
                    filter_type: filterType,
                    values: Array.isArray(row.values) ? row.values : (row.values ? [row.values] : [])
                });
            }
        });

        return result;
    }

    function mergePermissions() {
        // Merge saved permissions into current permissions
        Object.entries(state.savedPermissions).forEach(([key, savedPerm]) => {
            state.permissions[key] = {
                ...savedPerm,
                isSaved: true,
                hasBeenSaved: true
            };
        });
        console.log('Permissions merged. Current permissions:', state.permissions);
        renderSchemas();
        renderTables();
        renderPermissions();
        updatePayload();
    }
    function renderSchemas() {
        if (state.schemaLoading) {
            const container = document.getElementById('schema-list');
            container.innerHTML = '<span style="font-size: 0.875rem; color: var(--rbac-text-muted);">Loading schemas...</span>';
            return;
        }

        // Require role selection before showing schemas
        if (!state.selectedRoleId) {
            const container = document.getElementById('schema-list');
            container.innerHTML = '<span style="font-size: 0.875rem; color: var(--rbac-text-muted);">Select a role first (Step 1) to view schemas.</span>';
            return;
        }

        const schemas = Array.from(new Set(state.schemas.map(t => t.schemaName)));
        const filtered = schemas.filter(s => s.toLowerCase().includes(state.schemaSearch.toLowerCase()));

        const container = document.getElementById('schema-list');
        container.innerHTML = filtered.map(schema => `
            <button class="rbac-schema-item ${state.selectedSchema === schema ? 'active' : ''}"
                    data-schema="${schema}"
                    onclick="rbac.selectSchema('${schema}')">
                ${schema}
            </button>
        `).join('');

        if (filtered.length === 0) {
            container.innerHTML = '<span style="font-size: 0.875rem; color: var(--rbac-text-muted);">No schemas match</span>';
        }

        // Auto-select first schema if none selected
        if (!state.selectedSchema && filtered.length > 0) {
            selectSchema(filtered[0]);
        }
    }

    function selectSchema(schema) {
        if (!state.selectedRoleId) {
            alert('Please select a role first (Step 1)');
            return;
        }

        state.selectedSchema = schema;
        state.tableSearch = '';
        document.getElementById('table-search').value = '';
        // Reset UI: step 3 table list only
        // Keep permissions intact
        renderSchemas();
        renderTables();
    }

    // ════════════════════════════════════════════════════════
    //  Table Rendering
    // ════════════════════════════════════════════════════════
    function renderTables() {
        // Require role selection before showing tables
        if (!state.selectedRoleId) {
            const container = document.getElementById('tables-list');
            container.innerHTML = '<span style="font-size: 0.875rem; color: var(--rbac-text-muted);">Select a role first (Step 1) to view tables.</span>';
            updateTableCount();
            return;
        }
        const filtered = state.schemas.filter(t => 
            t.schemaName === state.selectedSchema && 
            t.tableName.toLowerCase().includes(state.tableSearch.toLowerCase())
        );

        const sorted = filtered.sort((a, b) => {
            const keyA = getTableKey(a.schemaName, a.tableName);
            const keyB = getTableKey(b.schemaName, b.tableName);
            const permA = state.permissions[keyA];
            const permB = state.permissions[keyB];

            let scoreA = 0;
            let scoreB = 0;

            if (permA) {
                scoreA = 1;
                if (permA.isSaved) scoreA = 2;
            }
            if (permB) {
                scoreB = 1;
                if (permB.isSaved) scoreB = 2;
            }

            if (scoreA !== scoreB) return scoreB - scoreA;
            return a.tableName.localeCompare(b.tableName);
        });

        const container = document.getElementById('tables-list');
        container.innerHTML = sorted.map(table => {
            const key = getTableKey(table.schemaName, table.tableName);
            const isSelected = !!state.permissions[key];
            return `
                <button class="rbac-table-item ${isSelected ? 'selected' : ''}"
                        data-key="${key}"
                        onclick="rbac.toggleTable('${key}', '${table.schemaName}', '${table.tableName}')">
                    ${table.tableName}
                </button>
            `;
        }).join('');

        if (sorted.length === 0) {
            container.innerHTML = '<span style="font-size: 0.875rem; color: var(--rbac-text-muted);">No tables match</span>';
        }

        updateTableCount();
    }

    function toggleTable(key, schema, table) {
        if (state.permissions[key]) {
            delete state.permissions[key];
        } else {
            state.permissions[key] = {
                reason: '',
                access_level: 'unrestricted',
                required_filters: [],
                isSaved: false,
                hasBeenSaved: false
            };
        }
        renderTables();
        renderPermissions();
        updatePayload();
    }

    function updateTableCount() {
        const count = Object.keys(state.permissions).length;
        document.getElementById('table-count-badge').textContent = `${count} selected`;
    }

    // ════════════════════════════════════════════════════════
    //  Permissions Rendering
    // ════════════════════════════════════════════════════════
    function renderPermissionCard(key, perm, isSavedCard) {
        const columns = getColumns(key);
        return `
            <div class="rbac-permission-card">
                <div class="rbac-permission-info">
                    <div class="rbac-permission-table">${key}</div>
                    <div class="rbac-permission-controls">
                        <!-- Reason -->
                        <div class="rbac-form-group">
                            <label class="rbac-label">Reason <span class="rbac-required">*</span></label>
                            <textarea placeholder="Why does this role need access?"
                                      rows="2"
                                      data-key="${key}"
                                      data-field="reason"
                                      onchange="rbac.updatePermission('${key}', 'reason', this.value)">
${perm.reason || ''}</textarea>
                        </div>

                        <!-- Access Level -->
                        <div class="rbac-form-group">
                            <label class="rbac-label">Access Level</label>
                            <div class="rbac-access-radio">
                                <label>
                                    <input type="radio" name="access-${key}" value="unrestricted"
                                           ${perm.access_level === 'unrestricted' ? 'checked' : ''}
                                           onchange="rbac.updatePermission('${key}', 'access_level', 'unrestricted')">
                                    Unrestricted
                                </label>
                                <label>
                                    <input type="radio" name="access-${key}" value="filtered"
                                           ${perm.access_level === 'filtered' ? 'checked' : ''}
                                           onchange="rbac.updatePermission('${key}', 'access_level', 'filtered')">
                                    Filtered
                                </label>
                            </div>
                        </div>

                        <!-- Filters (if needed) -->
                        ${perm.access_level === 'filtered' ? `
                            <div class="rbac-filters-section">
                                <div class="rbac-label" style="margin-bottom: 0.5rem;">Required Filters</div>
                                ${(perm.required_filters || []).map((f, i) => `
                                    <div class="rbac-filter-item">
                                        <select onchange="rbac.updateFilter('${key}', ${i}, 'column', this.value)">
                                            ${columns.map(c => `<option value="${c}" ${f.column === c ? 'selected' : ''}>${c}</option>`).join('')}
                                        </select>
                                        <select onchange="rbac.updateFilter('${key}', ${i}, 'filter_type', this.value)">
                                            <option value="id" ${f.filter_type === 'id' ? 'selected' : ''}>ID</option>
                                            <option value="range" ${f.filter_type === 'range' ? 'selected' : ''}>Range</option>
                                            <option value="like" ${f.filter_type === 'like' ? 'selected' : ''}>Like</option>
                                        </select>
                                        <input type="text" placeholder="Values (comma-separated)"
                                               value="${f.values || ''}"
                                               onchange="rbac.updateFilter('${key}', ${i}, 'values', this.value)">
                                        <button class="rbac-remove-filter-btn"
                                                onclick="rbac.removeFilter('${key}', ${i})">
                                            <i class="bi bi-trash"></i>
                                        </button>
                                    </div>
                                `).join('')}
                                <button class="rbac-add-filter-btn"
                                        onclick="rbac.addFilter('${key}')">
                                    <i class="bi bi-plus"></i> Add Filter
                                </button>
                            </div>
                        ` : ''}

                        <!-- Action Buttons -->
                        <div style="display: flex; gap: 0.5rem; margin-top: 0.5rem;">
                            ${isSavedCard ? `
                                <button class="rbac-btn-primary rbac-btn-small"
                                        onclick="rbac.editTable('${key}')">
                                    <i class="bi bi-pencil"></i> Edit
                                </button>
                            ` : `
                                <button class="rbac-btn-primary rbac-btn-small"
                                        onclick="rbac.saveTable('${key}')">
                                    <i class="bi bi-check"></i> Save
                                </button>
                            `}
                            <button class="rbac-btn-secondary rbac-btn-small"
                                    onclick="rbac.removeTable('${key}')">
                                <i class="bi bi-trash"></i> Remove
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    function renderPermissions() {
        const entries = Object.entries(state.permissions);
        const saved = entries.filter(([_, p]) => p.isSaved);
        const unsaved = entries.filter(([_, p]) => !p.isSaved);
        const container = document.getElementById('permissions-list');
        const saveAllBtn = document.getElementById('btn-save-all');

        if (saved.length === 0 && unsaved.length === 0) {
            container.innerHTML = `
                <div class="rbac-empty-state">
                    <i class="bi bi-inbox"></i>
                    <p>Select tables from Step 3 to configure permissions.</p>
                </div>
            `;
            saveAllBtn.style.display = 'none';
            return;
        }

        saveAllBtn.style.display = unsaved.length > 0 ? 'inline-flex' : 'none';

        const savedSectionHtml = saved.length > 0 ? `
            <div class="rbac-saved-section">
                <div class="rbac-saved-header">
                    <span>Previously Configured (${saved.length})</span>
                    <button class="rbac-collapse-btn" onclick="rbac.toggleSavedSection()">
                        ${state.savedSectionCollapsed ? 'Expand' : 'Collapse'}
                    </button>
                </div>
                ${state.savedSectionCollapsed ? '' : saved.map(([key, perm]) => renderPermissionCard(key, perm, true)).join('')}
            </div>
        ` : '';

        const unsavedSectionHtml = unsaved.map(([key, perm]) => renderPermissionCard(key, perm, false)).join('');

        container.innerHTML = savedSectionHtml + unsavedSectionHtml;
    }

    function toggleSavedSection() {
        state.savedSectionCollapsed = !state.savedSectionCollapsed;
        renderPermissions();
    }

    function updatePermission(key, field, value) {
        if (state.permissions[key]) {
            state.permissions[key][field] = value;
            if (field === 'access_level' && value !== 'filtered') {
                state.permissions[key].required_filters = [];
            }
            renderPermissions();
            updatePayload();
        }
    }

    function saveTable(key) {
        if (state.permissions[key]) {
            state.permissions[key].isSaved = true;
            state.permissions[key].hasBeenSaved = true;
            renderPermissions();
            updatePayload();
        }
    }

    function editTable(key) {
        if (state.permissions[key]) {
            state.permissions[key].isSaved = false;
            renderPermissions();
        }
    }

    function saveAllTables() {
        Object.keys(state.permissions).forEach(key => {
            if (!state.permissions[key].isSaved) {
                state.permissions[key].isSaved = true;
                state.permissions[key].hasBeenSaved = true;
            }
        });
        renderPermissions();
        updatePayload();
    }

    async function postRuntimePermissions() {
        if (!state.selectedRoleId || !state.selectedConnectionId) {
            alert('Select a role and connection before saving permissions.');
            return false;
        }

        const payload = generatePayload();
        try {
            const response = await fetch('/Admin/runtime-access/save', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const text = await response.text();
                console.error('Save runtime permissions failed:', response.status, text);
                alert('Failed to save runtime permissions.');
                return false;
            }

            const json = await response.json();
            if (!json.success) {
                console.error('Save runtime permissions error response:', json);
                alert('Failed to save runtime permissions.');
                return false;
            }

            showToast('Permissions saved successfully.');
            return true;
        } catch (err) {
            console.error('Error saving runtime permissions:', err);
            alert('Failed to save runtime permissions.');
            return false;
        }
    }

    function removeTable(key) {
        delete state.permissions[key];
        renderTables();
        renderPermissions();
        updatePayload();
    }

    function addFilter(key) {
        if (!state.permissions[key].required_filters) {
            state.permissions[key].required_filters = [];
        }
        const columns = getColumns(key);
        const usedColumns = state.permissions[key].required_filters.map(f => f.column);
        const available = columns.find(c => !usedColumns.includes(c));
        if (available) {
            state.permissions[key].required_filters.push({
                column: available,
                filter_type: 'id',
                values: ''
            });
            renderPermissions();
        }
    }

    function removeFilter(key, index) {
        state.permissions[key].required_filters.splice(index, 1);
        renderPermissions();
        updatePayload();
    }

    function updateFilter(key, index, field, value) {
        if (state.permissions[key].required_filters[index]) {
            state.permissions[key].required_filters[index][field] = value;
            updatePayload();
        }
    }

    // ════════════════════════════════════════════════════════
    //  Payload Generation
    // ════════════════════════════════════════════════════════
    function generatePayload() {
        const payload = {
            role: state.roleName.trim() || 'unnamed_role',
            selectedRoleId: state.selectedRoleId,
            database: state.connections.find(c => c.id === state.selectedConnectionId)?.name || null,
            connectionId: state.selectedConnectionId,
            permissions: {}
        };

        Object.entries(state.permissions).forEach(([key, perm]) => {
            if (!perm.hasBeenSaved) return;

            const processedFilters = (perm.required_filters || []).map(f => {
                let finalValues = f.values;
                if (typeof f.values === 'string') {
                    const parts = f.values.split(',').map(v => v.trim()).filter(Boolean);
                    finalValues = parts.length === 1 ? parts[0] : parts;
                    if (parts.length === 0) finalValues = '';
                }
                return { ...f, values: finalValues };
            });

            payload.permissions[key] = {
                reason: perm.reason,
                access_level: perm.access_level,
                required_filters: processedFilters
            };
        });

        return payload;
    }

    function updatePayload() {
        const payload = generatePayload();
        document.getElementById('payload-output').textContent = JSON.stringify(payload, null, 2);
    }

    // ════════════════════════════════════════════════════════
    //  Copy & Send
    // ════════════════════════════════════════════════════════
    function copyPayload() {
        const payload = generatePayload();
        const json = JSON.stringify(payload, null, 2);
        navigator.clipboard.writeText(json).then(() => {
            showToast('Copied to clipboard!');
        }).catch(() => {
            alert('Could not copy to clipboard');
        });
    }

    async function sendPayload() {
        const payload = generatePayload();
        const confirmed = confirm('Payload Review:\n\n' + JSON.stringify(payload, null, 2) + '\n\nSend this payload?');
        if (!confirmed) {
            return;
        }

        await postRuntimePermissions();
    }

    // ════════════════════════════════════════════════════════
    //  Utilities
    // ════════════════════════════════════════════════════════
    function getTableKey(schema, table) {
        return `${schema}.${table}`;
    }

    function getColumns(key) {
        const [s, ...rest] = key.split('.');
        const t = rest.join('.');
        const row = state.schemas.find(x => x.schemaName === s && x.tableName === t);
        return row ? row.columns : [];
    }

    function showToast(message) {
        const toast = document.createElement('div');
        toast.style.cssText = `
            position: fixed;
            bottom: 1rem;
            right: 1rem;
            background: var(--rbac-success);
            color: white;
            padding: 0.75rem 1.5rem;
            border-radius: 0.5rem;
            font-size: 0.875rem;
            z-index: 9999;
            animation: slideIn 0.2s ease-out;
        `;
        toast.textContent = message;
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 3000);
    }

    // ════════════════════════════════════════════════════════
    //  Public API
    // ════════════════════════════════════════════════════════
    return {
        init,
        selectSchema,
        selectConnection,
        selectRole,
        toggleTable,
        updatePermission,
        saveTable,
        editTable,
        saveAllTables,
        removeTable,
        addFilter,
        removeFilter,
        updateFilter,
        toggleSavedSection,
        loadSchemasForConnection
    };
})();
