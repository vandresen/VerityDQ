using System;
using System.Collections.Generic;
using System.Text;

namespace VerityDQ.Application.Models
{
    public sealed class UserConnectionSettings
    {
        // SQL Server
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string AuthenticationType { get; set; } = "SqlServer";
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool TrustServerCertificate { get; set; } = true;

        // OpenAI
        public string OpenAIApiKey { get; set; } = string.Empty;
        public string OpenAIModel { get; set; } = "gpt-4.1-mini";
    }
}
