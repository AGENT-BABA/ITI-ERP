using Hangfire;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Domain.Enums;
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

        var expiredStudents = await _context.Students
            .Where(s => s.Status == StudentStatus.Archived
                && s.RetentionUntil != null
                && s.RetentionUntil <= now)
            .Select(s => new { s.Id, s.FirstName, s.LastName, s.RetentionUntil })
            .ToListAsync();

        if (expiredStudents.Count == 0)
        {
            _logger.LogInformation("No archived students past retention found. Cleanup complete.");
            return;
        }

        var totalStudentsDeleted = 0;
        var totalMarksDeleted = 0;
        var totalAttendanceDeleted = 0;
        var errors = 0;

        foreach (var student in expiredStudents)
        {
            _logger.LogInformation(
                "Processing expired student {StudentName} (RetentionUntil: {RetentionUntil})",
                $"{student.FirstName} {student.LastName}", student.RetentionUntil);

            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var marksDeleted = await _context.PracticalMarks
                    .Where(m => m.StudentId == student.Id)
                    .ExecuteDeleteAsync();

                var yearlyMarksDeleted = await _context.YearlyPracticalMarks
                    .Where(m => m.StudentId == student.Id)
                    .ExecuteDeleteAsync();

                var attendanceDeleted = await _context.AttendanceRecords
                    .Where(a => a.StudentId == student.Id)
                    .ExecuteDeleteAsync();

                await _context.Students
                    .Where(s => s.Id == student.Id)
                    .ExecuteDeleteAsync();

                await _context.SaveChangesAsync(CancellationToken.None);
                await transaction.CommitAsync();

                totalStudentsDeleted++;
                totalMarksDeleted += marksDeleted + yearlyMarksDeleted;
                totalAttendanceDeleted += attendanceDeleted;

                _logger.LogDebug(
                    "Deleted student {StudentId} ({StudentName}): {Marks} marks, {Attendance} attendance records",
                    student.Id, $"{student.FirstName} {student.LastName}", marksDeleted + yearlyMarksDeleted, attendanceDeleted);
            }
            catch (Exception ex)
            {
                errors++;
                _logger.LogError(ex, "Failed to delete student {StudentId} during retention cleanup", student.Id);
            }
        }

        await _auditService.LogAsync(
            AuditAction.RetentionCleanup,
            "RetentionCleanup",
            null,
            new
            {
                StudentsEvaluated = expiredStudents.Count,
                StudentsDeleted = totalStudentsDeleted,
                MarksDeleted = totalMarksDeleted,
                AttendanceDeleted = totalAttendanceDeleted,
                Errors = errors
            },
            null,
            CancellationToken.None);

        await _context.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation(
            "Retention cleanup complete. Students evaluated: {Evaluated}, Deleted: {Deleted}, " +
            "Marks deleted: {Marks}, Attendance deleted: {Attendance}, Errors: {Errors}",
            expiredStudents.Count, totalStudentsDeleted, totalMarksDeleted, totalAttendanceDeleted, errors);
    }
}
