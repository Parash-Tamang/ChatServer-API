using AIChatbot.web.Interfaces;
using AIChatbot.web.Models.RoleManager;
using Microsoft.AspNetCore.Mvc;


namespace AIChatbot.web.Controllers
{
    [Route("RoleManager")]
    public class RoleManagerController : Controller
    {
        private readonly IRoleManagerService _roleManagerService;

        public RoleManagerController(IRoleManagerService roleManagerService)
        {
            _roleManagerService = roleManagerService;
        }

        private string GetUserRole() =>
            User.FindFirst("role")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            ?? "User";

        [HttpGet("")]
        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var userRole = GetUserRole();
            var vm = new RoleManagerDashboardViewModel { UserRole = userRole };

            if (userRole == "SuperAdmin")
            {
                var (success, roles, message) = await _roleManagerService.ListRolesAsync();
                if (success) vm.Roles = roles;
                else vm.ErrorMessage = message;
            }

            return View(vm);
        }

        [HttpGet("CreateAdmin")]
        public IActionResult CreateAdmin()
        {
            var role = GetUserRole();
            if (role != "SuperAdmin" && role != "Admin") return Forbid();
            ViewBag.UserRole = role;
            return View();
        }

        [HttpPost("CreateAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(CreateAdminRequest request)
        {
            var role = GetUserRole();
            if (role != "SuperAdmin" && role != "Admin") return Forbid();
            if (!ModelState.IsValid) { ViewBag.UserRole = role; return View(request); }

            var (success, message) = await _roleManagerService.CreateAdminAsync(request);
            if (success) TempData["SuccessMessage"] = message;
            else TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet("CreateRole")]
        public IActionResult CreateRole()
        {
            var role = GetUserRole();
            if (role != "SuperAdmin" && role != "Admin") return Forbid();
            ViewBag.UserRole = role;
            return View();
        }

        [HttpPost("CreateRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRoleRequest request)
        {
            var role = GetUserRole();
            if (role != "SuperAdmin" && role != "Admin") return Forbid();
            if (!ModelState.IsValid) { ViewBag.UserRole = role; return View(request); }

            var (success, message) = await _roleManagerService.CreateRoleAsync(request);
            if (success) TempData["SuccessMessage"] = message;
            else TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost("DeleteRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            if (GetUserRole() != "SuperAdmin") return Forbid();
            var (success, message) = await _roleManagerService.DiscardRoleAsync(roleId);
            if (success) TempData["SuccessMessage"] = message;
            else TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet("Users/{roleId}")]
        public async Task<IActionResult> Users(string roleId)
        {
            if (GetUserRole() != "SuperAdmin") return Forbid();
            var (success, users, message) = await _roleManagerService.ListUsersAsync(roleId);
            
            // Get role name from first user or use roleId as fallback
            var roleName = users.FirstOrDefault()?.RoleName ?? roleId;
            
            return View(new RoleUsersViewModel
            {
                RoleId = roleId,
                RoleName = roleName,
                UserRole = GetUserRole(),
                Users = users,
                ErrorMessage = success ? null : message
            });
        }

        [HttpPost("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId, string? returnRoleId)
        {
            var (success, message) = await _roleManagerService.DiscardUserAsync(userId);
            if (success) TempData["SuccessMessage"] = message;
            else TempData["ErrorMessage"] = message;
            return !string.IsNullOrEmpty(returnRoleId)
                ? RedirectToAction(nameof(Users), new { roleId = returnRoleId })
                : RedirectToAction(nameof(Dashboard));
        }

        [HttpGet("AccessDenied")]
        public IActionResult AccessDenied() => View();
    }
}   