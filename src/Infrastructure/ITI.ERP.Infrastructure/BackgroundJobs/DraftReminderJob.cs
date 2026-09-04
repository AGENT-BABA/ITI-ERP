using Hangfire;
using Microsoft.Extensions.Logging;

namespace ITI.ERP.Infrastructure.BackgroundJobs
{
    public class DraftReminderJob : IRecurringJob
    {
        private readonly ILogger<DraftReminderJob> _logger;

        public DraftReminderJob(ILogger<DraftReminderJob> logger)
        {
            _logger = logger;
        }

        public string JobId => "draft-reminder";

        public string CronExpression => Cron.Daily(9, 0);

        public string Queue => "default";

        public Task Execute()
        {
            // TODO: Implement draft reminder when Invoice and Notification entities are added to IApplicationDbContext
            _logger.LogInformation("Draft reminder job placeholder - not yet implemented");
            return Task.CompletedTask;
        }
    }
}
