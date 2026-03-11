namespace AIChatbot.web.Models.RoleManager
{
  
    
        public class RoleManagerDashboardViewModel
        {
            public List<RoleDto> Roles { get; set; } = new();
            public string UserRole { get; set; } = string.Empty;
            public string? ErrorMessage { get; set; }
            public string? SuccessMessage { get; set; }
        }

        public class RoleUsersViewModel
        {
            public string RoleId { get; set; } = string.Empty;
            public string RoleName { get; set; } = string.Empty;
            public List<UserDto> Users { get; set; } = new();
            public string UserRole { get; set; } = string.Empty;
            public string? ErrorMessage { get; set; }
            public string? SuccessMessage { get; set; }
        }
    
}
