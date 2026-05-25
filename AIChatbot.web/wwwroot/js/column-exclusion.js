// column-exclusion.js

// ─── State ─────────────────────────────────────────────────────────────────
const connections = JSON.parse(document.getElementById('connections-data').textContent || '[]');

let schemaData = [];   // flat list from API: [{ schemaName, tableName, columns[] }]
let exclusionsData = [];   // flat strings from API: ["dbo.Table", "dbo.Table.Column"]
let selectedConnId = null;
let selectedConnName = '';
let currentTable = null; // { schemaName, tableName, columns[] }

// pendingExclusions: { "schema.Table": Set<columnName> }
// empty Set = whole table excluded
let pendingExclusions = {};

// ─── Init ──────────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', function () {
    loadAllSummaries();
});

// ─── Summary View ──────────────────────────────────────────────────────────

async function loadAllSummaries() {
    showSummaryLoading();

    // fetch exclusions for every connection in parallel
    const results = await Promise.all(
        connections.map(c =>
            fetch(`/ColumnExclusion/GetExclusions?connectionId=${encodeURIComponent(c.id)}`)
                .then(r => r.json())
                .then(res => ({ conn: c, exclusions: res.success ? res.data : [] }))
                .catch(() => ({ conn: c, exclusions: [] }))
        )
    );

    // filter only connections that have exclusions
    const withExclusions = results.filter(r => r.exclusions.length > 0);
    renderSummary(withExclusions);
}

function renderSummary(results) {
    const list = document.getElementById('summary-list');
    const loading = document.getElementById('summary-loading');
    const empty = document.getElementById('summary-empty');

    loading.style.display = 'none';

    if (results.length === 0) {
        empty.style.display = 'block';
        updateMetrics(0, 0, 0);
        return;
    }

    // parse exclusion strings into rows
    const rows = [];
    results.forEach(({ conn, exclusions }) => {
        // group by schema.table
        const tableMap = {}; // key: "schema.Table" → { schema, table, columns[] | wholeTable }

        exclusions.forEach(ex => {
            const parts = ex.split('.');
            if (parts.length === 2) {
                // whole table: "schema.Table"
                const key = ex;
                tableMap[key] = tableMap[key] || { schema: parts[0], table: parts[1], columns: [], wholeTable: true };
                tableMap[key].wholeTable = true;
            } else if (parts.length === 3) {
                // column: "schema.Table.Column"
                const key = `${parts[0]}.${parts[1]}`;
                tableMap[key] = tableMap[key] || { schema: parts[0], table: parts[1], columns: [], wholeTable: false };
                if (!tableMap[key].wholeTable) tableMap[key].columns.push(parts[2]);
            }
        });

        Object.values(tableMap).forEach(t => {
            rows.push({ conn, schema: t.schema, table: t.table, columns: t.columns, wholeTable: t.wholeTable });
        });
    });

    // metrics
    const uniqueDbs = new Set(rows.map(r => r.conn.id)).size;
    const totalTables = rows.length;
    const totalColumns = rows.reduce((acc, r) => acc + (r.wholeTable ? 0 : r.columns.length), 0);
    updateMetrics(uniqueDbs, totalTables, totalColumns);

    list.innerHTML = rows.map(row => buildSummaryRow(row)).join('');
    empty.style.display = 'none';
}

function buildSummaryRow(row) {
    const colsHtml = row.wholeTable
        ? `<span class="ce-badge ce-badge-red"><i class="bi bi-slash-circle"></i> Whole table excluded</span>`
        : buildColumnBadges(row.columns);

    return `
        <div class="ce-summary-row">
            <div class="ce-col-db">
                <div class="ce-db-icon"><i class="bi bi-database"></i></div>
                <div>
                    <div class="ce-db-name">${escHtml(row.conn.name)}</div>
                    <div class="ce-db-sub">${escHtml(row.schema)}</div>
                </div>
            </div>
            <div class="ce-col-table">
                <span class="ce-badge ce-badge-blue">${escHtml(row.table)}</span>
            </div>
            <div class="ce-col-cols">${colsHtml}</div>
            <div class="ce-col-action">
                <button class="ce-btn-ghost"
                    onclick="openExclusionFlowEdit('${escHtml(row.conn.id)}')">
                    <i class="bi bi-pencil"></i> Edit
                </button>
            </div>
        </div>`;
}

