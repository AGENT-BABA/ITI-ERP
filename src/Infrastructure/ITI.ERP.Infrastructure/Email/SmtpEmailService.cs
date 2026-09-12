using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ITI.ERP.Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, string? instituteName, CancellationToken ct)
    {
        ValidateSmtpConfiguration();

        var subject = "Password Reset Request - ITI ERP";
        var body = BuildPasswordResetBody(userName, resetLink, instituteName);

        using var message = new MailMessage();
        message.From = new MailAddress(_options.FromAddress ?? "noreply@iti-erp.com", _options.FromName ?? "ITI ERP");
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = true;

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.SmtpUseSsl,
            Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword)
        };

        try
        {
            await client.SendMailAsync(message, ct);
            _logger.LogInformation("Password reset email sent to {Email}", toEmail);
        }
        catch (SmtpException)
        {
            _logger.LogError("Failed to send password reset email to {Email}: SMTP error (status code: {StatusCode})", toEmail, "transient/failure");
            throw new InvalidOperationException("Failed to send password reset email due to an SMTP error. Check SMTP server availability and configuration.");
        }
        catch (Exception)
        {
            _logger.LogError("Failed to send password reset email to {Email}: unexpected error", toEmail);
            throw new InvalidOperationException("Failed to send password reset email. Verify SMTP configuration and server connectivity.");
        }
    }

    private void ValidateSmtpConfiguration()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_options.SmtpHost))
            errors.Add("Email:SmtpHost is not configured.");
        if (string.IsNullOrWhiteSpace(_options.SmtpUsername))
            errors.Add("Email:SmtpUsername is not configured.");
        if (string.IsNullOrWhiteSpace(_options.SmtpPassword))
            errors.Add("Email:SmtpPassword is not configured.");

        if (errors.Count > 0)
        {
            errors.Add("Set via: dotnet user-secrets set \"Email:SmtpUsername\" \"<address>\" --project src/API/ITI.ERP.Api");
            errors.Add("Set via: dotnet user-secrets set \"Email:SmtpPassword\" \"<app-password>\" --project src/API/ITI.ERP.Api");
            throw new InvalidOperationException(string.Join(" ", errors));
        }
    }

    private static string BuildPasswordResetBody(string userName, string resetLink, string? instituteName)
    {
        var orgName = string.IsNullOrEmpty(instituteName) ? "ITI ERP" : instituteName;
        return $"""
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
                <h2 style="color: #1a365d;">Password Reset Request</h2>
                <p>Hello {userName},</p>
                <p>A password reset was requested for your account at <strong>{orgName}</strong>.</p>
                <p>Click the button below to reset your password. This link expires in <strong>15 minutes</strong>.</p>
                <p style="text-align: center; margin: 30px 0;">
                    <a href="{resetLink}" style="background-color: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;">Reset Password</a>
                </p>
                <p style="color: #666; font-size: 13px;">If you did not request this password reset, please ignore this email. Your password will remain unchanged.</p>
                <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;">
                <p style="color: #999; font-size: 12px;">This is an automated message from {orgName}. Do not reply to this email.</p>
            </body>
            </html>
            """;
    }
}
