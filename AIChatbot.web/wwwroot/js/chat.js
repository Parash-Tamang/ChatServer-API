// ================= STATE =================

let currentSessionId = null;
let lastUserMessages = {};
let lastUserMessageId = null;
let isTyping = false;
const STORAGE_KEY = "lastChatSession";

// ================= HELPERS: CHAT STATE =================

function setChatEmpty() {
    const chatArea = document.querySelector(".chat-area");
    chatArea.classList.remove("is-chatting");
    chatArea.classList.add("is-empty");

    const container = document.getElementById("messagesContainer");
    container.innerHTML = "";

    // Re-insert empty state above input
    const existing = document.getElementById("emptyState");
    if (!existing) {
        const empty = document.createElement("div");
        empty.className = "empty-state";
        empty.id = "emptyState";
        empty.innerHTML = "<h4>What can I help with?</h4>";
        chatArea.insertBefore(empty, document.getElementById("chatInputBar"));
    }
}

function setChatting() {
    const chatArea = document.querySelector(".chat-area");
    chatArea.classList.remove("is-empty");
    chatArea.classList.add("is-chatting");
    document.getElementById("emptyState")?.remove();
}

// ================= LOAD USER =================

async function loadUserDetails() {
    try {
        let res = await fetch("/Auth/UserDetails", { credentials: "include" });
        let data = await res.json();
        document.getElementById("userName").innerText = data.firstName + " " + data.lastName;
    } catch {
        document.getElementById("userName").innerText = "User";
    }
}

// ================= LOAD SESSIONS =================

async function loadSessions() {
    document.getElementById("historyList").innerHTML =
        "<div class='text-muted p-2'>Loading chats...</div>";

    try {
        let res = await fetch("/Chat/GetSessions");
        if (!res.ok) throw "fail";
        let data = await res.json();

        let html = "";
        data.forEach(s => {
            let topic = s.topicName || "New Chat";
            html += `
<div class="session-item d-flex justify-content-between align-items-center"
     onclick="openSession('${s.id}')">
    <div class="session-title">${topic}</div>
    <div class="dropdown" onclick="event.stopPropagation()">
        <i class="bi bi-three-dots-vertical"
           data-bs-toggle="dropdown"
           style="cursor:pointer"></i>
        <ul class="dropdown-menu dropdown-menu-end">
            <li>
                <button class="dropdown-item text-danger"
                        onclick="deleteSession('${s.id}')">
                    Delete
                </button>
            </li>
        </ul>
    </div>
</div>`;
        });

        document.getElementById("historyList").innerHTML = html;

    } catch {
        document.getElementById("historyList").innerHTML =
            "<div class='text-danger p-2'>Failed to load chats</div>";
    }
}

// ================= OPEN SESSION =================

async function openSession(id) {
    if (!id) return;

    localStorage.setItem(STORAGE_KEY, id);
    currentSessionId = id;

    document.getElementById("messagesContainer").innerHTML = "";
    setChatting();

    const res = await fetch("/Chat/GetMessages?id=" + id);
    const data = await res.json();

    if (!data || data.length === 0) {
        setChatEmpty();
        return;
    }

    data.forEach(m => {
        appendMessage(m.role, m.content);
        if (m.role === "user")
            lastUserMessages[id] = m.id;
    });

    scrollBottom();
}

// ================= SEND MESSAGE =================

async function sendMessage() {
    if (isTyping) return;

    const box = document.getElementById("messageInput");
    const text = box.value.trim();
    if (!text) return;

    appendMessage("user", text);
    showRetryButton();

    box.value = "";
    box.style.height = "auto";
    isTyping = true;
    box.disabled = true;

    appendTyping();

    const res = await fetch("/Chat/SendMessage", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            chatSessionId: currentSessionId,
            message: text
        })
    });

    const data = await res.json();

    removeTyping();
    isTyping = false;
    box.disabled = false;
    box.focus();

    currentSessionId = data.chatSessionId;
    lastUserMessageId = data.messageId;
    localStorage.setItem(STORAGE_KEY, currentSessionId);

    streamMessage(data.assistantReply || "No response from AIChatbot !! Please Retry");
    loadSessions();
}