function buildColumnBadges(columns) {
    const MAX_SHOW = 3;
    if (columns.length === 0) return '<span class="ce-badge ce-badge-gray">none</span>';

    const visible = columns.slice(0, MAX_SHOW);
    const extra = columns.length - MAX_SHOW;

    let html = visible.map(c => `<span class="ce-badge ce-badge-amber">${escHtml(c)}</span>`).join('');

    if (extra > 0) {
        const allBadges = columns.map(c => `<span class="ce-badge ce-badge-amber">${escHtml(c)}</span>`).join('');
        html += `
            <span class="ce-badge ce-badge-gray ce-expand-trigger"
                  onclick="toggleExpandCols(this)"
                  data-full='${escHtml(allBadges)}'
                  data-short='${escHtml(html)}'>
                +${extra} more
            </span>`;
    }
    return html;
}

function toggleExpandCols(el) {
    const full = el.dataset.full;
    const short = el.dataset.short;
    const isShort = el.textContent.includes('more');
    const parent = el.parentElement;

    if (isShort) {
        parent.innerHTML = full +
            `<span class="ce-badge ce-badge-gray ce-expand-trigger"
                   onclick="toggleExpandCols(this)"
                   data-full='${el.dataset.full}'
                   data-short='${el.dataset.short}'>
                Show less
            </span>`;
    } else {
        parent.innerHTML = short +
            `<span class="ce-badge ce-badge-gray ce-expand-trigger"
                   onclick="toggleExpandCols(this)"
                   data-full='${el.dataset.full}'
                   data-short='${el.dataset.short}'>
                +${(el.dataset.full.match(/ce-badge-amber/g) || []).length - 3} more
            </span>`;
    }
}

function updateMetrics(dbs, tables, columns) {
    document.getElementById('metric-dbs').textContent = dbs;
    document.getElementById('metric-tables').textContent = tables;
    document.getElementById('metric-columns').textContent = columns;
}

function showSummaryLoading() {
    document.getElementById('summary-loading').style.display = 'block';
    document.getElementById('summary-empty').style.display = 'none';
    document.getElementById('summary-list').innerHTML = '';
}

// ─── Flow: open / close ────────────────────────────────────────────────────

function openExclusionFlow() {
    pendingExclusions = {};
    document.getElementById('flow-conn-select').value = '';
    document.getElementById('ce-summary-card').style.display = 'none';
    document.getElementById('ce-flow-panel').style.display = 'block';
    goToStep1();
}

async function openExclusionFlowEdit(connId) {
    pendingExclusions = {};
    selectedConnId = connId;

    const connOption = connections.find(c => String(c.id) === String(connId));
    selectedConnName = connOption ? connOption.name : connId;

    document.getElementById('flow-conn-select').value = connId;
    document.getElementById('ce-summary-card').style.display = 'none';
    document.getElementById('ce-flow-panel').style.display = 'block';

    // load schema + existing exclusions then go to step 2
    await loadSchemaAndExclusions(connId);
    setStep(2);
}

function closeExclusionFlow() {
    document.getElementById('ce-flow-panel').style.display = 'none';
    document.getElementById('ce-summary-card').style.display = 'block';
    loadAllSummaries();
}

// ─── Flow: Step navigation ─────────────────────────────────────────────────

function goToStep1() { setStep(1); }

async function goToStep2() {
    const connId = document.getElementById('flow-conn-select').value;
    if (!connId) { toastr.warning('Please select a database connection.'); return; }

    selectedConnId = connId;
    const connOption = connections.find(c => String(c.id) === String(connId));
    selectedConnName = connOption ? connOption.name : connId;

    setStep(2);
    await loadSchemaAndExclusions(connId);
}

function goToStep3() {
    renderReview();
    setStep(3);
}

function setStep(n) {
    [1, 2, 3].forEach(i => {
        document.getElementById(`flow-step-${i}`).style.display = i === n ? 'block' : 'none';
        document.getElementById(`step-indicator-${i}`).classList.toggle('active', i <= n);
    });
}

// ─── Schema + Exclusions loading ───────────────────────────────────────────

