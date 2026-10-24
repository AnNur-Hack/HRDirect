using System.Net;
using System.Net.Mail;
using HRDirectAPI.Services.Interfaces;

namespace HRDirectAPI.Services.Implementations;

public class EmailService(IConfiguration configuration) : IEmailService
{
    private readonly IConfiguration _configuration = configuration;
    
    
    public async Task SendEmailAsync(string recipient, string subject, string message)
    {
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = int.Parse(
            _configuration["EmailSettings:SmtpPort"]!);
        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var senderPassword = _configuration["EmailSettings:SenderPassword"];

        using var smtpClient = new SmtpClient(smtpHost, smtpPort);

        smtpClient.EnableSsl = true;
        smtpClient.Credentials = new NetworkCredential(
            senderEmail,
            senderPassword);

        using var mailMessage = new MailMessage();

        mailMessage.From = new MailAddress(senderEmail!);
        mailMessage.To.Add(recipient);
        mailMessage.Subject = subject;
        mailMessage.Body = message;
        mailMessage.IsBodyHtml = false;

        await smtpClient.SendMailAsync(mailMessage);    }
}