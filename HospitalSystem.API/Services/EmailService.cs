using HospitalSystem.API.Configuration;
using HospitalSystem.API.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HospitalSystem.API.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string? toEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogInformation("Patient email notification skipped because no patient email address is available.");
            return;
        }

        if (!_settings.IsEnabled)
        {
            _logger.LogInformation("Patient email notification skipped because email delivery is disabled.");
            return;
        }

        ValidateSettings();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();
        var socketOptions = _settings.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await client.ConnectAsync(_settings.SmtpServer, _settings.Port, socketOptions);

        if (!string.IsNullOrWhiteSpace(_settings.Username))
        {
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpServer))
        {
            throw new InvalidOperationException("EmailSettings:SmtpServer must be configured when email delivery is enabled.");
        }

        if (string.IsNullOrWhiteSpace(_settings.SenderEmail))
        {
            throw new InvalidOperationException("EmailSettings:SenderEmail must be configured when email delivery is enabled.");
        }
    }
}
