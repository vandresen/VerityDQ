
namespace VerityDQ.Application.Models
{
    public sealed class UserConnectionSettings
    {
        // Primary SQL Server
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string AuthenticationType { get; set; } = "SqlServer";
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool TrustServerCertificate { get; set; } = true;

        // Reference SQL Server
        public string ReferenceServer { get; set; } = string.Empty;
        public string ReferenceDatabase { get; set; } = string.Empty;
        public string ReferenceAuthenticationType { get; set; } = "SqlServer";
        public string ReferenceUserName { get; set; } = string.Empty;
        public string ReferencePassword { get; set; } = string.Empty;
        public bool ReferenceTrustServerCertificate { get; set; } = true;

        // OpenAI
        public string OpenAIApiKey { get; set; } = string.Empty;
        public string OpenAIModel { get; set; } = "gpt-4.1-mini";
    }
}
