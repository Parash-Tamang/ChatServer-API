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

    // auto-open session from URL on refresh
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
        div.className = 'session-item d-flex justify-content-between align-items-center' +
            (s.id === currentSessionId ? ' active' : '');
        div.dataset.id = s.id;
        div.innerHTML = `
            <button class="btn btn-link session-title"
                    style="color:inherit;text-decoration:none;text-align:left;padding:8px;flex:1;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;"
                    onclick="openSession('${s.id}')">
                ${escHtml(title)}
            </button>
            <div class="dropdown">
                <i class="bi bi-three-dots-vertical" data-bs-toggle="dropdown" style="cursor:pointer;"></i>
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

            // update URL with new session
            const url = new URL(window.location);
            url.searchParams.set('sessionId', currentSessionId);
            window.history.pushState({}, '', url);
        }

        if (result.assistantReply) {
            removeTypingIndicator();
            setSendLoading(false);
            appendMessage('assistant', result.assistantReply, new Date().toISOString(), result.columns, result.rows);
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
                appendMessage('assistant', json.data.content, json.data.createdAt, json.data.columns, json.data.rows);
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
//  APPEND MESSAGE — renders markdown + table + excel btn
// ═══════════════════════════════════════════════════════
function appendMessage(role, content, isoTime, columns, rows) {
    const container = document.getElementById('messagesContainer');
    const isUser = role === 'user';
    const time = isoTime
        ? new Date(isoTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
        : '';

    const wrap = document.createElement('div');
    wrap.className = `d-flex flex-column ${isUser ? 'align-items-end' : 'align-items-start'}`;

    const bubble = document.createElement('div');
    bubble.className = `msg ${isUser ? 'user' : 'bot'}`;

    if (isUser) {
        // user messages — plain escaped text
        bubble.textContent = content;
    } else {
        // AI messages — render markdown
        bubble.classList.add('markdown-body');
        bubble.innerHTML = marked.parse(content || '');
    }

    const timeEl = document.createElement('div');
    timeEl.className = 'msg-time';
    timeEl.textContent = time;

    wrap.appendChild(bubble);

    // Excel download button — only if structured data exists
    if (!isUser && columns?.length > 0 && rows?.length > 0) {
        const btnWrap = document.createElement('div');
        btnWrap.className = 'mt-1';
        const btn = document.createElement('button');
        btn.className = 'btn btn-sm btn-outline-success';
        btn.innerHTML = '<i class="bi bi-file-earmark-excel"></i> Download Excel';
        btn.addEventListener('click', () => downloadExcel(columns, rows));
        btnWrap.appendChild(btn);
        wrap.appendChild(btnWrap);
    }

    wrap.appendChild(timeEl);
    container.appendChild(wrap);
}

// ═══════════════════════════════════════════════════════
//  EXCEL DOWNLOAD
// ═══════════════════════════════════════════════════════
function downloadExcel(columns, rows) {
    try {
        // Build header row from column names
        const header = columns.map(c => c.name);

        // Build data rows — convert each cell to a plain value
        const data = rows.map(row =>
            row.map(cell => {
                if (cell === null || cell === undefined) return '';
                // JsonElement comes as object from JSON — get its value
                if (typeof cell === 'object' && cell.toString) {
                    const str = String(cell);
                    // skip binary/gif data
                    if (str.length > 200) return '[binary data]';
                    return str;
                }
                return cell;
            })
        );

        // Create worksheet
        const wsData = [header, ...data];
        const ws = XLSX.utils.aoa_to_sheet(wsData);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, 'Data');

        // Download
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
    const el = document.createElement('div');
    el.id = 'typingIndicator';
    el.className = 'd-flex flex-column align-items-start';
    el.innerHTML = `<div class="msg bot p-0"><div class="typing-indicator"><span></span><span></span><span></span></div></div>`;
    container.appendChild(el);
    scrollToBottom();
}

function removeTypingIndicator() { document.getElementById('typingIndicator')?.remove(); }

function clearMessages() {
    document.getElementById('messagesContainer').innerHTML =
        '<div class="text-center mt-5 text-muted" id="emptyState" style="display:none;">' +
        '<h5>Start a conversation</h5><div>Ask anything. The AI is ready.</div></div>';
}

function showEmptyState() { const el = document.getElementById('emptyState'); if (el) el.style.display = ''; }
function hideEmptyState() { const el = document.getElementById('emptyState'); if (el) el.style.display = 'none'; }
function scrollToBottom() { const c = document.getElementById('messagesContainer'); c.scrollTop = c.scrollHeight; }

function setSendLoading(loading) {
    document.getElementById('sendBtn').classList.toggle('btn-send-loading', loading);
    document.getElementById('sendIcon').className = loading ? 'bi bi-hourglass-split' : 'bi bi-send';
}

function autoResizeTextarea(el) {
    el.style.height = 'auto';
    el.style.height = Math.min(el.scrollHeight, 160) + 'px';
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