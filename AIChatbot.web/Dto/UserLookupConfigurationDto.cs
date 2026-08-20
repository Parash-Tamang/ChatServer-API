namespace AIChatbot.web.Dto
{
    public class UserLookupConfigurationDto
    {
        public string RoleId { get; set; } = string.Empty;
        public Guid ConnectionId { get; set; }
        public string UserTableName { get; set; } = string.Empty;
        public string UserIdColumn { get; set; } = string.Empty;
        public string? EmailColumn { get; set; }
        public string? PhoneColumn { get; set; }
        public string? FirstNameColumn { get; set; }
        public string? LastNameColumn { get; set; }
        public string? FullNameColumn { get; set; }
        public bool UseSeparateFirstLast { get; set; }
        public bool UseMergedFullName { get; set; }
    }

    public class SaveUserLookupConfigurationDto
    {
        public string RoleId { get; set; } = string.Empty;
        public Guid ConnectionId { get; set; }
        public string UserTableName { get; set; } = string.Empty;
        public string UserIdColumn { get; set; } = string.Empty;
        public string? EmailColumn { get; set; }
        public string? PhoneColumn { get; set; }
        public string? FirstNameColumn { get; set; }
        public string? LastNameColumn { get; set; }
        public string? FullNameColumn { get; set; }
        public bool UseSeparateFirstLast { get; set; }
        public bool UseMergedFullName { get; set; }
    }
}
