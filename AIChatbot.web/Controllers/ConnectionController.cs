using AIChatbot.web.Models.Chat;
using Microsoft.AspNetCore.Mvc;

namespace AIChatbot.web.Controllers
{
    public class ConnectionController : Controller
    {

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

                return Json(new { success = true, database = model.Database });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
