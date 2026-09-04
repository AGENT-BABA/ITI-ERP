using ITI.ERP.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace ITI.ERP.Infrastructure.Email;

public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, string? instituteName, CancellationToken ct)
    {
        var subject = "Password Reset Request - ITI ERP";
        var body = BuildPasswordResetBody(userName, resetLink, instituteName);

        var logDir = Path.Combine(AppContext.BaseDirectory, "Logs", "emails");
        Directory.CreateDirectory(logDir);
        var logFile = Path.Combine(logDir, $"password-reset-{DateTime.UtcNow:yyyyMMdd}.log");

        var entry = $"""
            [{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC]
            To: {toEmail}
            Subject: {subject}
            Body:
            {body}
            ---
            """;

        File.AppendAllText(logFile, entry + Environment.NewLine);

        _logger.LogWarning(
            "DEV EMAIL: Password reset requested for {Email}. Reset link written to {LogFile}. " +
            "DO NOT log this in production. Raw token is present in the file.",
            toEmail, logFile);

        return Task.CompletedTask;
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
