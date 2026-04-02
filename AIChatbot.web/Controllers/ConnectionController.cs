using AIChatbot.web.Interfaces;
using AIChatbot.web.Models.Chat;
using Microsoft.AspNetCore.Mvc;

namespace AIChatbot.web.Controllers
{

    public class ConnectionController : Controller
    {
        private readonly IConnectionService _connectionService;

        public ConnectionController(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConnectDatabase(SqlConnectionViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid input." });

            try
            {
                // _dbService isn't available in this project context; skip actual connection here.
                await Task.CompletedTask;

                if (success)
                    return Json(new { success = true, database = model.Database });

                return Json(new { success = false, message = "Could not connect to database." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }


}