async function loadSchemaAndExclusions(connId) {
    document.getElementById('step2-loading').style.display = 'block';
    document.getElementById('step2-content').style.display = 'none';
    document.getElementById('col-panel').style.display = 'none';
    currentTable = null;

    try {
        const [schemaRes, exclRes] = await Promise.all([
            fetch(`/ColumnExclusion/GetSchema?connectionId=${encodeURIComponent(connId)}`).then(r => r.json()),
            fetch(`/ColumnExclusion/GetExclusions?connectionId=${encodeURIComponent(connId)}`).then(r => r.json())
        ]);

        schemaData = schemaRes.success ? schemaRes.data : [];
        exclusionsData = exclRes.success ? exclRes.data : [];

        // pre-populate pendingExclusions from existing exclusions
        pendingExclusions = {};
        exclusionsData.forEach(ex => {
            const parts = ex.split('.');
            if (parts.length === 2) {
                // whole table
                const key = `${parts[0]}.${parts[1]}`;
                pendingExclusions[key] = new Set('__ALL__');
            } else if (parts.length === 3) {
                const key = `${parts[0]}.${parts[1]}`;
                if (!pendingExclusions[key]) pendingExclusions[key] = new Set();
                pendingExclusions[key].add(parts[2]);
            }
        });

        renderTableList();
        document.getElementById('step2-loading').style.display = 'none';
        document.getElementById('step2-content').style.display = 'block';
    } catch {
        document.getElementById('step2-loading').style.display = 'none';
        toastr.error('Failed to load schema. Please try again.');
    }
}

// ─── Table list rendering ──────────────────────────────────────────────────

function renderTableList() {
    const list = document.getElementById('table-list');

    // group by schema
    const grouped = {};
    schemaData.forEach(item => {
        if (!grouped[item.schemaName]) grouped[item.schemaName] = [];
        grouped[item.schemaName].push(item);
    });

    list.innerHTML = Object.entries(grouped).map(([schema, tables]) => `
        <div class="ce-schema-group">
            <div class="ce-schema-label">
                <i class="bi bi-diagram-3"></i> ${escHtml(schema)}
            </div>
            ${tables.map(t => {
        const key = `${t.schemaName}.${t.tableName}`;
        const excl = pendingExclusions[key];
        let badge = '';
        if (excl) {
            if (excl.has('__ALL__')) {
                badge = `<span class="ce-badge ce-badge-red" style="font-size:10px;">whole table</span>`;
            } else {
                badge = `<span class="ce-badge ce-badge-amber" style="font-size:10px;">${excl.size} col</span>`;
            }
        }
        return `
                <div class="ce-table-row" id="trow-${escHtml(key)}"
                     onclick="selectTable('${escHtml(t.schemaName)}', '${escHtml(t.tableName)}')">
                    <i class="bi bi-table"></i>
                    <span>${escHtml(t.tableName)}</span>
                    <span style="margin-left:auto">${badge}</span>
                </div>`;
    }).join('')}
        </div>
    `).join('');
}

// ─── Column panel ──────────────────────────────────────────────────────────

function selectTable(schemaName, tableName) {
    // deselect all
    document.querySelectorAll('.ce-table-row').forEach(r => r.classList.remove('selected'));
    const row = document.getElementById(`trow-${schemaName}.${tableName}`);
    if (row) row.classList.add('selected');

    currentTable = schemaData.find(
        s => s.schemaName === schemaName && s.tableName === tableName
    );
    if (!currentTable) return;

    renderColumnPanel(currentTable);
    document.getElementById('col-panel').style.display = 'flex';
}

function renderColumnPanel(table) {
    const key = `${table.schemaName}.${table.tableName}`;
    const excl = pendingExclusions[key] || new Set();
    const isAll = excl.has('__ALL__');

    document.getElementById('col-panel-title').textContent =
        `${table.schemaName}.${table.tableName}`;
    document.getElementById('chk-select-all').checked = isAll;

    const colList = document.getElementById('col-list');
    colList.innerHTML = table.columns.map(col => `
        <div class="ce-col-item">
            <input type="checkbox"
                   id="chk-col-${escHtml(col)}"
                   value="${escHtml(col)}"
                   ${isAll || excl.has(col) ? 'checked' : ''}
                   ${isAll ? 'disabled' : ''}
                   onchange="onColCheck()" />
            <label for="chk-col-${escHtml(col)}" class="ce-col-label">${escHtml(col)}</label>
            <span class="ce-col-type"></span>
        </div>
    `).join('');
}

