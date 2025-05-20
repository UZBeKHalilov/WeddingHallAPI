using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;


namespace WeddingHallAPI.Services
{
    public interface IEmailOtpService
    {
        void SendOtpEmail(string toEmail, string otpCode);
    }

    public class EmailSettings
    {
        public string FromEmail { get; set; }
        public string FromPassword { get; set; }

        public EmailSettings()
        {
            FromEmail = Environment.GetEnvironmentVariable("FromEmail") ?? FromEmail ?? "No Email";
            FromPassword = Environment.GetEnvironmentVariable("FromPassword") ?? FromPassword ?? "No Password";
        }
    }


    public class EmailOtpService : IEmailOtpService
    {
        private readonly EmailSettings _emailSettings;

        public EmailOtpService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public void SendOtpEmail(string toEmail, string otpCode)
        {
            var fromAddress = new MailAddress(_emailSettings.FromEmail, "WeddingHall");
            var toAddress = new MailAddress(toEmail);
            string fromPassword = _emailSettings.FromPassword;
            const string subject = "Your WeddingHall One-Time Password Code";
            string body = $"Your One-Time Passwor is: {otpCode}";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };
            smtp.Send(message);
        }
    }
}
