// ═══════════════════════════════════════════════════════
//  MARKED CONFIG
// ═══════════════════════════════════════════════════════
marked.setOptions({ breaks: true, gfm: true });

// ═══════════════════════════════════════════════════════
//  HELPERS
// ═══════════════════════════════════════════════════════
function getAntiForgeryToken() {
    const el = document.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
}

function escHtml(str) {
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

// ═══════════════════════════════════════════════════════
//  TOAST HELPERS
// ═══════════════════════════════════════════════════════
function showError(msg) { toastr.error(msg); }
function showWarning(msg) { toastr.warning(msg); }
function showSuccess(msg) { toastr.success(msg); }

// ═══════════════════════════════════════════════════════
//  STATE
// ═══════════════════════════════════════════════════════
let currentSessionId = null;
let sessions = [];
let pollTimer = null;
let sentMessageTime = null;

// ═══════════════════════════════════════════════════════
//  BOOT
// ═══════════════════════════════════════════════════════
document.addEventListener('DOMContentLoaded', async () => {
    await loadSessions();
    bindEvents();
    const ta = document.getElementById('messageInput');
    ta.addEventListener('input', () => autoResizeTextarea(ta));
    autoResizeTextarea(ta);

    const params = new URLSearchParams(window.location.search);
    const sessionId = params.get('sessionId');
    if (sessionId) await openSession(sessionId);
});

function bindEvents() {
    document.getElementById('btnNewChat').addEventListener('click', startNewChat);
    document.getElementById('sendBtn').addEventListener('click', sendMessage);
    document.getElementById('messageInput').addEventListener('keydown', e => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });
}

// ═══════════════════════════════════════════════════════
//  SESSIONS
// ═══════════════════════════════════════════════════════
async function loadSessions() {
    try {
        const res = await fetch('/Chat/GetSessions');
        if (!res.ok) { showError('Oops! Could not load your chats. Please refresh.'); return; }
        const json = await res.json();
        if (json.success) {
            sessions = json.data ?? [];
            renderSessionList();
        } else {
            showError(json.message ?? 'Oops! Could not load your chats.');
            document.getElementById('sessionList').innerHTML =
                '<div class="text-muted p-2 text-center small">Could not load chats.</div>';
        }
    } catch {
        showError('Oops! Could not reach the server.');
        document.getElementById('sessionList').innerHTML =
            '<div class="text-muted p-2 text-center small">Could not reach server.</div>';
    }
}

function renderSessionList() {
    const list = document.getElementById('sessionList');
    if (sessions.length === 0) {
        list.innerHTML = '<div class="text-muted p-2 text-center small">No chats yet. Start a new one!</div>';
        return;
    }
    list.innerHTML = '';
    sessions.forEach(s => {
        const title = s.topicName || 'New Chat';
        const div = document.createElement('div');
        div.className = 'session-item' + (s.id === currentSessionId ? ' active' : '');
        div.dataset.id = s.id;
        div.innerHTML = `
            <button class="btn btn-link session-title"
                    style="color:inherit;text-decoration:none;text-align:left;padding:0;flex:1;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:13px;"
                    onclick="openSession('${s.id}')">
                ${escHtml(title)}
            </button>
            <div class="dropdown" onclick="event.stopPropagation()">
                <i class="bi bi-three-dots-vertical" data-bs-toggle="dropdown" style="cursor:pointer;font-size:13px;color:#999;"></i>
                <ul class="dropdown-menu dropdown-menu-end">
                    <li><button class="dropdown-item text-danger" onclick="deleteSession('${s.id}', event)">Delete</button></li>
                </ul>
            </div>`;
        list.appendChild(div);
    });
}

// ═══════════════════════════════════════════════════════
//  OPEN SESSION
// ═══════════════════════════════════════════════════════
async function openSession(sessionId) {
    stopPolling();
    currentSessionId = sessionId;

    const url = new URL(window.location);
    url.searchParams.set('sessionId', sessionId);
    window.history.pushState({}, '', url);

    renderSessionList();
    clearMessages();
    showTypingIndicator();

    try {
        const res = await fetch(`/Chat/GetMessages?sessionId=${sessionId}`);
        if (!res.ok) { removeTypingIndicator(); showError('Oops! Could not load this chat.'); return; }
        const json = await res.json();
        removeTypingIndicator();

        if (json.success && json.data?.length > 0) {
            hideEmptyState();
            json.data.forEach(m => appendMessage(m.role, m.content, m.createdAt, m.columns, m.rows));
        } else if (!json.success) {
            showError(json.message ?? 'Oops! Could not load messages.');
        } else {
            showEmptyState();
        }
    } catch {
        removeTypingIndicator();
        showError('Oops! Could not reach the server.');
    }
    scrollToBottom();
}