function toggleSelectAll(checkbox) {
    const isAll = checkbox.checked;
    const inputs = document.querySelectorAll('#col-list input[type="checkbox"]');

    inputs.forEach(inp => {
        inp.checked = isAll;
        inp.disabled = isAll;
    });
}

function onColCheck() {
    const allInputs = document.querySelectorAll('#col-list input[type="checkbox"]');
    const allChecked = Array.from(allInputs).every(i => i.checked);
    document.getElementById('chk-select-all').checked = allChecked;
}

function saveCurrentTable() {
    if (!currentTable) return;

    const key = `${currentTable.schemaName}.${currentTable.tableName}`;
    const isAll = document.getElementById('chk-select-all').checked;

    if (isAll) {
        pendingExclusions[key] = new Set(['__ALL__']);
    } else {
        const checked = Array.from(
            document.querySelectorAll('#col-list input[type="checkbox"]:checked')
        ).map(i => i.value);

        if (checked.length === 0) {
            delete pendingExclusions[key];
        } else {
            pendingExclusions[key] = new Set(checked);
        }
    }

    // refresh badge on table row
    renderTableList();
    // re-select the table to keep panel open
    selectTable(currentTable.schemaName, currentTable.tableName);
    toastr.success(`Changes applied to ${currentTable.tableName}.`);
}

// ─── Review & Save ─────────────────────────────────────────────────────────

function renderReview() {
    const reviewList = document.getElementById('step3-list');
    const empty = document.getElementById('step3-empty');
    const entries = Object.entries(pendingExclusions);

    if (entries.length === 0) {
        reviewList.innerHTML = '';
        empty.style.display = 'block';
        return;
    }

    empty.style.display = 'none';
    reviewList.innerHTML = entries.map(([key, colSet]) => {
        const [schema, table] = key.split('.');
        const isAll = colSet.has('__ALL__');
        const colsHtml = isAll
            ? `<span class="ce-badge ce-badge-red"><i class="bi bi-slash-circle"></i> Whole table excluded</span>`
            : Array.from(colSet).map(c =>
                `<span class="ce-badge ce-badge-amber">${escHtml(c)}</span>`
            ).join('');

        return `
            <div class="ce-review-row">
                <div class="ce-review-left">
                    <span class="ce-badge ce-badge-blue">${escHtml(schema)}.${escHtml(table)}</span>
                </div>
                <div class="ce-review-cols">${colsHtml}</div>
                <button class="ce-btn-ghost" onclick="removeReviewEntry('${escHtml(key)}')">
                    <i class="bi bi-trash"></i>
                </button>
            </div>`;
    }).join('');
}

function removeReviewEntry(key) {
    delete pendingExclusions[key];
    renderReview();
}

async function saveAllExclusions() {
    const entries = Object.entries(pendingExclusions);

    if (entries.length === 0) {
        toastr.warning('No exclusions to save.');
        return;
    }

    // build the nested payload expected by the API
    // group by schemaName
    const schemaMap = {};
    entries.forEach(([key, colSet]) => {
        const [schema, table] = key.split('.');
        if (!schemaMap[schema]) schemaMap[schema] = {};

        if (colSet.has('__ALL__')) {
            // send all columns for whole-table exclusion
            const schemaItem = schemaData.find(
                s => s.schemaName === schema && s.tableName === table
            );
            schemaMap[schema][table] = schemaItem ? [...schemaItem.columns] : [];
        } else {
            schemaMap[schema][table] = Array.from(colSet);
        }
    });

    const payload = {
        connectionId: selectedConnId,
        exclusions: Object.entries(schemaMap).map(([schemaName, tables]) => ({
            schemaName,
            tables: Object.entries(tables).map(([tableName, columns]) => ({
                tableName,
                columns
            }))
        }))
    };

    const btn = document.getElementById('btn-save-exclusions');
    btn.disabled = true;
    btn.innerHTML = '<span class="ce-spinner-inline"></span> Saving...';

    try {
        const res = await fetch('/ColumnExclusion/SaveExclusions', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        const data = await res.json();

        if (data.success) {
            toastr.success(data.message);
            closeExclusionFlow();
        } else {
            toastr.error(data.message);
        }
    } catch {
        toastr.error('Network error. Please try again.');
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="bi bi-floppy"></i> Save exclusions';
    }
}

// ─── Helpers ───────────────────────────────────────────────────────────────

function escHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}