using Microsoft.AspNetCore.Mvc;
using AIChatbot.web.Filters;

namespace AIChatbot.web.Controllers
{
    [ServiceFilter(typeof(AuthFilter))]
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
