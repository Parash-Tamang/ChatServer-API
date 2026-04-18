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



        public async Task<IActionResult> SaveConnection([FromBody] ConnectionRequestDto model)
        {
            model.Id = null;
            model.IsActive = false;
            var result = await _connectionService.SaveConnectionAsync(model);

            if (!result.Success)
                return Json(new { success = false, message = result.Message });

            return PartialView("_ConnectionTableRow", model); // ⚠️ uses ORIGINAL model
        }


        [HttpPost]
        public async Task<IActionResult> UpdateConnection([FromBody] ConnectionRequestDto model)
        {
            var success = await _connectionService.UpdateConnectionAsync(model);
            return Json(new { success });
        }


        [HttpPost]
        public async Task<IActionResult> SetActive([FromBody] Guid connectionId)
        {
            var success = await _connectionService.ActivateConnectionAsync(connectionId);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateKB([FromBody] Guid connectionId)
        {
            var success = await _connectionService.UpdateKnowledgeBaseAsync(connectionId);
            if (success)
                TempData["Success"] = "Knowledge base updated successfully";
            else
                TempData["Error"] = "Failed to update knowledge base";
            return Json(new { success });
        }
        [HttpGet]
        public async Task<IActionResult> GetLocalPrompt(string connectionId)
        {
            var function = await _connectionService.GetConnectionFunctionAsync(connectionId);
            if (function == null)
                return Json(new { success = false });

            var prompt = await _connectionService.GetLocalPromptAsync(function.Id);
            if (prompt == null)
                return Json(new { success = false });

            return Json(new { success = true, data = prompt });
        }

        [HttpPost]
        public async Task<IActionResult> SaveGlobalPrompt([FromBody] GlobalPromptDto dto)
        {
            var success = await _connectionService.SaveGlobalPromptAsync(dto);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> SaveLocalPrompt([FromBody] LocalPromptDto dto)
        {
            var success = await _connectionService.SaveLocalPromptAsync(dto);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> SetPromptingMode([FromBody] SetPromptingModeRequest request)
        {
            var success = await _connectionService.SetPromptingModeAsync(request.ConnectionId, request.PromptingMode);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConnection([FromBody] Guid connectionId)
        {
            var success = await _connectionService.DeleteConnectionAsync(connectionId);
            if (success) TempData["Success"] = "Connection deleted successfully";
            else TempData["Error"] = "Failed to delete connection";
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFunction([FromBody] DeleteFunctionRequest request)
        {
            var success = await _connectionService.DeleteFunctionAsync(request.ConnectionId, request.FunctionId);
            return Json(new { success });
        }


    }
}