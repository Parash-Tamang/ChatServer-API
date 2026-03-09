using Microsoft.AspNetCore.Mvc;

namespace AIChatbot.web.Controllers
{
    public class ConnectionController : Controller
    {
        [HttpGet]
        public IActionResult Connection()
        {
            return PartialView("_ConnectionPartial");
        }
    }
}
