using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIChatbot.web.Filters;
using AIChatbot.web.Models.Chat;
using AIChatbot.web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIChatbot.web.Controllers
{
    [ServiceFilter(typeof(TokenAuthorizationFilter))]
    public class ChatController : Controller
    {
        private readonly ChatApiService _chat;
        
        public ChatController(ChatApiService chat)
        {
            _chat = chat;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new ChatPageModel
            {
                UserName = User.Identity?.Name ?? "User",
                Sessions = new List<ChatSessionDto>(),
                CurrentSessionMessages = new List<ChatMessageDto>()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetSessions()
        {
            try
            {
                var sessions = await _chat.GetSessionsAsync();
                return Json(new { success = true, data = sessions });
            }
            catch
            {
                return Json(new { success = false, message = "Oops! Could not load your chats. Please refresh the page." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(Guid sessionId)
        {
            try
            {
                var messages = await _chat.GetMessagesAsync(sessionId);
                return Json(new { success = true, data = messages });
            }
            catch
            {
                return Json(new { success = false, message = "Oops! Could not load messages for this chat." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PollReply(Guid sessionId, string afterUtc)
        {
            try
            {
                var messages = await _chat.GetMessagesAsync(sessionId);

                DateTime after = DateTime.MinValue;
                if (!string.IsNullOrWhiteSpace(afterUtc))
                    DateTime.TryParse(afterUtc, null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out after);

                var reply = messages
                    .Where(m => m.Role == "assistant" && m.CreatedAt > after)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();

                if (reply != null)
                    return Json(new { success = true, found = true, data = reply });

                return Json(new { success = true, found = false });
            }
            catch
            {
                return Json(new { success = false, message = "Oops! Something went wrong while waiting for a reply." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessageAjax([FromBody] SendMessageRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Message))
                return Json(new { success = false, message = "Please type a message before sending." });

            try
            {
                var result = await _chat.SendMessageAsync(req);
                if (result != null)
                    return Json(new { success = true, data = result });

                return Json(new { success = false, message = "Oops! The AI service is currently unavailable. Please try again shortly." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"DEBUG: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSessionAjax([FromBody] DeleteSessionRequest req)
        {
            try
            {
                await _chat.DeleteSessionAsync(req.SessionId);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Oops! Could not delete this chat. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetryAjax([FromBody] RetryRequest req)
        {
            try
            {
                var result = await _chat.RetryAsync(req.SessionId, req.MessageId);
                if (result != null)
                    return Json(new { success = true, data = result });

                return Json(new { success = false, message = "Oops! Retry failed. The AI service may be unavailable." });
            }
            catch
            {
                return Json(new { success = false, message = "Oops! Something went wrong during retry." });
            }
        }

        [HttpGet]
        public IActionResult LoadConnectionModal()
        {
            var model = new SqlConnectionViewModel();
            return PartialView("~/Views/Settings/_SqlConnectionModal.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConnectDatabase(SqlConnectionViewModel connection)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index");
            TempData["ConnectionSuccess"] = "Connected Successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Dash()
        {
            var model = new ChatPageModel
            {
                UserName = User.Identity?.Name ?? "Guest",
                Sessions = new List<ChatSessionDto>
                {
                    new ChatSessionDto { Id = Guid.NewGuid(), TopicName = "First Chat" },
                    new ChatSessionDto { Id = Guid.NewGuid(), TopicName = "Second Chat" }
                },
                CurrentSessionId = null,
                CurrentSessionMessages = new List<ChatMessageDto>()
            };
            return View("Dash", model);
        }

        [HttpPost]
        public IActionResult OpenSession(Guid sessionId) =>
            RedirectToAction("Index", new { sessionId });

        [HttpPost]
        public IActionResult NewChat() => RedirectToAction("Index");

        [HttpPost]
        public async Task<IActionResult> DeleteSession(Guid sessionId)
        {
            try { await _chat.DeleteSessionAsync(sessionId); } catch { }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Retry(Guid chatSessionId, Guid messageId)
        {
            try
            {
                var result = await _chat.RetryAsync(chatSessionId, messageId);
                if (result != null)
                    return RedirectToAction("Index", new { sessionId = chatSessionId });
            }
            catch { }
            return RedirectToAction("Index", new { sessionId = chatSessionId });
        }
    }

    public class DeleteSessionRequest { public Guid SessionId { get; set; } }
    public class RetryRequest { public Guid SessionId { get; set; } public Guid MessageId { get; set; } }
}