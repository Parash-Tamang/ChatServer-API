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
//  CONFIG
// ═══════════════════════════════════════════════════════
const apiBase = '/Chat';

// ═══════════════════════════════════════════════════════
//  STATE
// ═══════════════════════════════════════════════════════
let currentSessionId = null;
let sessions = [];
let sentMessageTime = null;
let pollTimer = null;
let imageModal = null;
let modalImage = null;
let modalScale = 1;
let modalBaseScale = 1;
let modalZoom = 1;
let modalOriginalSrc = null;
let modalOriginalWidth = 0;
let modalOriginalHeight = 0;
let pointerState = { active: false, points: new Map(), initialDistance: 0, initialZoom: 1 };

// ═══════════════════════════════════════════════════════
//  BOOT
// ═══════════════════════════════════════════════════════
document.addEventListener('DOMContentLoaded', async () => {
    await loadSessions();
    bindEvents();
    initImageModal();

    const ta = document.getElementById('messageInput');
    ta.addEventListener('input', () => autoResizeTextarea(ta));
    autoResizeTextarea(ta);

    const params = new URLSearchParams(window.location.search);
    const sessionId = params.get('sessionId');
    if (sessionId) {
        // Only auto-open if the session is present in the loaded session list.
        // This avoids calling GetMessages on reload for an invalid/unauthorized id.
        if (sessions && sessions.some(s => s.id === sessionId)) {
            await openSession(sessionId);
        } else {
            // Remove stale sessionId from query string to prevent repeated attempts
            params.delete('sessionId');
            const url = new URL(window.location);
            url.search = params.toString();
            window.history.replaceState({}, '', url);
        }
    }
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
        const res = await fetch(`${apiBase}/GetSessions`);
        if (!res.ok) { showError('Oops! Could not load your chats. Please refresh.'); return; }
        const json = await res.json();
        if (json.success) {
            sessions = json.data ?? [];
        } else {
            showError(json.message ?? 'Oops! Could not load your chats.');
            sessions = [];
        }
        renderSessionList();
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
    currentSessionId = sessionId;

    const url = new URL(window.location);
    url.searchParams.set('sessionId', sessionId);
    window.history.pushState({}, '', url);

    renderSessionList();
    clearMessages();
    showTypingIndicator();

    try {
        const res = await fetch(`${apiBase}/GetMessages?sessionId=${sessionId}`);
        if (!res.ok) { removeTypingIndicator(); showError('Oops! Could not load this chat.'); return; }
        const json = await res.json();
        removeTypingIndicator();

        if (json.success && Array.isArray(json.data) && json.data.length > 0) {
            hideEmptyState();
            json.data.forEach(m => appendMessage(
                m.role,
                m.content,
                m.id,
                m.createdAt,
                m.columns,
                m.rows,
                m.excelGenerated,
                m.excelAvailableNow,
                m.graphImageBase64,
                m.graphImageUrl,
                m.graphTitle
            ));
        } else if (json.success) {
            showEmptyState();
        } else {
            showError(json.message ?? 'Oops! Could not load this chat.');
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
    appendMessage('user', message, null, sentMessageTime, null, null);
    input.value = '';
    autoResizeTextarea(input);
    setSendLoading(true);
    showTypingIndicator();
    scrollToBottom();

    try {
        const res = await fetch(`${apiBase}/SendMessageAjax`, {
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

        if (result.assistantReply || result.graphImageBase64 || result.graphImageUrl) {
            removeTypingIndicator();
            setSendLoading(false);
            appendMessage(
                'assistant',
                result.assistantReply || '',
                result.messageId,
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
            removeTypingIndicator();
            setSendLoading(false);
            showWarning('Message sent. No assistant response arrived yet. The chat session is still active.');
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
    const MAX = 10;

    pollTimer = setInterval(async () => {
        attempts++;
        try {
            const res = await fetch(`${apiBase}/PollReply?sessionId=${sessionId}&afterUtc=${encodeURIComponent(afterUtc)}`);
            if (!res.ok) return;
            const json = await res.json();
            if (!json.success || !json.found) return;

            const reply = json.data;
            if (reply) {
                stopPolling();
                removeTypingIndicator();
                setSendLoading(false);
                appendMessage(
                    'assistant',
                    reply.content,
                    reply.id,
                    reply.createdAt,
                    reply.columns,
                    reply.rows,
                    reply.excelGenerated,
                    reply.excelAvailableNow,
                    reply.graphImageBase64,
                    reply.graphImageUrl,
                    reply.graphTitle
                );
                scrollToBottom();
                return;
            }

            if (reply) {
                stopPolling();
                removeTypingIndicator();
                setSendLoading(false);
                appendMessage(
                    'assistant',
                    reply.content,
                    reply.id,
                    reply.createdAt,
                    reply.columns,
                    reply.rows,
                    reply.excelGenerated,
                    reply.excelAvailableNow,
                    reply.graphImageBase64,
                    reply.graphImageUrl,
                    reply.graphTitle
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
        const res = await fetch(`${apiBase}/DeleteSessionAjax`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ sessionId })
        });
        if (!res.ok) { showError('Oops! Could not delete this chat.'); return; }
        const json = await res.json();
        if (!json.success) { showError(json.message ?? 'Oops! Could not delete this chat.'); return; }
        sessions = sessions.filter(s => s.id !== sessionId);
        renderSessionList();
        if (currentSessionId === sessionId) startNewChat();
        showSuccess('Chat deleted.');
    } catch {
        showError('Oops! Could not reach the server.');
    }
}

// ═══════════════════════════════════════════════════════
//  APPEND MESSAGE — centered layout with avatar
// ═══════════════════════════════════════════════════════
function appendMessage(role, content, messageId, isoTime, columns, rows, excelGenerated, excelAvailableNow, graphImageBase64, graphImageUrl, graphTitle) {
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
        const imageSrc = graphImageBase64
            ? `data:image/png;base64,${graphImageBase64}`
            : graphImageUrl;
        const imageNode = createChatImage(
            imageSrc,
            graphTitle || 'Generated image',
            messageId,
            graphTitle || 'Generated image'
        );
        col.appendChild(imageNode);
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
            if (messageId) {
                downloadExcelFromServer(messageId);
            } else if (excelAvailableNow && excelAvailableNow.columns?.length > 0 && excelAvailableNow.rows?.length > 0) {
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

function createChatImage(src, alt, messageId, title) {
    const wrapper = document.createElement('div');
    wrapper.className = 'assistant-image-container';
    wrapper.setAttribute('role', 'button');
    wrapper.setAttribute('tabindex', '0');
    wrapper.setAttribute('aria-label', `Open image preview: ${alt}`);

    const image = document.createElement('img');
    image.className = 'assistant-image';
    image.src = src;
    image.alt = alt;
    image.loading = 'lazy';
    image.decoding = 'async';
    image.addEventListener('click', () => openImageModal(src, alt, title));
    wrapper.appendChild(image);

    const downloadWrap = document.createElement('div');
    downloadWrap.className = 'assistant-image-download';
    downloadWrap.addEventListener('click', event => event.stopPropagation());

    const downloadBtn = document.createElement('button');
    downloadBtn.type = 'button';
    downloadBtn.className = 'image-download-button';
    downloadBtn.setAttribute('aria-label', 'Download full resolution image');
    downloadBtn.innerHTML = '<i class="bi bi-download" aria-hidden="true"></i>';
    downloadBtn.addEventListener('click', async event => {
        event.stopPropagation();
        await downloadImage(src, alt, messageId);
    });

    downloadWrap.appendChild(downloadBtn);
    wrapper.appendChild(downloadWrap);

    wrapper.addEventListener('keydown', event => {
        if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            openImageModal(src, alt, title);
        }
    });

    return wrapper;
}

function initImageModal() {
    imageModal = document.createElement('div');
    imageModal.className = 'image-modal-backdrop';
    imageModal.setAttribute('role', 'dialog');
    imageModal.setAttribute('aria-modal', 'true');
    imageModal.setAttribute('aria-label', 'Image preview');
    imageModal.innerHTML = `
        <div class="image-modal-content">
            <button class="image-modal-close" type="button" aria-label="Close image preview">
                <i class="bi bi-x-lg" aria-hidden="true"></i>
            </button>
            <div class="image-modal-inner">
                <img src="" alt="" />
            </div>
            <div class="image-modal-toolbar">
                <button class="image-modal-zoom-button" type="button" aria-label="Zoom out">-</button>
                <button class="image-modal-zoom-button" type="button" aria-label="Zoom in">+</button>
            </div>
        </div>
    `;

    document.body.appendChild(imageModal);
    modalImage = imageModal.querySelector('img');

    imageModal.addEventListener('click', event => {
        if (event.target === imageModal) {
            closeImageModal();
        }
    });

    const closeButton = imageModal.querySelector('.image-modal-close');
    const zoomButtons = Array.from(imageModal.querySelectorAll('.image-modal-zoom-button'));

    closeButton.addEventListener('click', closeImageModal);
    zoomButtons[0].addEventListener('click', () => setModalScale(modalZoom - 0.2));
    zoomButtons[1].addEventListener('click', () => setModalScale(modalZoom + 0.2));

    modalImage.addEventListener('click', event => {
        event.stopPropagation();
        if (modalZoom > 1) {
            setModalScale(1);
        } else {
            setModalScale(2);
        }
    });

    modalImage.addEventListener('load', () => {
        adjustModalBaseScale();
    });

    imageModal.addEventListener('wheel', event => {
        if (!imageModal.classList.contains('open')) return;
        event.preventDefault();
        const delta = event.deltaY < 0 ? 0.16 : -0.16;
        setModalScale(modalZoom + delta);
    }, { passive: false });

    document.addEventListener('keydown', event => {
        if (!imageModal.classList.contains('open')) return;
        if (event.key === 'Escape') {
            closeImageModal();
        }
    });
}

function openImageModal(src, alt, title) {
    if (!imageModal || !modalImage) return;
    modalOriginalSrc = src;
    modalZoom = 1;
    modalBaseScale = 1;
    modalOriginalWidth = 0;
    modalOriginalHeight = 0;
    modalImage.style.width = 'auto';
    modalImage.style.height = 'auto';
    modalImage.style.transform = 'scale(1)';
    modalImage.src = src;
    modalImage.alt = alt;
    imageModal.classList.add('open');
    document.body.style.overflow = 'hidden';
}

function closeImageModal() {
    if (!imageModal) return;
    modalZoom = 1;
    modalBaseScale = 1;
    modalImage.style.transform = 'scale(1)';
    imageModal.classList.remove('open');
    document.body.style.overflow = '';
    modalImage.src = '';
}

function adjustModalBaseScale() {
    if (!modalImage || !modalImage.naturalWidth || !modalImage.naturalHeight) return;
    modalOriginalWidth = modalImage.naturalWidth;
    modalOriginalHeight = modalImage.naturalHeight;

    const maxWidth = window.innerWidth * 0.85;
    const maxHeight = window.innerHeight * 0.85;
    const widthScale = maxWidth / modalOriginalWidth;
    const heightScale = maxHeight / modalOriginalHeight;
    modalBaseScale = Math.min(1, widthScale, heightScale);

    modalImage.style.width = `${Math.round(modalOriginalWidth * modalBaseScale)}px`;
    modalImage.style.height = `${Math.round(modalOriginalHeight * modalBaseScale)}px`;
    setModalScale(1);
}

function applyModalScale() {
    if (!modalImage) return;
    modalScale = Math.max(0.5, Math.min(3, modalZoom));
    modalImage.style.transform = `scale(${modalScale})`;
    modalImage.style.cursor = modalScale > 1 ? 'zoom-out' : 'zoom-in';
}

function setModalScale(value) {
    if (!modalImage) return;
    modalZoom = Math.max(0.5, Math.min(3, value));
    applyModalScale();
}

async function downloadImage(src, alt, messageId) {
    const filename = normalizeImageName(alt || `image-${messageId || Date.now()}`);
    try {
        if (src.startsWith('data:')) {
            const anchor = document.createElement('a');
            anchor.href = src;
            anchor.download = `${filename}.png`;
            document.body.appendChild(anchor);
            anchor.click();
            anchor.remove();
            return;
        }

        const response = await fetch(src, { mode: 'cors' });
        if (!response.ok) throw new Error('Image fetch failed');
        const blob = await response.blob();
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `${filename}${getExtensionFromMime(blob.type)}`;
        document.body.appendChild(anchor);
        anchor.click();
        anchor.remove();
        URL.revokeObjectURL(url);
    } catch {
        showError('Could not download the image.');
    }
}

function normalizeImageName(text) {
    return text
        .toLowerCase()
        .replace(/[^a-z0-9\-\_\.]/g, '-')
        .replace(/-+/g, '-')
        .replace(/^-|-$/g, '')
        .slice(0, 60) || 'chat-image';
}

function getExtensionFromMime(mime) {
    if (mime === 'image/jpeg') return '.jpg';
    if (mime === 'image/png') return '.png';
    if (mime === 'image/webp') return '.webp';
    return '.png';
}

// ═══════════════════════════════════════════════════════
//  EXCEL DOWNLOAD
// ═══════════════════════════════════════════════
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

async function downloadExcelFromServer(messageId) {
    try {
        const res = await fetch(`${apiBase}/GenerateExcelAjax`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ messageId })
        });

        if (!res.ok) {
            const errorText = await res.text();
            let messages = 'Could not download Excel file.';
            try {
                const json = JSON.parse(errorText);
                messages = json.message || messages;
            } catch {
                messages = errorText || messages;
            }
            showError(messages);
            return;
        }

        const blob = await res.blob();
        const disposition = res.headers.get('Content-Disposition');
        const fileName = getFileNameFromContentDisposition(disposition) || `Report_${new Date().toISOString().slice(0, 10)}.xlsx`;
        downloadBlob(blob, fileName);
        showSuccess('Excel file downloaded.');
    } catch {
        showError('Could not reach the server to generate Excel.');
    }
}

function getFileNameFromContentDisposition(contentDisposition) {
    if (!contentDisposition) return null;
    const parts = contentDisposition.split(';').map(part => part.trim());
    for (const part of parts) {
        if (part.startsWith('filename*=')) {
            const value = part.substring('filename*='.length);
            const parts2 = value.split("''");
            return decodeURIComponent(parts2[parts2.length - 1]).replace(/^"|"$/g, '');
        }
        if (part.startsWith('filename=')) {
            return part.substring('filename='.length).replace(/^"|"$/g, '');
        }
    }
    return null;
}

function downloadBlob(blob, filename) {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = filename;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
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