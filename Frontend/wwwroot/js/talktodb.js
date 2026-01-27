/**
 * TalkToDB Engine - Simplified JavaScript Module
 * Wrapped in IIFE to avoid variable redeclaration
 */
(function () {
    
    let state = {
        user: JSON.parse(localStorage.getItem('tdb_user')) || { name: 'Explorer', details: '' },
        chats: JSON.parse(localStorage.getItem('tdb_chats')) || [],
        activeId: localStorage.getItem('tdb_activeId') || null,
        searching: false
    };

    $(document).ready(() => {
        initializeApp();
        bindEventHandlers();
    });

    function initializeApp() {
        refreshUserUI();
        if (state.activeId) {
            renderMessages();
        } else {
            resetView();
        }
        renderHistory();
    }

    function bindEventHandlers() {
        $('#toggle-sidebar').on('click', () => $('body').toggleClass('sidebar-collapsed'));

        $('#avatar-btn').on('click', (e) => {
            e.stopPropagation();
            $('#profile-dropdown').toggleClass('active');
        });

        $(window).on('click', () => $('.dropdown-menu').removeClass('active'));

        $('#user-input').on('input', handleInputChange)
            .on('keydown', handleInputKeydown);

        $('#btn-send').on('click', triggerSendMessage);
        $('#btn-new-chat').on('click', createNewChat);
        $('#menu-personalize').on('click', () => {
            $('#input-name').val(state.user.name);
            $('#input-details').val(state.user.details);
            $('#user-modal').addClass('active');
        });
        $('#cancel-settings').on('click', () => $('#user-modal').removeClass('active'));
        $('#save-settings').on('click', saveSettings);
        $('#menu-logout').on('click', handleLogout);
        $('#menu-test').on('click', testAPI);

    }

    function handleInputChange() {
        this.style.height = 'auto';
        this.style.height = this.scrollHeight + 'px';
        $('#btn-send').prop('disabled', !$(this).val().trim() || state.searching);
    }

    function handleInputKeydown(e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            triggerSendMessage();
        }
    }

    function refreshUserUI() {
        $('#avatar-btn').text(state.user.name.charAt(0).toUpperCase());
    }

    function saveSettings() {
        state.user = {
            name: $('#input-name').val() || 'Explorer',
            details: $('#input-details').val()
        };
        localStorage.setItem('tdb_user', JSON.stringify(state.user));
        $('#user-modal').removeClass('active');
        refreshUserUI();
        if (!state.activeId) {
            resetView();
        }
    }

 function handleLogout() {
        $.ajax({
            url: `${window.env.API_BASE_URL}/Auth/logout`,
            type: "POST",
            xhrFields: { withCredentials: true }
        })
            .done(() => {
                // Clear client state AFTER server logout
                localStorage.clear();
                AuthService.clear();

                // Redirect once everything is clean
                window.location.replace("/Account/Login");
            })
            .fail(() => {
                // Even if server fails, clear local state
                localStorage.clear();
                AuthService.clear();
                window.location.replace("/Account/Login");
            });
    }

    function testAPI() {
        $.ajax({
            url: `${window.env.API_BASE_URL}/Session/Create-Session`,
            type: "POST",
           
            success: function(data) {
            console.log(data);
        }
        })
    }

    function resetView() {
        state.activeId = null;
        localStorage.removeItem('tdb_activeId');
        $('#chat-viewport').html(`
            <div class="welcome-hero">
                <h1>Hello, ${state.user.name}</h1>
                <p>How can I help you today?</p>
            </div>
        `);
        $('.nav-item').removeClass('active');
    }

    function getData() {
        return $.ajax({
            url: `https://localhost:7009/api/chat/history`,
            type: "POST",
            contentType: "application/json",

            data: JSON.stringify({
                sessionId: "string",
                messageText: "string",
                senderType: "string"
            })
        });
    }

    function createNewChat() {
        resetView();
        renderHistory();
        $('#user-input').focus();
    }

    async function triggerSendMessage() {
        const val = $('#user-input').val().trim();
        if (!val || state.searching) return;

        if (!state.activeId) {
            state.activeId = Date.now().toString();
            state.chats.unshift({
                id: state.activeId,
                title: val.substring(0, 30),
                pairs: []
            });
        }

        const chat = state.chats.find(c => c.id === state.activeId);
        const pair = { query: val, answer: null };
        chat.pairs.push(pair);

        $('#user-input').val('').css('height', 'auto');
        $('#btn-send').prop('disabled', true);

        renderMessages();
        renderHistory();
        await fetchGeminiResponse(pair);
    }

   async function fetchGeminiResponse(pair) {
        state.searching = true;

        try {
            const context = `System: Use Markdown. User info: ${state.user.name}, ${state.user.details}`;
            const data = await getData();

            const fullText = data.content || "Error occurred";
          
            console.log("Result : "+fullText);
            // Targeted typing animation
            const $lastAnswerBody = $('.interaction-block').last().find('.answer-body');
            $lastAnswerBody.empty(); // Remove loader

            let currentText = "";
            const chunks = fullText.split(/(\s+)/);

            for (const chunk of chunks) {
                currentText += chunk;
                pair.answer = currentText;
                $lastAnswerBody.html(parseMarkdown(currentText));
                $('#chat-viewport').scrollTop($('#chat-viewport')[0].scrollHeight);
                await new Promise(r => setTimeout(r, chunk.trim() ? 25 : 5));
            }
            console.log(fullText);
            pair.answer = fullText;

        } catch (e) {
            pair.answer = "Service unavailable.";
            renderMessages();
        } finally {
            state.searching = false;
            persistState();
            $('#btn-send').prop('disabled', false);
        }
    }

    function renderMessages() {
        const chat = state.chats.find(c => c.id === state.activeId);
        if (!chat) return resetView();

        const $view = $('#chat-viewport').empty();
        const $cont = $('<div class="chat-container">');

        chat.pairs.forEach((p, idx) => {
            const isLast = idx === chat.pairs.length - 1;
            const answerContent = p.answer === null
                ? '<div class="loader"><div class="loader-dot"></div><div class="loader-dot"></div><div class="loader-dot"></div></div>'
                : parseMarkdown(p.answer);

            const $block = $(`
                <div class="interaction-block ${isLast ? 'latest-interaction' : ''}">
                    <div class="query-wrapper">
                        <div class="query-bubble">
                            <div class="query-text">${escapeHtml(p.query)}</div>
                        </div>
                    </div>
                    <div class="answer-container">
                        <div class="answer-label">Answer</div>
                        <div class="answer-body">${answerContent}</div>
                    </div>
                </div>
            `);
            $cont.append($block);
        });

        $view.append($cont).append('<div class="bottom-anchor"></div>');
        $view.scrollTop($view[0].scrollHeight);
    }
    function renderHistory() {
        const $h = $('#history-container').empty();
        state.chats.forEach(c => {
            const $item = $('<div class="nav-item">')
                .text(c.title)
                .toggleClass('active', c.id === state.activeId);

            $item.on('click', () => {
                state.activeId = c.id;
                persistState();
                renderMessages();
                renderHistory();
            });

            $h.append($item);
        });
    }

    function parseMarkdown(text) {
        if (!text) return "";

        let html = text
            .replace(/``````/g, '<pre><code>$1</code></pre>')
            .replace(/`([^`]+)`/g, '<code>$1</code>');

        const lines = html.split('\n');
        let inTable = false;
        let processed = [];

        for (let l of lines) {
            if (l.trim().startsWith('|')) {
                if (!inTable) {
                    processed.push('<table>');
                    inTable = true;
                }
                if (l.includes('---')) continue;

                const cells = l.split('|').filter(c => c.trim() !== "");
                processed.push(`<tr>${cells.map(c => `<td>${c.trim()}</td>`).join('')}</tr>`);
            } else {
                if (inTable) {
                    processed.push('</table>');
                    inTable = false;
                }
                if (l.trim().match(/^[-*]\s/)) {
                    processed.push(`<ul><li>${l.trim().substring(2)}</li></ul>`);
                } else if (l.trim()) {
                    processed.push(`<p>${l}</p>`);
                }
            }
        }

        if (inTable) processed.push('</table>');

        return processed.join('\n').replace(/<\/ul>\n<ul>/g, '');
    }

    function escapeHtml(text) {
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, m => map[m]);
    }

    function persistState() {
        localStorage.setItem('tdb_chats', JSON.stringify(state.chats));
        localStorage.setItem('tdb_activeId', state.activeId);
    }

})();
