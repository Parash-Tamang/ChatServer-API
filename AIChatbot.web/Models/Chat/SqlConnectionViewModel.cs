using System.ComponentModel.DataAnnotations;

namespace AIChatbot.web.Models.Chat
{
    public class SqlConnectionViewModel
    {
        [Required]
        public string Server { get; set; } 

        [Required]
        public string Database { get; set; }

        public string AuthMode { get; set; } // "SQL" or "Windows"

        public string Username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool EncryptConnection { get; set; } = true;

        public int Timeout { get; set; } = 30;
    }
}