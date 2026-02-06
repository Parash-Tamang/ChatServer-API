/**
 * TalkToDB Chatbot Engine - Production Version
 * In-memory state management with API integration
 */
(function () {

    // ==================== STATE MANAGEMENT ====================
    let state = {
        user: { name: 'Explorer', details: '' },
        sessions: [],           // Array of {id, title}
        activeSessionId: null,
        messages: [],           // Current session messages
        searching: false
    };

    // ==================== INITIALIZATION ====================
    $(document).ready(() => {
        initializeApp();
        bindEventHandlers();
    });

    async function initializeApp() {
        refreshUserUI();
        await loadSessionHistory();
        resetWelcomeView();
    }

    // ==================== EVENT HANDLERS ====================
    function bindEventHandlers() {
        // Sidebar toggle
        $('#toggle-sidebar').on('click', () =>
            $('body').toggleClass('sidebar-collapsed')
        );

        // Profile dropdown
        $('#avatar-btn').on('click', (e) => {
            e.stopPropagation();
            $('#profile-dropdown').toggleClass('active');
        });

        $(window).on('click', () =>
            $('#profile-dropdown').removeClass('active')
        );

        // User settings
        $('#menu-personalize').on('click', openSettingsModal);
        $('#cancel-settings').on('click', () =>
            $('#user-modal').removeClass('active')
        );
        $('#save-settings').on('click', saveUserSettings);

        // Chat input
        $('#user-input')
            .on('input', handleInputChange)
            .on('keydown', handleInputKeydown);

        $('#btn-send').on('click', sendMessage);

        // New chat button
        $('#btn-new-chat').on('click', startNewChat);

        // Logout
        $('#menu-logout').on('click', handleLogout);
    }

    function handleInputChange() {
        this.style.height = 'auto';
        this.style.height = this.scrollHeight + 'px';
        $('#btn-send').prop('disabled', !$(this).val().trim() || state.searching);
    }

    function handleInputKeydown(e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    }

    // ==================== USER MANAGEMENT ====================
    function refreshUserUI() {
        $('#avatar-btn').text(state.user.name.charAt(0).toUpperCase());
    }

    function openSettingsModal() {
        $('#input-name').val(state.user.name);
        $('#input-details').val(state.user.details);
        $('#user-modal').addClass('active');
    }

    function saveUserSettings() {
        state.user = {
            name: $('#input-name').val().trim() || 'Explorer',
            details: $('#input-details').val().trim()
        };
        $('#user-modal').removeClass('active');
        refreshUserUI();

        // Update welcome message if on welcome screen
        if (!state.activeSessionId) {
            resetWelcomeView();
        }
    }

    function handleLogout() {
        $.ajax({
            url: `${window.env.API_BASE_URL}/Auth/logout`,
            type: "POST",
            xhrFields: { withCredentials: true }
        })
            .always(() => {
                window.location.replace("/Account/Login");
            });
    }

    // ==================== SESSION MANAGEMENT ====================
    async function loadSessionHistory() {
        try {
            const response = await $.ajax({
                url: `${window.env.API_BASE_URL}/data`,
                type: "GET",
                contentType: "application/json"
            });

            if (response.success && response.data?.sessionTitleList) {
                state.sessions = response.data.sessionTitleList.map(s => ({
                    id: s.sessionId,
                    title: s.sessionTitle
                }));
                renderSessionHistory();
            }
        } catch (error) {
            console.error('Failed to load session history:', error);
            showError('Failed to load chat history');
        }
    }

    function renderSessionHistory() {
        const $history = $('#history-container').empty();

        state.sessions.forEach(session => {
            const $item = $('<div class="nav-item">')
                .text(session.title)
                .toggleClass('active', session.id === state.activeSessionId)
                .on('click', () => loadSession(session.id));

            $history.append($item);
        });
    }

    async function loadSession(sessionId) {
        try {
            state.activeSessionId = sessionId;
            state.messages = [];

            renderSessionHistory();
            showLoadingState();

            const response = await $.ajax({
                url: `${window.env.API_BASE_URL}/message/${sessionId}`,
                type: "GET",
                contentType: "application/json"
            });

            console.log('API Response:', response); // Debug log

            if (response.success) {
                // Handle different response structures
                let messages = null;

                // Case 1: data is direct array
                if (Array.isArray(response.data)) {
                    messages = response.data;
                }
                // Case 2: data has messages property
                else if (response.data?.messages && Array.isArray(response.data.messages)) {
                    messages = response.data.messages;
                }
                // Case 3: data has messageList property
                else if (response.data?.messageList && Array.isArray(response.data.messageList)) {
                    messages = response.data.messageList;
                }
                // Case 4: data is object with other structure
                else {
                    console.warn('Unexpected data structure:', response.data);
                    messages = [];
                }

                if (messages && messages.length > 0) {
                    state.messages = groupMessages(messages);
                    renderMessages();
                } else {
                    // Empty session or no messages
                    state.messages = [];
                    $('#chat-viewport').html(`
                    <div class="welcome-hero">
                        <h2>Session loaded</h2>
                        <p>No messages yet in this conversation</p>
                    </div>
                `);
                }
            } else {
                throw new Error(response.message || 'Failed to load session');
            }

        } catch (error) {
            console.error('Failed to load session:', error);

            // Log the actual response for debugging
            if (error.responseJSON) {
                console.error('API Error Response:', error.responseJSON);
            }

            showError('Failed to load conversation');

            // Reset to welcome view after error
            setTimeout(() => {
                state.activeSessionId = null;
                resetWelcomeView();
                renderSessionHistory();
            }, 2000);
        }
    }

    function groupMessages(apiMessages) {
        // Safety check
        if (!Array.isArray(apiMessages)) {
            console.error('groupMessages expects an array, got:', typeof apiMessages);
            return [];
        }

        if (apiMessages.length === 0) {
            return [];
        }

        const grouped = [];
        let currentPair = {};

        apiMessages.forEach(msg => {
            // Ensure msg has required properties
            if (!msg || !msg.senderType || !msg.messageText) {
                console.warn('Invalid message format:', msg);
                return;
            }

            if (msg.senderType.toLowerCase() === "user") {
                // Save previous pair if exists
                if (currentPair.query) {
                    grouped.push({ ...currentPair });
                }
                // Start new pair
                currentPair = {
                    query: msg.messageText,
                    answer: null,
                    columns: null,
                    rows: null
                };
            }
            else if (msg.senderType.toLowerCase() === "assistant") {
                // Add assistant response to current pair
                currentPair.answer = msg.messageText;

                // Include any additional data if present
                if (msg.columns) currentPair.columns = msg.columns;
                if (msg.rows) currentPair.rows = msg.rows;
            }
        });

        // Add the last pair if it exists
        if (currentPair.query) {
            grouped.push(currentPair);
        }

        return grouped;
    }

  
    function startNewChat() {
        state.activeSessionId = null;
        state.messages = [];
        renderSessionHistory();
        resetWelcomeView();
        $('#user-input').focus();
    }
    // ==================== CHAT MESSAGING ====================
    async function sendMessage() {
        const messageText = $('#user-input').val().trim();

        if (!messageText || state.searching) return;

        // Clear input and disable send button
        $('#user-input').val('').css('height', 'auto');
        $('#btn-send').prop('disabled', true);
        state.searching = true;

        // Add user message immediately to UI
        const newMessage = { query: messageText, answer: null };
        state.messages.push(newMessage);

        // ✅ FIX: Always render, even without sessionId
        renderMessages(true); // Pass flag to force render

        try {
            const response = await $.ajax({
                url: `${window.env.API_BASE_URL}/chat`,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    sessionId: state.activeSessionId,
                    message: messageText,
      
                })
            });

            if (response.success && response.data) {
                handleChatResponse(response.data, newMessage);
            } else {
                throw new Error(response.message || 'Failed to send message');
            }

        } catch (error) {
            console.error('Chat error:', error);
            newMessage.answer = "Sorry, something went wrong. Please try again.";
            renderMessages(true);
        } finally {
            state.searching = false;
            $('#btn-send').prop('disabled', false);
        }
    }

    function handleChatResponse(data, messageObj) {
        // Update session ID if this was a new chat
        if (!state.activeSessionId && data.sessionId) {
            state.activeSessionId = data.sessionId;

            // Add new session to history
            const existingSession = state.sessions.find(s => s.id === data.sessionId);
            if (!existingSession) {
                state.sessions.unshift({
                    id: data.sessionId,
                    title: data.sessionTitle
                });
                renderSessionHistory();
            }
        }

        // Store response data
        messageObj.answer = data.assistantMessage.messageText;
        messageObj.columns = data.columns;
        messageObj.rows = data.rows;

        // Render with typing animation
        animateResponse(messageObj);
    }

    async function animateResponse(messageObj) {
        const fullText = messageObj.answer;
        const $lastAnswer = $('.interaction-block').last().find('.answer-body');

        $lastAnswer.empty();

        let currentText = "";
        const words = fullText.split(' ');

        for (let i = 0; i < words.length; i++) {
            currentText += (i > 0 ? ' ' : '') + words[i];
            $lastAnswer.html(parseMarkdown(currentText));
            scrollToBottom();
            await sleep(30);
        }

        // Add table if data exists
        if (messageObj.columns && messageObj.rows && messageObj.rows.length > 0) {
            const $table = buildDataTable(messageObj.columns, messageObj.rows);
            $('.interaction-block').last().append($table);
            scrollToBottom();
        }
    }

    // ==================== UI RENDERING ====================
    function renderMessages(forceRender = false) {
        // ✅ FIX: Allow rendering even without activeSessionId for new chats
        if (!forceRender && !state.activeSessionId) {
            return;
        }

        if (state.messages.length === 0) {
            if (!forceRender) {
                resetWelcomeView();
            }
            return;
        }

        const $viewport = $('#chat-viewport').empty();
        const $container = $('<div class="chat-container">');

        state.messages.forEach((msg, index) => {
            const isLast = index === state.messages.length - 1;

            const answerContent = msg.answer === null
                ? '<div class="loader"><div class="loader-dot"></div><div class="loader-dot"></div><div class="loader-dot"></div></div>'
                : parseMarkdown(msg.answer);

            const $block = $(`
            <div class="interaction-block ${isLast ? 'latest-interaction' : ''}">
                <div class="query-wrapper">
                    <div class="query-bubble">
                        <div class="query-text">${escapeHtml(msg.query)}</div>
                    </div>
                </div>
                <div class="answer-container">
                    <div class="answer-label">Answer</div>
                    <div class="answer-body">${answerContent}</div>
                </div>
            </div>
        `);

            $container.append($block);

            // Add table if exists and not the current loading message
            if (msg.columns && msg.rows && msg.rows.length > 0 && msg.answer !== null) {
                const $table = buildDataTable(msg.columns, msg.rows);
                $block.append($table);
            }
        });

        $viewport.append($container).append('<div class="bottom-anchor"></div>');
        scrollToBottom();
    }

    function buildDataTable(columns, rows) {
        const $wrapper = $('<div class="table-wrapper">');

        // Excel download button
        const $downloadBtn = $('<button class="excel-btn">')
            .html(`
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                    <polyline points="7 10 12 15 17 10"></polyline>
                    <line x1="12" y1="15" x2="12" y2="3"></line>
                </svg>
                Download Excel
            `)
            .on('click', () => exportToExcel(columns, rows));

        // Build table
        const $table = $('<table class="data-table">');

        // Header
        const $thead = $('<thead>');
        const $headerRow = $('<tr>');
        columns.forEach(col => {
            $headerRow.append(`<th>${escapeHtml(col.name)}</th>`);
        });
        $thead.append($headerRow);
        $table.append($thead);

        // Body
        const $tbody = $('<tbody>');
        rows.forEach(row => {
            const $tr = $('<tr>');
            row.forEach(cell => {
                $tr.append(`<td>${escapeHtml(String(cell))}</td>`);
            });
            $tbody.append($tr);
        });
        $table.append($tbody);

        $wrapper.append($downloadBtn).append($table);
        return $wrapper;
    }

    function exportToExcel(columns, rows) {
        if (typeof XLSX === 'undefined') {
            alert('Excel export library not loaded');
            return;
        }

        // Prepare data
        const headers = columns.map(col => col.name);
        const data = [headers, ...rows];

        // Create workbook
        const ws = XLSX.utils.aoa_to_sheet(data);
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, "Query Results");

        // Download
        const fileName = `TalkToDB_Export_${new Date().getTime()}.xlsx`;
        XLSX.writeFile(wb, fileName);
    }

    function resetWelcomeView() {
        $('#chat-viewport').html(`
            <div class="welcome-hero">
                <h1>Hello, ${state.user.name}</h1>
                <p>How can I help you today?</p>
            </div>
        `);
    }

    function showLoadingState() {
        $('#chat-viewport').html(`
            <div class="welcome-hero">
                <div class="loader"><div class="loader-dot"></div><div class="loader-dot"></div><div class="loader-dot"></div></div>
                <p>Loading conversation...</p>
            </div>
        `);
    }

    function showError(message) {
        $('#chat-viewport').html(`
            <div class="welcome-hero">
                <p style="color: #e74c3c;">${escapeHtml(message)}</p>
            </div>
        `);
    }

    // ==================== UTILITIES ====================
    function parseMarkdown(text) {
        if (!text) return "";

        // Use marked.js if available, otherwise basic parsing
        if (typeof marked !== 'undefined') {
            return marked.parse(text);
        }

        // Basic markdown parsing fallback
        let html = text
            .replace(/```([\s\S]*?)```/g, '<pre><code>$1</code></pre>')
            .replace(/`([^`]+)`/g, '<code>$1</code>')
            .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
            .replace(/\*([^*]+)\*/g, '<em>$1</em>');

        // Handle tables
        const lines = html.split('\n');
        let result = [];
        let inTable = false;

        for (let line of lines) {
            if (line.trim().startsWith('|')) {
                if (!inTable) {
                    result.push('<table>');
                    inTable = true;
                }
                if (line.includes('---')) continue;

                const cells = line.split('|').filter(c => c.trim());
                result.push(`<tr>${cells.map(c => `<td>${c.trim()}</td>`).join('')}</tr>`);
            } else {
                if (inTable) {
                    result.push('</table>');
                    inTable = false;
                }
                if (line.trim()) {
                    result.push(`<p>${line}</p>`);
                }
            }
        }

        if (inTable) result.push('</table>');

        return result.join('\n');
    }

    function escapeHtml(text) {
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return String(text).replace(/[&<>"']/g, m => map[m]);
    }

    function scrollToBottom() {
        const $viewport = $('#chat-viewport');
        $viewport.scrollTop($viewport[0].scrollHeight);
    }

    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

})();