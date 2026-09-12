using Hangfire;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITI.ERP.Infrastructure.BackgroundJobs;

public class BatchAutoPurgeJob : IRecurringJob
{
    private readonly IBatchService _batchService;
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<BatchAutoPurgeJob> _logger;

    public BatchAutoPurgeJob(
        IBatchService batchService,
        IApplicationDbContext context,
        IAuditService auditService,
        ILogger<BatchAutoPurgeJob> logger)
    {
        _batchService = batchService;
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public string JobId => "batch-auto-purge";
    public string CronExpression => Cron.Daily(5, 0);
    public string Queue => "default";

    public async Task Execute()
    {
        _logger.LogInformation("Batch auto-purge job started at {Time}", DateTime.UtcNow);

        var cutoffDate = DateTime.UtcNow.AddDays(-7);

        var expiredBatches = await _context.Batches
            .Where(b => b.IsDeleted && b.DeletedAt != null && b.DeletedAt < cutoffDate)
            .Select(b => new { b.Id, b.Name, b.DeletedAt })
            .ToListAsync();

        if (expiredBatches.Count == 0)
        {
            _logger.LogInformation("No expired deleted batches found. Purge complete.");
            return;
        }

        var purgedCount = 0;
        var skippedCount = 0;
        var errorCount = 0;

        foreach (var batch in expiredBatches)
        {
            _logger.LogInformation(
                "Processing expired batch {BatchName} (DeletedAt: {DeletedAt})",
                batch.Name, batch.DeletedAt);

            try
            {
                var result = await _batchService.PermanentDeleteBatchInternalAsync(
                    batch.Id, CancellationToken.None);

                if (result.IsSuccess)
                {
                    purgedCount++;
                    var purgeResult = result.Value!;
                    _logger.LogInformation(
                        "Purged batch {BatchName}: {Students} students, " +
                        "{MonthlyPracticals} monthly practicals, " +
                        "{YearlyPracticals} yearly practicals, " +
                        "{UserRoles} user roles deleted. " +
                        "{MonthlySkipped} monthly practicals skipped, " +
                        "{YearlySkipped} yearly practicals skipped.",
                        batch.Name,
                        purgeResult.StudentsDeleted,
                        purgeResult.MonthlyPracticalsDeleted,
                        purgeResult.YearlyPracticalsDeleted,
                        purgeResult.UserRolesDeleted,
                        purgeResult.MonthlyPracticalsSkipped,
                        purgeResult.YearlyPracticalsSkipped);

                    if (purgeResult.SkipReasons.Count > 0)
                    {
                        foreach (var reason in purgeResult.SkipReasons)
                            _logger.LogWarning("Purge skip: {Reason}", reason);
                    }
                }
                else
                {
                    skippedCount++;
                    _logger.LogWarning(
                        "Skipped batch {BatchName}: {Reason}",
                        batch.Name, result.Error);
                }
            }
            catch (Exception ex)
            {
                errorCount++;
                _logger.LogError(ex,
                    "Failed to purge batch {BatchName} ({BatchId})",
                    batch.Name, batch.Id);
            }
        }

        await _auditService.LogAsync(
            AuditAction.RetentionCleanup,
            "BatchAutoPurge",
            null,
            new
            {
                BatchesEvaluated = expiredBatches.Count,
                BatchesPurged = purgedCount,
                BatchesSkipped = skippedCount,
                Errors = errorCount
            },
            null,
            CancellationToken.None);

        await _context.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation(
            "Batch auto-purge complete. Evaluated: {Evaluated}, Purged: {Purged}, " +
            "Skipped: {Skipped}, Errors: {Errors}",
            expiredBatches.Count, purgedCount, skippedCount, errorCount);
    }
}
