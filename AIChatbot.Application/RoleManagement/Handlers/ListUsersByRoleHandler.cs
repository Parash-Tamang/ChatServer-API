using AIChatbot.Application.RoleManagement.Queries;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.RoleManagement.Handlers
{
    public class ListUsersByRoleHandler
      : IRequestHandler<ListUsersByRoleQuery, List<object>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ListUsersByRoleHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<List<object>> Handle(ListUsersByRoleQuery request, CancellationToken ct)
        {
            // 🔐 validate requester
            var requester = await _userManager.FindByIdAsync(request.RequestedByUserId);
            if (requester == null)
                throw new Exception("Requester not found");

            var requesterRoles = await _userManager.GetRolesAsync(requester);

            // 🔐 allow only Admin / SuperAdmin
            if (!requesterRoles.Contains("Admin") && !requesterRoles.Contains("SuperAdmin"))
                throw new UnauthorizedAccessException("Only Admin/SuperAdmin can view role users");

            // 🎯 fetch role
            var role = await _roleManager.FindByIdAsync(request.RoleId)
                ?? throw new Exception("Role not found");

            var users = await _userManager.GetUsersInRoleAsync(role.Name!);

            return users.Select(u => new
            {
                UserId = u.Id,
                u.Email,
                u.FirstName,
                u.LastName
            }).ToList<object>();
        }
    }
}