// ================= STREAMING =================

function streamMessage(text) {
    const wrap = document.createElement("div");
    wrap.className = "d-flex flex-column align-items-start";

    const div = document.createElement("div");
    div.className = "msg bot";

    const time = document.createElement("div");
    time.className = "msg-time";
    time.innerText = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

    wrap.appendChild(div);
    wrap.appendChild(time);
    document.getElementById("messagesContainer").appendChild(wrap);

    let i = 0;
    const interval = setInterval(() => {
        div.innerText += text[i];
        i++;
        scrollBottom();
        if (i >= text.length) clearInterval(interval);
    }, 12);
}

// ================= RETRY =================

async function retryLast() {
    const messageId = lastUserMessages[currentSessionId];
    if (!currentSessionId || !messageId) return;

    appendTyping();

    const res = await fetch("/Chat/Retry", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            chatSessionId: currentSessionId,
            messageId: messageId
        })
    });

    const data = await res.json();
    removeTyping();
    streamMessage(data.assistantReply);
}

// ================= DELETE =================

async function deleteSession(id) {
    if (!confirm("Delete this chat?")) return;

    await fetch("/Chat/Delete/" + id, { method: "DELETE" });

    if (currentSessionId === id) {
        currentSessionId = null;
        setChatEmpty();
    }

    loadSessions();
}

// ================= UI HELPERS =================

function appendMessage(role, text) {
    // Switch to chatting mode — moves input to bottom
    setChatting();

    const wrap = document.createElement("div");
    wrap.className = "d-flex flex-column " +
        (role === "user" ? "align-items-end" : "align-items-start");

    const div = document.createElement("div");
    div.className = "msg " + (role === "user" ? "user" : "bot");
    div.innerText = text;

    const time = document.createElement("div");
    time.className = "msg-time";
    time.innerText = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

    wrap.appendChild(div);
    wrap.appendChild(time);

    document.getElementById("messagesContainer").appendChild(wrap);
    scrollBottom();
}

function appendTyping() {
    const wrap = document.createElement("div");
    wrap.id = "typing";
    wrap.className = "msg bot typing-dots";
    wrap.innerHTML = "<span>.</span><span>.</span><span>.</span>";
    document.getElementById("messagesContainer").appendChild(wrap);
    scrollBottom();
}

function removeTyping() {
    document.getElementById("typing")?.remove();
}

function scrollBottom() {
    const msg = document.getElementById("messagesContainer");
    msg.scrollTo({ top: msg.scrollHeight, behavior: "smooth" });
}

function newChat() {
    currentSessionId = null;
    localStorage.removeItem(STORAGE_KEY);
    setChatEmpty();
}

function showRetryButton() {
    const btn = document.createElement("button");
    btn.className = "retry-btn";
    btn.innerText = "Retry";
    btn.onclick = retryLast;

    const wrap = document.createElement("div");
    wrap.className = "d-flex justify-content-end";
    wrap.appendChild(btn);

    document.getElementById("messagesContainer").appendChild(wrap);
}

// ================= DOM READY =================

document.addEventListener("DOMContentLoaded", () => {
    // Set initial empty/centered state
    setChatEmpty();

    const messageInput = document.getElementById("messageInput");
    if (messageInput) {
        messageInput.addEventListener("keydown", function (e) {
            if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            }
        });

        messageInput.addEventListener("input", function () {
            this.style.height = "auto";
            this.style.height = this.scrollHeight + "px";
        });
    }

    loadUserDetails();
    loadSessions().then(() => {
        const last = localStorage.getItem(STORAGE_KEY);
        if (last) openSession(last);
    });
});