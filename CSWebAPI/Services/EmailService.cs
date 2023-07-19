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
        var fromEmail = _configuration["EmailSettings:FromEmail"];
        var fromName = _configuration["EmailSettings:FromName"];
        var fromPassword = _configuration["EmailSettings:Password"];
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = _configuration["EmailSettings:SmtpPort"];
        var enableSsl = _configuration["EmailSettings:EnableSsl"];
        var baseUrl = _configuration["BaseUrl"];

        if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromName) || string.IsNullOrEmpty(fromPassword) || string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPort) || string.IsNullOrEmpty(enableSsl) || string.IsNullOrEmpty(baseUrl))
        {
            throw new ArgumentNullException("Email settings in configuration cannot be null or empty.");
        }

        var fromAddress = new MailAddress(fromEmail, fromName);
        var toAddress = new MailAddress(email);
        const string subject = "Email Verification";
        string body = $"Please verify your email by clicking the following link: {baseUrl}/users/verify?token={token}";

        SendEmail(fromAddress, toAddress, subject, body, smtpHost, smtpPort, enableSsl, fromPassword);
    }

    public void SendResetPasswordEmail(string email, string token)
    {
        var fromEmail = _configuration["EmailSettings:FromEmail"];
        var fromName = _configuration["EmailSettings:FromName"];
        var fromPassword = _configuration["EmailSettings:Password"];
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = _configuration["EmailSettings:SmtpPort"];
        var enableSsl = _configuration["EmailSettings:EnableSsl"];
        var baseUrl = _configuration["BaseUrl"];

        if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromName) || string.IsNullOrEmpty(fromPassword) || string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPort) || string.IsNullOrEmpty(enableSsl) || string.IsNullOrEmpty(baseUrl))
        {
            throw new ArgumentNullException("Email settings in configuration cannot be null or empty.");
        }

        var fromAddress = new MailAddress(fromEmail, fromName);
        var toAddress = new MailAddress(email);
        const string subject = "Password Reset Request";
        string body = $"You have requested to reset your password. Please click the following link to reset your password: {baseUrl}/reset-password?token={token}";

        SendEmail(fromAddress, toAddress, subject, body, smtpHost, smtpPort, enableSsl, fromPassword);
    }

    private void SendEmail(MailAddress fromAddress, MailAddress toAddress, string subject, string body, string smtpHost, string smtpPort, string enableSsl, string fromPassword)
    {
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
