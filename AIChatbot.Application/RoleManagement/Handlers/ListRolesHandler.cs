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
    public class ListRolesHandler
     : IRequestHandler<ListRolesQuery, List<object>>
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ListRolesHandler(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<List<object>> Handle(ListRolesQuery request, CancellationToken ct)
        {
            var requester = await _userManager.FindByIdAsync(request.RequestedByUserId);
            if (requester == null)
                throw new Exception("Requester not found");

            var roles = await _userManager.GetRolesAsync(requester);

            if (!roles.Contains("SuperAdmin"))
                throw new UnauthorizedAccessException("Only SuperAdmin can view roles");

            return _roleManager.Roles
                .Select(r => new
                {
                    RoleId = r.Id,
                    RoleName = r.Name
                })
                .ToList<object>();
        }
    }
}