// ═══════════════════════════════════════════════════════
//  NEW CHAT
// ═══════════════════════════════════════════════════════
function startNewChat() {
    stopPolling();
    currentSessionId = null;
    const url = new URL(window.location);
    url.searchParams.delete('sessionId');
    window.history.pushState({}, '', url);
    clearMessages();
    showEmptyState();
    renderSessionList();

    document.getElementById('messageInput').focus();
}

// ═══════════════════════════════════════════════════════
//  SEND MESSAGE
// ═══════════════════════════════════════════════════════
async function sendMessage() {
    const input = document.getElementById('messageInput');
    const message = input.value.trim();
    if (!message) return;

    sentMessageTime = new Date().toISOString();
    hideEmptyState();
    appendMessage('user', message, sentMessageTime, null, null);
    input.value = '';
    autoResizeTextarea(input);
    setSendLoading(true);
    showTypingIndicator();
    scrollToBottom();

    try {
        const res = await fetch('/Chat/SendMessageAjax', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ message, chatSessionId: currentSessionId || null })
        });

        if (!res.ok) { removeTypingIndicator(); setSendLoading(false); showError('Oops! Server error. Please try again.'); return; }

        const json = await res.json();
        if (!json.success) { removeTypingIndicator(); setSendLoading(false); showError(json.message ?? 'Oops! Something went wrong.'); return; }

        const result = json.data;

        if (result.chatSessionId && result.chatSessionId != currentSessionId) {
            currentSessionId = result.chatSessionId;
            const topic = message.length > 40 ? message.substring(0, 40).trimEnd() + '…' : message;
            sessions.unshift({ id: currentSessionId, topicName: topic, createdAt: sentMessageTime });
            renderSessionList();

         

            const url = new URL(window.location);
            url.searchParams.set('sessionId', currentSessionId);
            window.history.pushState({}, '', url);
        }

        if (result.assistantReply) {
            removeTypingIndicator();
            setSendLoading(false);
            appendMessage(
                'assistant',
                result.assistantReply,
                new Date().toISOString(),
                result.columns,
                result.rows,
                result.excelGenerated,
                result.excelAvailableNow,
                result.graphImageBase64,
                result.graphImageUrl,
                result.graphTitle
            );
            scrollToBottom();
        } else {
            startPolling(currentSessionId, sentMessageTime);
        }
    } catch {
        removeTypingIndicator();
        setSendLoading(false);
        showError('Oops! Could not reach the server.');
    }
}

// ═══════════════════════════════════════════════════════
//  POLLING
// ═══════════════════════════════════════════════════════
function startPolling(sessionId, afterUtc) {
    stopPolling();
    let attempts = 0;
    const MAX = 40;

    pollTimer = setInterval(async () => {
        attempts++;
        try {
            const res = await fetch(`/Chat/PollReply?sessionId=${sessionId}&afterUtc=${encodeURIComponent(afterUtc)}`);
            if (!res.ok) return;
            const json = await res.json();
            if (json.success && json.found) {
                stopPolling();
                removeTypingIndicator();
                setSendLoading(false);
                appendMessage(
                    'assistant',
                    json.data.content,
                    json.data.createdAt,
                    json.data.columns,
                    json.data.rows,
                    json.data.excelGenerated,
                    json.data.excelAvailableNow,
                    json.data.graphImageBase64,
                    json.data.graphImageUrl,
                    json.data.graphTitle
                );
                scrollToBottom();
                return;
            }
        } catch { }

        if (attempts >= MAX) {
            stopPolling();
            removeTypingIndicator();
            setSendLoading(false);
            showError('Oops! The AI is taking too long. Please try again.');
        }
    }, 1500);
}

function stopPolling() {
    if (pollTimer) { clearInterval(pollTimer); pollTimer = null; }
}

