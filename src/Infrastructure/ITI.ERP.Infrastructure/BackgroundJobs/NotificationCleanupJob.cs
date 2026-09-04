using Hangfire;
using Microsoft.Extensions.Logging;

namespace ITI.ERP.Infrastructure.BackgroundJobs
{
    public class NotificationCleanupJob : IRecurringJob
    {
        private readonly ILogger<NotificationCleanupJob> _logger;

        public NotificationCleanupJob(ILogger<NotificationCleanupJob> logger)
        {
            _logger = logger;
        }

        public string JobId => "notification-cleanup";

        public string CronExpression => Cron.Daily(2, 0);

        public string Queue => "default";

        public Task Execute()
        {
            // TODO: Implement notification cleanup when Notification entity is added to IApplicationDbContext
            _logger.LogInformation("Notification cleanup job placeholder - not yet implemented");
            return Task.CompletedTask;
        }
    }
}
