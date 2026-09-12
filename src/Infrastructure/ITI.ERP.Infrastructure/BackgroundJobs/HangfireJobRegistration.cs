using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace ITI.ERP.Infrastructure.BackgroundJobs
{
    public static class HangfireJobRegistration
    {
        private static readonly TimeZoneInfo IndiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

        public static void RegisterRecurringJobs(this IServiceProvider serviceProvider)
        {
            var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();

            recurringJobManager.AddOrUpdate<NotificationCleanupJob>(
                "notification-cleanup",
                job => job.Execute(),
                Cron.Daily(2, 0),
                new RecurringJobOptions
                {
                    TimeZone = IndiaTimeZone
                });

            recurringJobManager.AddOrUpdate<ExpiredTokenCleanupJob>(
                "expired-token-cleanup",
                job => job.Execute(),
                Cron.Daily(3, 0),
                new RecurringJobOptions
                {
                    TimeZone = IndiaTimeZone
                });

            recurringJobManager.AddOrUpdate<DraftReminderJob>(
                "draft-reminder",
                job => job.Execute(),
                Cron.Daily(9, 0),
                new RecurringJobOptions
                {
                    TimeZone = IndiaTimeZone
                });

            recurringJobManager.AddOrUpdate<RetentionCleanupJob>(
                "retention-cleanup",
                job => job.Execute(),
                Cron.Daily(4, 0),
                new RecurringJobOptions
                {
                    TimeZone = IndiaTimeZone
                });

            recurringJobManager.AddOrUpdate<PasswordResetTokenCleanupJob>(
                "password-reset-token-cleanup",
                job => job.Execute(),
                Cron.Daily(4, 30),
                new RecurringJobOptions
                {
                    TimeZone = IndiaTimeZone
                });

            recurringJobManager.AddOrUpdate<BatchAutoPurgeJob>(
                "batch-auto-purge",
                job => job.Execute(),
                Cron.Daily(5, 0),
                new RecurringJobOptions
                {
                    TimeZone = IndiaTimeZone
                });
        }
    }
}
