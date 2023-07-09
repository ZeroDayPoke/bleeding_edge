using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendVerificationEmail(string email, string token)
    {
        var fromEmail = _configuration["EmailSettings:FromEmail"];
        var fromName = _configuration["EmailSettings:FromName"];
        var fromPassword = _configuration["EmailSettings:Password"];
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = _configuration["EmailSettings:SmtpPort"];
        var enableSsl = _configuration["EmailSettings:EnableSsl"];

        if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromName) || string.IsNullOrEmpty(fromPassword) || string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPort) || string.IsNullOrEmpty(enableSsl))
        {
            throw new ArgumentNullException("Email settings in configuration cannot be null or empty.");
        }

        var fromAddress = new MailAddress(fromEmail, fromName);
        var toAddress = new MailAddress(email);
        const string subject = "Email Verification";
        string body = $"Your verification token is: {token}";

        var smtp = new SmtpClient
        {
            Host = smtpHost,
            Port = int.Parse(smtpPort),
            EnableSsl = bool.Parse(enableSsl),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };
        using (var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body
        })
        {
            smtp.Send(message);
        }
    }
}
