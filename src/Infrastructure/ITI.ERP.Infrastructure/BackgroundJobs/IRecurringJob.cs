namespace ITI.ERP.Infrastructure.BackgroundJobs
{
    public interface IRecurringJob
    {
        string JobId { get; }
        string CronExpression { get; }
        string Queue { get; }
        Task Execute();
    }
}
