using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UserInterfaceChatBot.Controllers
{

    public class ChatController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public ChatController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            return View(); // returns your chat UI
        }

        [HttpPost]
        [Route("Chat/SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Message cannot be empty." });

            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer ");

            var payload = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[] { new { role = "user", content = request.Message } },
                temperature = 0,
                max_tokens = 4096
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            return Content(responseString, "application/json");
        }
    }
}
public class ChatRequest
{
    public string? Message { get; set; }
}
