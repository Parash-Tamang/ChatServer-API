document.addEventListener('DOMContentLoaded', function () {

    const dbList = document.getElementById('dbList');
    const functionPanel = document.getElementById('functionListPanel');
    const editorCard = document.getElementById('editorCard');
    const selectedDbName = document.getElementById('selectedDbName');
    const savePromptBtn = document.getElementById('savePromptBtn');
    const newFunctionBtn = document.getElementById('newFunctionBtn');

    let currentContext = null; // 'global' or 'local'
    let currentConnectionId = null;

    if (!dbList) return;

    // ─── Sidebar click ───────────────────────────────────────────
    dbList.addEventListener('click', function (e) {
        const item = e.target.closest('.db-item');
        if (!item) return;

        document.querySelectorAll('.db-item').forEach(i => i.classList.remove('active'));
        item.classList.add('active');

        const id = item.dataset.id;

        if (id === 'global') {
            selectedDbName.textContent = '— Global';
            currentContext = 'global';
            currentConnectionId = null;
            newFunctionBtn.classList.remove('d-none'); // show for global
            loadGlobalFunctions();
        } else {
            selectedDbName.textContent = `— ${item.dataset.name || ''}`;
            currentContext = 'local';
            currentConnectionId = id;
            newFunctionBtn.classList.add('d-none'); // hide for local
            loadConnectionFunctions(id);
        }

        editorCard.style.display = 'none';
    });

    // ─── Load global functions ────────────────────────────────────
    function loadGlobalFunctions() {
        showFunctionLoading();

        fetch('/Admin/GetGlobalFunctions')
            .then(res => { if (!res.ok) throw new Error('Failed to load functions.'); return res.json(); })
            .then(data => renderFunctionList(data))
            .catch(err => showFunctionError(err.message));
    }

    // ─── Load connection functions ────────────────────────────────
    function loadConnectionFunctions(connectionId) {
        showFunctionLoading();

        fetch(`/Admin/GetConnectionFunctions?connectionId=${connectionId}`)
            .then(res => { if (!res.ok) throw new Error('Failed to load functions.'); return res.json(); })
            .then(data => renderFunctionList(data))
            .catch(err => showFunctionError(err.message));
    }

    // ─── Render function list ─────────────────────────────────────
    function renderFunctionList(functions) {
        if (!functions.length) {
            functionPanel.innerHTML = `
                <div class="text-center text-muted py-3">
                    <i class="bi bi-inbox fs-3"></i>
                    <p class="mt-2 small">No functions found.</p>
                </div>`;
            return;
        }

        const list = document.createElement('ul');
        list.className = 'list-group list-group-flush';

        functions.forEach(fn => {
            const li = document.createElement('li');
            li.className = 'list-group-item list-group-item-action d-flex align-items-center gap-2';
            li.style.cursor = 'pointer';
            li.dataset.id = fn.id;
            li.innerHTML = `
                <i class="bi bi-braces text-primary small"></i>
                <span class="small fw-semibold">${fn.functionName}</span>`;

            li.addEventListener('click', () => loadFunctionById(fn.id, li));
            list.appendChild(li);
        });

        functionPanel.innerHTML = '';
        functionPanel.appendChild(list);
    }

    // ─── Load function by ID into editor ─────────────────────────
    function loadFunctionById(functionId, listItem) {
        document.querySelectorAll('#functionListPanel .list-group-item')
            .forEach(i => i.classList.remove('active'));
        listItem.classList.add('active');

        editorCard.style.display = 'block';
        document.getElementById('editorFunctionName').value = 'Loading...';
        document.getElementById('editorSystemPrompt').value = '';
        document.getElementById('editorFunctionId').value = functionId;
        document.getElementById('editorContext').value = currentContext;

        fetch(`/Admin/GetFunctionById?functionId=${functionId}`)
            .then(res => { if (!res.ok) throw new Error('Failed to load function.'); return res.json(); })
            .then(data => {
                document.getElementById('editorFunctionName').value = data.functionName || '';
                document.getElementById('editorSystemPrompt').value = data.systemPrompt || '';
            })
            .catch(err => {
                document.getElementById('editorFunctionName').value = 'Error';
                document.getElementById('editorSystemPrompt').value = err.message;
            });
    }
    //-new button event handler \

    newFunctionBtn.addEventListener('click', function () {
        // Deselect any active function
        document.querySelectorAll('#functionListPanel .list-group-item')
            .forEach(i => i.classList.remove('active'));

        // Clear and open editor with blank fields
        document.getElementById('editorFunctionId').value = '';
        document.getElementById('editorFunctionName').value = '';
        document.getElementById('editorSystemPrompt').value = '';
        document.getElementById('editorContext').value = 'global';

        editorCard.style.display = 'block';

        // Focus on function name so user can start typing
        document.getElementById('editorFunctionName').removeAttribute('readonly');
        document.getElementById('editorFunctionName').focus();
    });
    // ─── Save prompt ──────────────────────────────────────────────
    savePromptBtn.addEventListener('click', function () {
        const functionId = document.getElementById('editorFunctionId').value || null;
        const functionName = document.getElementById('editorFunctionName').value.trim();
        const systemPrompt = document.getElementById('editorSystemPrompt').value.trim();
        const context = document.getElementById('editorContext').value;

        if (!functionName) {
            toastr.warning('Function name is required.');
            return;
        }

        const isLocal = context === 'local';
        const url = isLocal ? '/Admin/SaveLocalPrompt' : '/Admin/SaveGlobalPrompt';

        const body = isLocal
            ? { connectionStringId: currentConnectionId, functionId: functionId, functionName, systemPrompt }
            : { functionId: functionId, functionName, systemPrompt };

        savePromptBtn.disabled = true;
        savePromptBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Saving...';

        fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        })
            .then(res => { if (!res.ok) return res.json().then(e => { throw new Error(e.message); }); return res.json(); })
            .then(data => {
                toastr.success(data.message || 'Prompt saved successfully.');
            })
            .catch(err => {
                toastr.error(err.message);
            })
            .finally(() => {
                savePromptBtn.disabled = false;
                savePromptBtn.innerHTML = '<i class="bi bi-save me-1"></i> Save Prompt';
            });
    });

    // ─── Helpers ──────────────────────────────────────────────────
    function showFunctionLoading() {
        functionPanel.innerHTML = `
            <div class="text-center text-muted py-3">
                <div class="spinner-border spinner-border-sm text-primary" role="status"></div>
                <p class="mt-2 small">Loading functions...</p>
            </div>`;
    }

    function showFunctionError(msg) {
        functionPanel.innerHTML = `
            <div class="alert alert-danger m-3">
                <i class="bi bi-exclamation-triangle me-2"></i>${msg}
            </div>`;
    }
});