// ═══════════════════════════════════════════════════════
//  DELETE SESSION
// ═══════════════════════════════════════════════════════
async function deleteSession(sessionId, event) {
    event?.stopPropagation();
    if (!confirm('Delete this chat?')) return;
    try {
        const res = await fetch('/Chat/DeleteSessionAjax', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgeryToken() },
            body: JSON.stringify({ sessionId })
        });
        if (!res.ok) { showError('Oops! Could not delete this chat.'); return; }
        const json = await res.json();
        if (json.success) {
            sessions = sessions.filter(s => s.id !== sessionId);
            renderSessionList();
            if (currentSessionId === sessionId) startNewChat();
            showSuccess('Chat deleted.');
        } else {
            showError(json.message ?? 'Oops! Could not delete this chat.');
        }
    } catch {
        showError('Oops! Could not reach the server.');
    }
}

// ═══════════════════════════════════════════════════════
//  APPEND MESSAGE — centered layout with avatar
// ═══════════════════════════════════════════════════════
function appendMessage(role, content, isoTime, columns, rows, excelGenerated, excelAvailableNow, graphImageBase64, graphImageUrl, graphTitle) {
    const container = document.getElementById('messagesContainer');
    const isUser = role === 'user';
    const time = isoTime
        ? new Date(isoTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
        : '';

    // Outer row
    const row = document.createElement('div');
    row.className = 'msg-row ' + (isUser ? 'user' : 'bot');

    // Column wrapper (bubble + time + attachments)
    const col = document.createElement('div');
    col.style.cssText = `display:flex;flex-direction:column;${isUser ? 'align-items:flex-end;margin-left:auto;' : 'flex:1;min-width:0;'}`;

    // Bubble
    const bubble = document.createElement('div');
    bubble.className = 'msg ' + (isUser ? 'user' : 'bot');

    if (isUser) {
        bubble.textContent = content;
    } else {
        bubble.classList.add('markdown-body');
        bubble.innerHTML = marked.parse(content || '');
    }

    col.appendChild(bubble);

    if (!isUser && (graphImageBase64 || graphImageUrl)) {
        const graphWrap = document.createElement('div');
        graphWrap.className = 'assistant-graph';

        const title = document.createElement('div');
        title.className = 'assistant-graph-title';
        title.textContent = graphTitle || 'Chart preview';
        graphWrap.appendChild(title);

        const img = document.createElement('img');
        img.className = 'graph-image';
        img.src = graphImageBase64
            ? `data:image/png;base64,${graphImageBase64}`
            : graphImageUrl;
        img.alt = graphTitle || 'Chart image';
        img.loading = 'lazy';
        graphWrap.appendChild(img);
        col.appendChild(graphWrap);
    }

    // Excel download for generated Excel payloads
    if (!isUser && excelGenerated) {
        const btnWrap = document.createElement('div');
        btnWrap.className = 'download-card';

        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn btn-sm btn-outline-success';
        btn.innerHTML = '<i class="bi bi-file-earmark-excel"></i> Download Excel';
        btn.addEventListener('click', () => {
            if (excelAvailableNow && excelAvailableNow.columns?.length > 0 && excelAvailableNow.rows?.length > 0) {
                downloadExcelAvailableNow(excelAvailableNow);
            } else if (columns?.length > 0 && rows?.length > 0) {
                downloadExcel(columns, rows);
            } else {
                showError('Excel output is not available yet.');
            }
        });

        btnWrap.appendChild(btn);

        if (excelAvailableNow?.format) {
            const hint = document.createElement('div');
            hint.className = 'download-hint';
            hint.textContent = `Excel ready (${excelAvailableNow.rowcount ?? excelAvailableNow.rows?.length ?? 0} rows)`;
            btnWrap.appendChild(hint);
        }

        col.appendChild(btnWrap);
    }

    // Table download button for inline columns/rows
    if (!isUser && columns?.length > 0 && rows?.length > 0) {
        const btnWrap = document.createElement('div');
        btnWrap.className = 'mt-1';
        const btn = document.createElement('button');
        btn.className = 'btn btn-sm btn-outline-success';
        btn.innerHTML = '<i class="bi bi-file-earmark-excel"></i> Download Table';
        btn.addEventListener('click', () => downloadExcel(columns, rows));
        btnWrap.appendChild(btn);
        col.appendChild(btnWrap);
    }

    // Timestamp
    const timeEl = document.createElement('div');
    timeEl.className = 'msg-time';
    timeEl.textContent = time;
    col.appendChild(timeEl);

    row.appendChild(col);
    container.appendChild(row);
}

// ═══════════════════════════════════════════════════════
//  EXCEL DOWNLOAD
// ═══════════════════════════════════════════════════════
function downloadExcel(columns, rows) {
    try {
        const header = columns.map(c => c.name);
        const data = rows.map(row =>
            row.map(cell => {
                if (cell === null || cell === undefined) return '';
                if (typeof cell === 'object' && cell.toString) {
                    const str = String(cell);
                    if (str.length > 200) return '[binary data]';
                    return str;
                }
                return cell;
            })
        );
        const wsData = [header, ...data];
        const ws = XLSX.utils.aoa_to_sheet(wsData);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, 'Data');
        const fileName = `export_${new Date().toISOString().slice(0, 10)}.xlsx`;
        XLSX.writeFile(wb, fileName);
        showSuccess('Excel file downloaded.');
    } catch {
        showError('Oops! Could not generate the Excel file.');
    }
}

