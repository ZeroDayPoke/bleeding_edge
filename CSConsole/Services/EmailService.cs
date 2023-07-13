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
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPortString = _configuration["EmailSettings:SmtpPort"];
        var enableSslString = _configuration["EmailSettings:EnableSsl"];
        var fromPassword = _configuration["EmailSettings:Password"];

        if (fromEmail is null || fromName is null || smtpHost is null || smtpPortString is null || enableSslString is null || fromPassword is null)
        {
            throw new InvalidOperationException("Email settings are not properly configured.");
        }

        var smtpPort = int.Parse(smtpPortString);
        var enableSsl = bool.Parse(enableSslString);

        var fromAddress = new MailAddress(fromEmail, fromName);
        var toAddress = new MailAddress(email);
        const string subject = "Email Verification";
        string body = $"Your verification token is: {token}";

        using (var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body
        })
        {
            using (var smtp = new SmtpClient(smtpHost, smtpPort))
            {
                smtp.EnableSsl = enableSsl;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(fromAddress.Address, fromPassword);

                smtp.Send(message);
            }
        }
    }
}
