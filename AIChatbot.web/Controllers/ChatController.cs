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

        public ChatController(ChatApiService chat)
        {
            _chat = chat;
        }

        // dashboard page
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // get sessions
        [HttpGet]
        public async Task<IActionResult> GetSessions()
        {
            var sessions = await _chat.GetSessionsAsync();

            Console.WriteLine("SESSION COUNT: " + sessions.Count);

            foreach (var s in sessions)
                Console.WriteLine($"TOPIC => {s.TopicName}");

            return Json(sessions);
        }


        // get messages
        [HttpGet]
        public async Task<IActionResult> GetMessages(Guid id)
        {
            var messages = await _chat.GetMessagesAsync(id);
            return Json(messages);
        }

        // send message
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest req)
        {
            var result = await _chat.SendMessageAsync(req);
            return Json(result);
        }

        // retry message
        [HttpPost]
        public async Task<IActionResult> Retry([FromBody] RetryRequest req)
        {
            var result = await _chat.RetryAsync(req.ChatSessionId, req.MessageId);
            return Json(result);
        }



        // delete chat
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _chat.DeleteSessionAsync(id);

            if (!ok)
                return BadRequest();

            return Ok();
        }
    }
}
