namespace ITI.ERP.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, string? instituteName, CancellationToken ct);
}
