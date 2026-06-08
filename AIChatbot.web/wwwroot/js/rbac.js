// ════════════════════════════════════════════════════════
//  RBAC Permission Set Creator
// ════════════════════════════════════════════════════════

const SCHEMA = [
    { schemaName: "dbo", tableName: "BuildVersion", columns: ["SystemInformationID", "Database Version", "VersionDate", "ModifiedDate"] },
    { schemaName: "dbo", tableName: "ErrorLog", columns: ["ErrorLogID", "ErrorTime", "UserName", "ErrorNumber", "ErrorSeverity", "ErrorState", "ErrorProcedure", "ErrorLine", "ErrorMessage"] },
    { schemaName: "SalesLT", tableName: "Address", columns: ["AddressID", "AddressLine1", "AddressLine2", "City", "StateProvince", "CountryRegion", "PostalCode", "rowguid", "ModifiedDate"] },
    { schemaName: "SalesLT", tableName: "Customer", columns: ["CustomerID", "NameStyle", "Title", "FirstName", "MiddleName", "LastName", "Suffix", "CompanyName", "SalesPerson", "EmailAddress", "Phone", "PasswordHash", "PasswordSalt", "rowguid", "ModifiedDate"] },
    { schemaName: "SalesLT", tableName: "CustomerAddress", columns: ["CustomerID", "AddressID", "AddressType", "rowguid", "ModifiedDate"] },
    { schemaName: "SalesLT", tableName: "Product", columns: ["ProductID", "Name", "ProductNumber", "Color", "StandardCost", "ListPrice", "Size", "Weight", "ProductCategoryID", "ProductModelID", "SellStartDate", "SellEndDate", "DiscontinuedDate", "ThumbNailPhoto", "ThumbnailPhotoFileName", "rowguid", "ModifiedDate"] },
    { schemaName: "SalesLT", tableName: "ProductCategory", columns: ["ProductCategoryID", "ParentProductCategoryID", "Name", "rowguid", "ModifiedDate"] },
    { schemaName: "SalesLT", tableName: "ProductDescription", columns: ["ProductDescriptionID", "Description", "rowguid", "ModifiedDate"] },
    { schemaName: "SalesLT", tableName: "ProductModel", columns: ["ProductModelID", "Name", "CatalogDescription", "rowguid", "ModifiedDate"] },
    { schemaName: "SalesLT", tableName: "ProductModelProductDescription", columns: ["ProductModelID", "ProductDescriptionID", "Culture", "rowguid", "ModifiedDate"] },
    { schemaName: "SalesLT", tableName: "SalesOrderDetail", columns: ["SalesOrderID", "SalesOrderDetailID", "OrderQty", "ProductID", "UnitPrice", "UnitPriceDiscount", "LineTotal", "rowguid", "ModifiedDate"] },
    { schemaName: "SalesLT", tableName: "SalesOrderHeader", columns: ["SalesOrderID", "RevisionNumber", "OrderDate", "DueDate", "ShipDate", "Status", "OnlineOrderFlag", "SalesOrderNumber", "PurchaseOrderNumber", "AccountNumber", "CustomerID", "ShipToAddressID", "BillToAddressID", "ShipMethod", "CreditCardApprovalCode", "SubTotal", "TaxAmt", "Freight", "TotalDue", "Comment", "rowguid", "ModifiedDate"] }
];

const rbac = (() => {
    // State
    let state = {
        roleName: '',
        selectedSchema: null,
        permissions: {},
        schemaSearch: '',
        tableSearch: '',
        isDarkMode: false
    };

    // ════════════════════════════════════════════════════════
    //  Initialization
    // ════════════════════════════════════════════════════════
    function init() {
        loadDarkMode();
        bindEvents();
        renderSchemas();
        updatePayload();
    }

    // ════════════════════════════════════════════════════════
    //  Dark Mode
    // ════════════════════════════════════════════════════════
    function loadDarkMode() {
        const saved = localStorage.getItem('rbac-dark-mode');
        state.isDarkMode = saved === 'true';
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
        localStorage.setItem('rbac-dark-mode', state.isDarkMode);
        applyTheme();
    }

    // ════════════════════════════════════════════════════════
    //  Event Binding
    // ════════════════════════════════════════════════════════
    function bindEvents() {
        document.getElementById('btn-dark-mode').addEventListener('click', toggleDarkMode);
        document.getElementById('input-role-name').addEventListener('input', (e) => {
            state.roleName = e.target.value;
            updatePayload();
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
    function renderSchemas() {
        const schemas = Array.from(new Set(SCHEMA.map(t => t.schemaName)));
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
        state.selectedSchema = schema;
        state.tableSearch = '';
        document.getElementById('table-search').value = '';
        renderSchemas();
        renderTables();
    }

    // ════════════════════════════════════════════════════════
    //  Table Rendering
    // ════════════════════════════════════════════════════════
    function renderTables() {
        const filtered = SCHEMA.filter(t => 
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
    function renderPermissions() {
        const unsaved = Object.entries(state.permissions).filter(([_, p]) => !p.isSaved);
        const container = document.getElementById('permissions-list');
        const saveAllBtn = document.getElementById('btn-save-all');

        if (unsaved.length === 0) {
            container.innerHTML = `
                <div class="rbac-empty-state">
                    <i class="bi bi-inbox"></i>
                    <p>Select tables from Step 3 to configure permissions.</p>
                </div>
            `;
            saveAllBtn.style.display = 'none';
            return;
        }

        saveAllBtn.style.display = 'inline-flex';

        container.innerHTML = unsaved.map(([key, perm]) => {
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
                                <button class="rbac-btn-primary rbac-btn-small"
                                        onclick="rbac.saveTable('${key}')">
                                    <i class="bi bi-check"></i> Save
                                </button>
                                <button class="rbac-btn-secondary rbac-btn-small"
                                        onclick="rbac.removeTable('${key}')">
                                    <i class="bi bi-trash"></i> Remove
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }).join('');
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

    function sendPayload() {
        const payload = generatePayload();
        alert('Payload Review:\n\n' + JSON.stringify(payload, null, 2));
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
        const row = SCHEMA.find(x => x.schemaName === s && x.tableName === t);
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
        toggleTable,
        updatePermission,
        saveTable,
        saveAllTables,
        removeTable,
        addFilter,
        removeFilter,
        updateFilter
    };
})();
