using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendVerificationEmail(string email, string token)
    {
        var fromAddress = new MailAddress(_configuration["EmailSettings:FromEmail"], _configuration["EmailSettings:FromName"]);
        var toAddress = new MailAddress(email);
        var fromPassword = _configuration["EmailSettings:Password"];
        const string subject = "Email Verification";
        string body = $"Your verification token is: {token}";

        var smtp = new SmtpClient
        {
            Host = _configuration["EmailSettings:SmtpHost"],
            Port = int.Parse(_configuration["EmailSettings:SmtpPort"]),
            EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]),
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
