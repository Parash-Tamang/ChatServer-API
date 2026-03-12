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
                // your existing connection/service logic here
                await _dbService.ConnectAsync(model);

                return Json(new { success = true, database = model.Database });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }





    }
}
