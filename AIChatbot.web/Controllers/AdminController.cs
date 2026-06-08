using AIChatbot.web.Dto;
using AIChatbot.web.Filters;    
using AIChatbot.web.Interfaces;
using AIChatbot.web.Models.Admin;
using AIChatbot.web.Services;
 
using Microsoft.AspNetCore.Mvc;


namespace AIChatbot.web.Controllers
{
    [ServiceFilter(typeof(TokenAuthorizationFilter))]
    public class AdminController : Controller
    {
        private readonly IConnectionService _connectionService;
        private readonly IRoleManagerService _roleManagerService;
        public AdminController(IConnectionService connectionService, IAdminPromptService adminPromptService, IRoleManagerService roleManagerService)
        {
            _connectionService = connectionService;
            _adminPromptService = adminPromptService;
            _roleManagerService = roleManagerService;
        }
        private readonly IAdminPromptService _adminPromptService;




        public IActionResult Dashboard() => View();
        public IActionResult ConfigureDatabases() => View();
        public IActionResult KnowledgeBases() => View();
        public IActionResult Settings() => View();
        public async Task<IActionResult> ManagePrompts()
        {
            var model = await _connectionService.GetManagePromptsAsync();
            return View(model);
        }




        //[ServiceFilter(typeof(SuperAdminFilter))]
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

            // Reload the saved connection so the rendered row gets the generated Id.
            var connections = await _connectionService.GetAllConnectionsAsync();
            var savedConnection = connections.FirstOrDefault(c =>
                string.Equals(c.ServerName, model.ServerName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.DatabaseName, model.DatabaseName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.AuthMode, model.AuthMode, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Username ?? string.Empty, model.Username ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
                c.ConnectionTimeout == model.ConnectionTimeout &&
                c.TrustCertificate == model.TrustCertificate);

            if (savedConnection != null)
                model.Id = savedConnection.Id;

            return PartialView("_ConnectionTableRow", model);
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

        [HttpPost]
        public async Task<IActionResult> CreateKB([FromBody] Guid connectionId)
        {
            var success = await _connectionService.CreateKnowledgeBaseAsync(connectionId);
            if (success)
                TempData["Success"] = "Knowledge base created successfully";
            else
                TempData["Error"] = "Failed to create knowledge base";
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
        /// <summary>
        /// prompts view 
        /// </summary>

        [HttpGet]
        public async Task<IActionResult> GetGlobalFunctions()
        {
            var functions = await _adminPromptService.GetGlobalFunctionsAsync();
            return Json(functions);
        }

        [HttpGet]
        public async Task<IActionResult> GetFunctionById(string functionId)
        {
            if (string.IsNullOrEmpty(functionId))
                return BadRequest(new { message = "Function ID is required." });

            var function = await _adminPromptService.GetFunctionByIdAsync(functionId);

            if (function == null)
                return NotFound(new { message = "Function not found." });

            return Json(function);
        }
        [HttpPost]
        public async Task<IActionResult> SaveGlobalPrompt([FromBody] SaveGlobalPromptRequest request)
        {
            if (string.IsNullOrEmpty(request.FunctionName))
                return BadRequest(new { message = "Function name is required." });

            var success = await _adminPromptService.SaveGlobalPromptAsync(request);
            return success
                ? Json(new { message = "Prompt saved successfully." })
                : StatusCode(500, new { message = "Failed to save prompt." });
        }

        [HttpGet]
        public async Task<IActionResult> GetConnectionFunctions(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
                return BadRequest(new { message = "Connection ID is required." });

            var functions = await _adminPromptService.GetConnectionFunctionsAsync(connectionId);
            return Json(functions);
        }

        [HttpPost]
        public async Task<IActionResult> SaveLocalPrompt([FromBody] SaveLocalPromptRequest request)
        {
            if (string.IsNullOrEmpty(request.FunctionName))
                return BadRequest(new { message = "Function name is required." });

            var success = await _adminPromptService.SaveLocalPromptAsync(request);
            return success
                ? Json(new { message = "Prompt saved successfully." })
                : StatusCode(500, new { message = "Failed to save prompt." });
        }

        // the role part is below 
        // GET: /Admin/RoleManagement
        public async Task<IActionResult> RoleManagement()
        {
            return View();
        }

        // GET: /Admin/GetRoles  (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManagerService.GetAllRolesAsync();
            return Json(new { success = true, data = roles });
        }

        // POST: /Admin/CreateRole  (AJAX)
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.RoleName))
                return Json(new { success = false, message = "Role name is required." });

            var result = await _roleManagerService.CreateRoleAsync(request.RoleName.Trim());
            return Json(result
                ? new { success = true, message = $"Role '{request.RoleName}' created successfully." }
                : new { success = false, message = "Failed to create role. Please try again." });
        }

        // DELETE: /Admin/DeleteRole  (AJAX)
        [HttpDelete]
        public async Task<IActionResult> DeleteRole([FromBody] DeleteRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.RoleId))
                return Json(new { success = false, message = "Role ID is required." });

            var result = await _roleManagerService.DeleteRoleAsync(request.RoleId);
            return Json(result
                ? new { success = true, message = "Role deleted successfully." }
                : new { success = false, message = "Failed to delete role. Please try again." });
        }

        // GET: 
        [HttpGet]
        public async Task<IActionResult> GetUsersInRole(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return Json(new { success = false, message = "Role ID is required." });

            var users = await _roleManagerService.GetUsersInRoleAsync(roleId);
            return Json(new { success = true, data = users });
        }

        // DELETE: /Admin/DeleteUser  (AJAX)
        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.UserId))
                return Json(new { success = false, message = "User ID is required." });

            var result = await _roleManagerService.DeleteUserAsync(request.UserId);
            return Json(result
                ? new { success = true, message = "User deleted successfully." }
                : new { success = false, message = "Failed to delete user. Please try again." });
        }

        // GET: /Admin/GetConnections  (AJAX — for assign DB to role dropdown)
        [HttpGet]
        public async Task<IActionResult> GetConnections()
        {
            var connections = await _connectionService.GetAllConnectionsAsync();
            var mapped = connections.Select(c => new
            {
                id = c.Id,
                name = $"{c.ServerName} / {c.DatabaseName}"
            });
            return Json(new { success = true, data = mapped });
        }

        // POST: /Admin/AssignRoleToConnection  (AJAX)
        [HttpPost]
        public async Task<IActionResult> AssignRoleToConnection([FromBody] AssignRoleConnectionDto request)
        {
            if (string.IsNullOrWhiteSpace(request?.RoleId) || request.ConnectionId == Guid.Empty)
                return Json(new { success = false, message = "Role and connection are required." });

            var result = await _roleManagerService.AssignRoleToConnectionAsync(request.RoleId, request.ConnectionId);
            return Json(result
                ? new { success = true, message = "Role assigned to database successfully." }
                : new { success = false, message = "Failed to assign role. Please try again." });
        }
        // GET: /Admin/GetRoleDbAccess?roleId=xxx  (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetRoleDbAccess(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return Json(new { success = false, message = "Role ID ias required." });

            var assignments = await _roleManagerService.GetRoleConnectionsAsync(roleId);
            return Json(new { success = true, data = assignments });
        }

        // Request models (add these inside the Controllers namespace or a separate file)
        public class CreateRoleRequest { public string RoleName { get; set; } = string.Empty; }
        public class DeleteRoleRequest { public string RoleId { get; set; } = string.Empty; }
        public class DeleteUserRequest { public string UserId { get; set; } = string.Empty; }
    }
}


