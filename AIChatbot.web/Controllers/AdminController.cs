using Microsoft.AspNetCore.Mvc;

namespace AIChatbot.web.Controllers
{
    public class AdminController : Controller
    {
            public IActionResult Dashboard()
            {
                return View();
            }

            public IActionResult DatabaseConnections()
            {
                return View();
            }

            public IActionResult ConfigureDatabases()
            {
                return View();
            }

            public IActionResult KnowledgeBases()
            {
                return View();
            }

            public IActionResult Settings()
            {
                return View();
            }
    }
}