function downloadExcelAvailableNow(excelAvailableNow) {
    try {
        const header = excelAvailableNow.columns || [];
        const rows = (excelAvailableNow.rows || []).map(row =>
            header.map(col => {
                const value = row?.[col];
                if (value === null || value === undefined) return '';
                if (typeof value === 'object') {
                    return JSON.stringify(value);
                }
                return value;
            })
        );

        const wsData = [header, ...rows];
        const ws = XLSX.utils.aoa_to_sheet(wsData);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, 'Excel');
        const fileName = `export_${new Date().toISOString().slice(0, 10)}.xlsx`;
        XLSX.writeFile(wb, fileName);
        showSuccess('Excel file downloaded.');
    } catch {
        showError('Oops! Could not generate the Excel file.');
    }
}

// ═══════════════════════════════════════════════════════
//  DOM HELPERS
// ═══════════════════════════════════════════════════════
function showTypingIndicator() {
    removeTypingIndicator();
    const container = document.getElementById('messagesContainer');
    const row = document.createElement('div');
    row.id = 'typingIndicator';
    row.className = 'msg-row bot';

    const indicator = document.createElement('div');
    indicator.className = 'typing-indicator';
    indicator.innerHTML = '<span></span><span></span><span></span>';

    row.appendChild(indicator);
    container.appendChild(row);
    scrollToBottom();
}

function removeTypingIndicator() { document.getElementById('typingIndicator')?.remove(); }

function clearMessages() {
    const container = document.getElementById('messagesContainer');
    container.innerHTML =
        `<div id="emptyState" style="display:none;flex-direction:column;align-items:center;justify-content:center;flex:1;gap:8px;padding:80px 20px;text-align:center;">
            <h5 style="font-size:22px;font-weight:500;color:#333;margin:0;">Start a conversation</h5>
            <div style="font-size:14px;color:#999;">Ask anything. The AI is ready.</div>
        </div>`;
}

function showEmptyState() {
    const el = document.getElementById('emptyState');
    if (el) el.style.display = 'flex';
}

function hideEmptyState() {
    const el = document.getElementById('emptyState');
    if (el) el.style.display = 'none';
}

function scrollToBottom() {
    const c = document.getElementById('messagesContainer');
    c.scrollTop = c.scrollHeight;
}

function setSendLoading(loading) {
    const btn = document.getElementById('sendBtn');
    btn.classList.toggle('btn-send-loading', loading);
    btn.classList.toggle('active', !loading);
    document.getElementById('sendIcon').style.opacity = loading ? '0.5' : '1';
}

function autoResizeTextarea(el) {
    el.style.height = 'auto';
    el.style.height = Math.min(el.scrollHeight, 200) + 'px';
    const btn = document.getElementById('sendBtn');
    btn.classList.toggle('active', el.value.trim().length > 0);
}

// ═══════════════════════════════════════════════════════
//  MODALS
// ═══════════════════════════════════════════════════════
async function loadDbModal() {
    try {
        const response = await fetch('/Chat/LoadConnectionModal');
        if (!response.ok) { showError('Access denied.'); return; }
        document.getElementById('modalContainer').innerHTML = await response.text();
        new bootstrap.Modal(document.getElementById('dbConnectModal')).show();
    } catch { showError('Oops! Could not load the database settings.'); }
}

async function loadRoleManagerModal() {
    try {
        const response = await fetch('/RoleManager/LoadRoleManagerModal');
        if (!response.ok) { showError('Access denied.'); return; }
        document.getElementById('modalContainer').innerHTML = await response.text();
        new bootstrap.Modal(document.getElementById('roleManagerModal')).show();
    } catch { showError('Oops! Could not load the role manager.'); }
}   