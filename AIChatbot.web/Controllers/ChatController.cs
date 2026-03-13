using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
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
        private readonly TokenService _token;

        public ChatController(ChatApiService chat, TokenService token)
        {
            _chat = chat;
            _token = token;
        }

        // ?????????????????????????????????????????????
        // PAGE ENTRY POINT
        // ?????????????????????????????????????????????
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

        // ?????????????????????????????????????????????
        // AJAX – GET all sessions
        // ?????????????????????????????????????????????
        [HttpGet]
        public async Task<IActionResult> GetSessions()
        {
            try
            {
                var sessions = await _chat.GetSessionsAsync();
                return Json(new { success = true, data = sessions });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ?????????????????????????????????????????????
        // AJAX – GET messages for a session
        // ?????????????????????????????????????????????
        [HttpGet]
        public async Task<IActionResult> GetMessages(Guid sessionId)
        {
            try
            {
                var messages = await _chat.GetMessagesAsync(sessionId);
                return Json(new { success = true, data = messages });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ?????????????????????????????????????????????
        // AJAX – SEND message (no page reload)
        // ?????????????????????????????????????????????
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessageAjax([FromBody] SendMessageRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Message))
                return Json(new { success = false, message = "Empty message." });

            try
            {
                var result = await _chat.SendMessageAsync(req);
                if (result != null)
                    return Json(new { success = true, data = result });

                return Json(new { success = false, message = "No response from AI." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ?????????????????????????????????????????????
        // AJAX – DELETE a session
        // ?????????????????????????????????????????????
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSessionAjax([FromBody] DeleteSessionRequest req)
        {
            try
            {
                await _chat.DeleteSessionAsync(req.SessionId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ?????????????????????????????????????????????
        // AJAX – RETRY a message
        // ?????????????????????????????????????????????
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetryAjax([FromBody] RetryRequest req)
        {
            try
            {
                var result = await _chat.RetryAsync(req.SessionId, req.MessageId);
                if (result != null)
                    return Json(new { success = true, data = result });

                return Json(new { success = false, message = "Retry failed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ?????????????????????????????????????????????
        // MODAL – kept as-is from your original code
        // ?????????????????????????????????????????????
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

        // ?????????????????????????????????????????????
        // KEPT from your original – Dash, OpenSession,
        // NewChat, SendMessage (old), DeleteSession, Retry
        // ?????????????????????????????????????????????
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
        public async Task<IActionResult> OpenSession(Guid sessionId)
        {
            return RedirectToAction("Index", new { sessionId });
        }

        [HttpPost]
        public async Task<IActionResult> NewChat()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSession(Guid sessionId)
        {
            try
            {
                await _chat.DeleteSessionAsync(sessionId);
            }
            catch (Exception ex) { }

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
            catch (Exception ex) { }

            return RedirectToAction("Index", new { sessionId = chatSessionId });
        }
    }

    // ?? Request DTOs for AJAX endpoints ??
    public class DeleteSessionRequest { public Guid SessionId { get; set; } }
    public class RetryRequest { public Guid SessionId { get; set; } public Guid MessageId { get; set; } }
}