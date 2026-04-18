namespace AIChatbot.web.Models.Admin
{
    public class CreateConnectionRequest
    {
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string? AuthMode { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool TrustCertificate { get; set; }
        public int ConnectionTimeout { get; set; }
    }
}
