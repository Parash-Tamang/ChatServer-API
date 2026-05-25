using AIChatbot.web.Dto;
using AIChatbot.web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIChatbot.web.Controllers
{
    public class ColumnExclusionController : Controller
    {
        private readonly IColumnExclusionService _exclusionService;
        private readonly IConnectionService _connectionService;

        public ColumnExclusionController(
            IColumnExclusionService exclusionService,
            IConnectionService connectionService)
        {
            _exclusionService = exclusionService;
            _connectionService = connectionService;
        }

        // GET: /ColumnExclusion/ColumnExclusion
        public async Task<IActionResult> ColumnExclusion()
        {
            var connections = await _connectionService.GetAllConnectionsAsync();
            return View("~/Views/Admin/ColumnExclusion.cshtml", connections);
        }
        // GET: /ColumnExclusion/GetSchema?connectionId=xxx
        [HttpGet]
        public async Task<IActionResult> GetSchema(Guid connectionId)
        {
            if (connectionId == Guid.Empty)
                return Json(new { success = false, message = "Connection ID is required." });

            var schema = await _exclusionService.GetSchemaAsync(connectionId);
            return Json(new { success = true, data = schema });
        }

        // GET: /ColumnExclusion/GetExclusions?connectionId=xxx
        [HttpGet]
        public async Task<IActionResult> GetExclusions(Guid connectionId)
        {
            if (connectionId == Guid.Empty)
                return Json(new { success = false, message = "Connection ID is required." });

            var exclusions = await _exclusionService.GetExclusionsAsync(connectionId);
            return Json(new { success = true, data = exclusions });
        }

        // POST: /ColumnExclusion/SaveExclusions
        [HttpPost]
        public async Task<IActionResult> SaveExclusions([FromBody] ExclusionSaveDto request)
        {
            if (request.ConnectionId == Guid.Empty)
                return Json(new { success = false, message = "Connection ID is required." });

            var result = await _exclusionService.SaveExclusionsAsync(request);
            return Json(result
                ? new { success = true, message = "Exclusions saved successfully." }
                : new { success = false, message = "Failed to save exclusions. Please try again." });
        }
    }
}