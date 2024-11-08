namespace BumbleBeeFoundation_Client.Models
{
    // Models/SmtpSettings.cs
    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string AdminEmail { get; set; }
    }

}
