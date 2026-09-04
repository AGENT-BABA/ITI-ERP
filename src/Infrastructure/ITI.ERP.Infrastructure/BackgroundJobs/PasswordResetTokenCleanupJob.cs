using Hangfire;
using ITI.ERP.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITI.ERP.Infrastructure.BackgroundJobs
{
    public class PasswordResetTokenCleanupJob : IRecurringJob
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<PasswordResetTokenCleanupJob> _logger;

        public PasswordResetTokenCleanupJob(
            IApplicationDbContext context,
            ILogger<PasswordResetTokenCleanupJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string JobId => "password-reset-token-cleanup";

        public string CronExpression => Cron.Daily(4, 30);

        public string Queue => "default";

        public async Task Execute()
        {
            _logger.LogInformation("Starting password reset token cleanup job at {Time}", DateTime.UtcNow);

            var deletedCount = await _context.PasswordResetTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow || t.UsedAt != null)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Deleted {Count} expired/used password reset tokens", deletedCount);
        }
    }
}
