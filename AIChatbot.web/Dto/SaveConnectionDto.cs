namespace AIChatbot.web.Dto
{
    public class SaveConnectionDto
    {
        public Guid? Id { get; set; } = null;

        public string ServerName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public string? AuthMode { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public bool TrustCertificate { get; set; }

        public int ConnectionTimeout { get; set; }

        public bool IsActive { get; set; }
    }
}