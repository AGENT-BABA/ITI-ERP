using Hangfire;
using ITI.ERP.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITI.ERP.Infrastructure.BackgroundJobs
{
    public class ExpiredTokenCleanupJob : IRecurringJob
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<ExpiredTokenCleanupJob> _logger;

        public ExpiredTokenCleanupJob(
            IApplicationDbContext context,
            ILogger<ExpiredTokenCleanupJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string JobId => "expired-token-cleanup";

        public string CronExpression => Cron.Daily(3, 0);

        public string Queue => "default";

        public async Task Execute()
        {
            _logger.LogInformation("Starting expired token cleanup job at {Time}", DateTime.UtcNow);

            var deletedCount = await _context.RefreshTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow && !t.IsActive)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Deleted {Count} expired tokens", deletedCount);
        }
    }
}
