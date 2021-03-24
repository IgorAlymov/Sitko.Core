using MailKit.Security;

namespace Sitko.Core.Email.Smtp
{
    public class SmtpEmailModuleConfig : FluentEmailModuleConfig
    {
        public string Server { get; set; } = "localhost";
        public int Port { get; set; } = 25;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public SecureSocketOptions SocketOptions { get; set; } = SecureSocketOptions.Auto;
    }
}