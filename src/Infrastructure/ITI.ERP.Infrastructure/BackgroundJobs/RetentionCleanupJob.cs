using Hangfire;
using ITI.ERP.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITI.ERP.Infrastructure.BackgroundJobs;

public class RetentionCleanupJob : IRecurringJob
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<RetentionCleanupJob> _logger;

    public RetentionCleanupJob(
        IApplicationDbContext context,
        IAuditService auditService,
        ILogger<RetentionCleanupJob> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public string JobId => "retention-cleanup";
    public string CronExpression => Cron.Daily(4, 0);
    public string Queue => "default";

    public async Task Execute()
    {
        _logger.LogInformation("Retention cleanup job started at {Time}", DateTime.UtcNow);

        var now = DateTime.UtcNow;

        var expiredSessions = await _context.AcademicSessions
            .Where(s => !s.IsDeleted && s.EndDate.AddYears(1) <= now)
            .ToListAsync();

        if (expiredSessions.Count == 0)
        {
            _logger.LogInformation("No expired academic sessions found. Retention cleanup complete.");
            return;
        }

        var totalStudentsDeleted = 0;
        var totalMarksDeleted = 0;
        var totalAttendanceDeleted = 0;
        var errors = 0;

        foreach (var session in expiredSessions)
        {
            _logger.LogInformation(
                "Processing expired session {SessionYear} (EndDate: {EndDate}, RetentionExpiry: {Expiry})",
                session.SessionYear, session.EndDate, session.EndDate.AddYears(1));

            var studentIds = await _context.Students
                .Where(s => s.AcademicSessionId == session.Id && !s.IsDeleted)
                .Select(s => s.Id)
                .ToListAsync();

            foreach (var studentId in studentIds)
            {
                try
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();

                    var marksDeleted = await _context.PracticalMarks
                        .Where(m => m.StudentId == studentId)
                        .ExecuteDeleteAsync();

                    var yearlyMarksDeleted = await _context.YearlyPracticalMarks
                        .Where(m => m.StudentId == studentId)
                        .ExecuteDeleteAsync();

                    var attendanceDeleted = await _context.AttendanceRecords
                        .Where(a => a.StudentId == studentId)
                        .ExecuteDeleteAsync();

                    await _context.Students
                        .Where(s => s.Id == studentId)
                        .ExecuteDeleteAsync();

                    await _context.SaveChangesAsync(CancellationToken.None);
                    await transaction.CommitAsync();

                    totalStudentsDeleted++;
                    totalMarksDeleted += marksDeleted + yearlyMarksDeleted;
                    totalAttendanceDeleted += attendanceDeleted;

                    _logger.LogDebug(
                        "Deleted student {StudentId}: {Marks} marks, {Attendance} attendance records",
                        studentId, marksDeleted + yearlyMarksDeleted, attendanceDeleted);
                }
                catch (Exception ex)
                {
                    errors++;
                    _logger.LogError(ex, "Failed to delete student {StudentId} during retention cleanup", studentId);
                }
            }
        }

        await _auditService.LogAsync(
            Domain.Enums.AuditAction.RetentionCleanup,
            "RetentionCleanup",
            null,
            new
            {
                SessionsProcessed = expiredSessions.Count,
                StudentsDeleted = totalStudentsDeleted,
                MarksDeleted = totalMarksDeleted,
                AttendanceDeleted = totalAttendanceDeleted,
                Errors = errors
            },
            null,
            CancellationToken.None);

        await _context.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation(
            "Retention cleanup complete. Sessions: {Sessions}, Students deleted: {Students}, " +
            "Marks deleted: {Marks}, Attendance deleted: {Attendance}, Errors: {Errors}",
            expiredSessions.Count, totalStudentsDeleted, totalMarksDeleted, totalAttendanceDeleted, errors);
    }
}
