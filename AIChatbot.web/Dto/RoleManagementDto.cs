namespace AIChatbot.Web.Dto
{
    public class RoleDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }

    public class RoleUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}".Trim();

        public string Initials
        {
            get
            {
                var f = string.IsNullOrEmpty(FirstName) ? "" : FirstName[0].ToString();
                var l = string.IsNullOrEmpty(LastName) ? "" : LastName[0].ToString();
                return (f + l).ToUpper();
            }
        }
    }

    public class CreateRoleRequestDto
    {
        public string RoleName { get; set; } = string.Empty;
    }
}