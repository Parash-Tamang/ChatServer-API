using AIChatbot.web.Dto;
using AIChatbot.web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using AIChatbot.web.Models.Admin;


namespace AIChatbot.web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConnectionService _connectionService;

        public AdminController(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public IActionResult Dashboard() => View();
        public IActionResult ConfigureDatabases() => View();
        public IActionResult KnowledgeBases() => View();
        public IActionResult Settings() => View();
        public async Task<IActionResult> ManagePrompts()
        {
            var model = await _connectionService.GetManagePromptsAsync();
            return View(model);
        }

        public async Task<IActionResult> DatabaseConnections()
        {
            var connections = await _connectionService.GetAllConnectionsAsync();
            return View(connections);
        }


        [HttpPost]
        public async Task<IActionResult> SaveConnection(ConnectionRequestDto model)
        {
            model.Id = null;
            model.IsActive = false;

            var result = await _connectionService.SaveConnectionAsync(model);

            if (result.Success)
                TempData["Success"] = result.Message;
            else
                TempData["Error"] = result.Details ?? result.Message.ToString();

            return RedirectToAction("DatabaseConnections");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateConnection(ConnectionRequestDto model)
        {
            var success = await _connectionService.UpdateConnectionAsync(model);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> SetActive(Guid id)
        {
            var allConnections = await _connectionService.GetAllConnectionsAsync();
            var success = await _connectionService.SetActiveConnectionAsync(id, allConnections);
            return Json(new { success });
        }
    }
}