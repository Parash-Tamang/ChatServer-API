const textarea = document.getElementById("userInput");
const wrapper = document.getElementById("chatWrapper");
const sendBtn = document.getElementById("send-btn");
const newChatBtn = document.getElementById("newChatBtn");
const canvas = document.createElement("canvas");
const message = document.getElementById("msg");
const ctx = canvas.getContext("2d");
let availableWidth = null;
let prevWidth = null;
let isBotResponding = false;
let tokenExceed = false;
const maxToken = 4096;
async function botResponse(message) {
    try {
        const res = await fetch("/Chat/SendMessage", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ message })
        });
        if (!res.ok) {
            if (res.status === 429) return "Rate limit exceeded. Please wait a moment.";
            if (res.status === 500) return "Server error. Please try again later.";
            if (res.status === 404) return "Service unavailable. Please check your connection.";

            throw new Error(`HTTP error! Status: ${res.status}`);
        }
        console.log("Status:", res.status, "OK:", res.ok);
        const data = await res.json();
        return data.choices?.[0]?.message?.content || "I couldn't generate a response. Please try again.";
    }
    catch (error) {

        if (error.name === 'AbortError') {
            return "Request timed out. Please try again.";
        }
        if (!navigator.onLine) {
            return "No internet connection. Please check your network.";
        }
        console.error("Error sending message:", error);
        return "Something went wrong. Please try again.";
    }
}
function getMessageBubble(isUser, textMessage) {
    if (!textMessage) return;
    const msgBubble = document.createElement("div");
    msgBubble.classList.add(isUser ? "userMsg" : "botMsg");
    if (isUser) {
        msgBubble.textContent = textMessage;
    } else {
        msgBubble.innerHTML = DOMPurify.sanitize(marked.parse(textMessage));
    }
    message.appendChild(msgBubble);
    //const children = message.children;
    //if (children.length > 1) {
    //    const secondLast = children[children.length - 2];
    //    secondLast.scrollIntoView({ behavior: "smooth", block: "start" });
    //}
    message.lastElementChild.scrollIntoView({ behavior: "smooth", block: "start" });
}
function showTypingIndicator() {
    const typingDiv = document.createElement("div");
    typingDiv.classList.add("botMsg", "typing-indicator");
    typingDiv.id = "bot-typing";

    typingDiv.innerHTML = `
       <span>Thinking</span>
        <span class="typing-dots">
            <span>.</span>
            <span>.</span>
            <span>.</span>
        </span>
    `;
    message.appendChild(typingDiv);
    typingDiv.scrollIntoView({ behavior: "smooth", block: "start" });
}

function removeTypingIndicator() {
    const typingDiv = document.getElementById("bot-typing");
    if (typingDiv) typingDiv.remove();
}

function getTextWidth(text, font) {
    ctx.font = font;
    return ctx.measureText(text).width;
}

function adjustTextarea() {

    if (textarea.value.length > maxToken) {
        tokenExceed = true;
    }
    else {
        tokenExceed = false;
    }
    const style = getComputedStyle(textarea);
    const font = `${style.fontSize} ${style.fontFamily}`;
    const textWidth = getTextWidth(textarea.value, font);
    availableWidth = textarea.clientWidth -
        parseFloat(style.paddingLeft) -
        parseFloat(style.paddingRight);

    if (textarea.value.trim() === "") {
        updateButtonState();
        wrapper.classList.remove("column-layout");
        textarea.style.height = "36px";
    } else {
        updateButtonState();
        const lineCount = textarea.value.split("\n").length;
        if (textWidth >= availableWidth || lineCount > 1) {
            wrapper.classList.add("column-layout");
            textarea.style.height = textarea.scrollHeight + "px";
        }
    }
}
function updateButtonState() {
    if (textarea.value.trim() === "" || isBotResponding || tokenExceed) {
        sendBtn.disabled = true;
        sendBtn.style.backgroundColor = "gray";
    } else {
        sendBtn.disabled = false;
        sendBtn.style.backgroundColor = "black";
    }
}

textarea.addEventListener("input", adjustTextarea);
window.addEventListener("load", adjustTextarea);

sendBtn.addEventListener("click", async function () {
    const text = textarea.value.trim();
    if (!text || isBotResponding) return;

    isBotResponding = true;
    updateButtonState();
    textarea.value = "";

    getMessageBubble(true, text);
    adjustTextarea();
    showTypingIndicator();

    try {

        const reply = await botResponse(text);
        await new Promise(resolve => setTimeout(resolve, 500));
        removeTypingIndicator();
        getMessageBubble(false, reply);

    } finally {

        isBotResponding = false;
        updateButtonState();
        adjustTextarea();
    }
});

textarea.addEventListener("keydown", function (e) {
    if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        sendBtn.click();
    }
});

newChatBtn.addEventListener("click", () => {
    message.innerHTML = "";
    textarea.value = "";
    adjustTextarea();

    textarea.focus();

    textarea.placeholder = "New Chat Started";
    //const welcome = document.createElement("div");
    //welcome.classList.add("botMsg");
    //welcome.textContent = "New chat started!";
    //message.appendChild(welcome);
    //message.lastElementChild.scrollIntoView({ behavior: "smooth", block: "start" });
});
