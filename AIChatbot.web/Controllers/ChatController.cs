using AIChatbot.web.Filters;
using AIChatbot.web.Models.Chat;
using AIChatbot.web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIChatbot.web.Controllers
{
    [ServiceFilter(typeof(AuthFilter))]
    public class ChatController : Controller
    {
        private readonly ChatApiService _chat;
        private readonly TokenService _token;

        public ChatController(ChatApiService chat, TokenService token)
        {
            _chat = chat;
            _token = token;
        }

        /// <summary>
        /// Display chat page with sessions and messages
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(Guid? sessionId)
        {
            try
            {
                // Get all sessions
                var sessions = await _chat.GetSessionsAsync();

                // Get current user details
                var userName = "User";
                try
                {
                    var res = await new HttpClient().GetAsync("http://localhost:5000/Auth/UserDetails");
                    if (res.IsSuccessStatusCode)
                    {
                        var json = await res.Content.ReadAsStringAsync();
                        // Parse user name if needed
                    }
                }
                catch { }

                var model = new ChatPageModel
                {
                    Sessions = sessions,
                    CurrentSessionId = sessionId,
                    UserName = userName,
                    CurrentSessionMessages = new()
                };

                // Load messages if session selected
                if (sessionId.HasValue && sessionId.Value != Guid.Empty)
                {
                    var messages = await _chat.GetMessagesAsync(sessionId.Value);
                    model.CurrentSessionMessages = messages;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return View(new ChatPageModel { UserName = "User" });
            }
        }

        /// <summary>
        /// Open a chat session
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> OpenSession(Guid sessionId)
        {
            return RedirectToAction("Index", new { sessionId });
        }

        /// <summary>
        /// Create new chat session
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> NewChat()
        {
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Send a message to the chat
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendMessage(Guid? chatSessionId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return RedirectToAction("Index", new { sessionId = chatSessionId });
            }

            try
            {
                // Send message to API
                var req = new SendMessageRequest
                {
                    ChatSessionId = chatSessionId,
                    Message = message
                };

                var result = await _chat.SendMessageAsync(req);

                if (result != null)
                {
                    // Redirect to the session with the new/updated session ID
                    return RedirectToAction("Index", new { sessionId = result.ChatSessionId });
                }
            }
            catch (Exception ex)
            {
                // Log error
            }

            return RedirectToAction("Index", new { sessionId = chatSessionId });
        }

        /// <summary>
        /// Delete a chat session
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteSession(Guid sessionId)
        {
            try
            {
                await _chat.DeleteSessionAsync(sessionId);
            }
            catch (Exception ex)
            {
                // Log error
            }

            // Redirect back to chat index
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Retry last message (optional, for backwards compatibility)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Retry(Guid chatSessionId, Guid messageId)
        {
            try
            {
                var result = await _chat.RetryAsync(chatSessionId, messageId);

                if (result != null)
                {
                    return RedirectToAction("Index", new { sessionId = chatSessionId });
                }
            }
            catch (Exception ex)
            {
                // Log error
            }

            return RedirectToAction("Index", new { sessionId = chatSessionId });
        }
    }
}
