using ITI.ERP.Application.Common.Helpers;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Attendance;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly ITradeAccessService _tradeAccess;

    public AttendanceService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        ITradeAccessService tradeAccess)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _tradeAccess = tradeAccess;
    }

    private async Task<List<Guid>> GetActiveBatchIdsForSessionAsync(Guid tradeId, Guid academicSessionId, CancellationToken ct)
    {
        var targetSession = await _context.AcademicSessions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == academicSessionId, ct);
        if (targetSession is null)
            return new List<Guid>();

        var allBatches = await _context.Batches
            .AsNoTracking()
            .Include(b => b.Trade)
            .Include(b => b.StartAcademicSession)
            .Where(b => b.TradeId == tradeId && !b.IsDeleted)
            .ToListAsync(ct);

        return allBatches
            .Where(b => BatchYearLevelCalculator.IsBatchActiveInSession(b, b.Trade, targetSession))
            .Select(b => b.Id)
            .ToList();
    }

    public async Task<Result<List<AttendanceRecordDto>>> GetAttendanceByDateAsync(Guid tradeId, DateTime date, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidateTradeAccessAsync(tradeId, ct);
        if (!accessCheck.IsSuccess)
            return Result<List<AttendanceRecordDto>>.Failure(accessCheck.Error!);

        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

        if (instituteId is null || academicSessionId is null)
            return Result<List<AttendanceRecordDto>>.Failure("Institute or Academic Session not found.");

        var dateOnly = date.Date;
        var effectiveBatchId = _tradeAccess.EffectiveBatchId;

        var recordsQuery = _context.AttendanceRecords
            .AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.MarkedByUser)
            .Where(a =>
                a.InstituteId == instituteId &&
                a.AcademicSessionId == academicSessionId &&
                a.TradeId == tradeId &&
                a.Date == dateOnly);

        if (_tradeAccess.IsTradeHead && effectiveBatchId.HasValue)
            recordsQuery = recordsQuery.Where(a => a.BatchId == effectiveBatchId.Value);

        var records = await recordsQuery
            .OrderBy(a => a.Student.LastName)
            .ThenBy(a => a.Student.FirstName)
            .Select(a => new AttendanceRecordDto
            {
                Id = a.Id,
                InstituteId = a.InstituteId,
                AcademicSessionId = a.AcademicSessionId,
                TradeId = a.TradeId,
                StudentId = a.StudentId,
                StudentName = $"{a.Student.FirstName} {a.Student.LastName}",
                RollNumber = a.Student.RollNumber,
                Date = a.Date,
                Status = a.Status,
                Remarks = a.Remarks,
                MarkedByUserName = $"{a.MarkedByUser.FirstName} {a.MarkedByUser.LastName}",
                IsLocked = a.IsLocked,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(ct);

        if (records.Count == 0)
        {
            var activeBatchIds = await GetActiveBatchIdsForSessionAsync(tradeId, academicSessionId.Value, ct);

            var studentsQuery = _context.Students
                .AsNoTracking()
                .Where(s =>
                    s.InstituteId == instituteId &&
                    s.TradeId == tradeId &&
                    s.BatchId.HasValue && activeBatchIds.Contains(s.BatchId.Value) &&
                    s.Status == StudentStatus.Active);

            if (_tradeAccess.IsTradeHead && effectiveBatchId.HasValue)
                studentsQuery = studentsQuery.Where(s => s.BatchId == effectiveBatchId.Value);

            var students = await studentsQuery
                .OrderBy(s => s.RollNumber)
                .ThenBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .Select(s => new AttendanceRecordDto
                {
                    Id = Guid.Empty,
                    InstituteId = instituteId.Value,
                    AcademicSessionId = academicSessionId.Value,
                    TradeId = tradeId,
                    StudentId = s.Id,
                    StudentName = $"{s.FirstName} {s.LastName}",
                    RollNumber = s.RollNumber,
                    Date = dateOnly,
                    Status = AttendanceStatus.Present,
                    IsLocked = false,
                    CreatedAt = DateTime.UtcNow
                })
                .ToListAsync(ct);

            return Result<List<AttendanceRecordDto>>.Success(students);
        }

        return Result<List<AttendanceRecordDto>>.Success(records);
    }

    public async Task<Result<AttendanceSummaryDto>> GetAttendanceSummaryAsync(Guid tradeId, DateTime date, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidateTradeAccessAsync(tradeId, ct);
        if (!accessCheck.IsSuccess)
            return Result<AttendanceSummaryDto>.Failure(accessCheck.Error!);

        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

        if (instituteId is null || academicSessionId is null)
            return Result<AttendanceSummaryDto>.Failure("Institute or Academic Session not found.");

        var dateOnly = date.Date;

        var trade = await _context.Trades
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.InstituteId == instituteId, ct);

        if (trade is null)
            return Result<AttendanceSummaryDto>.Failure("Trade not found.");

        var activeBatchIds = await GetActiveBatchIdsForSessionAsync(tradeId, academicSessionId.Value, ct);

        var totalStudentsQuery = _context.Students
            .AsNoTracking()
            .Where(s =>
                s.InstituteId == instituteId &&
                s.TradeId == tradeId &&
                s.BatchId.HasValue && activeBatchIds.Contains(s.BatchId.Value) &&
                s.Status == StudentStatus.Active);

        var recordsQuery = _context.AttendanceRecords
            .AsNoTracking()
            .Where(a =>
                a.InstituteId == instituteId &&
                a.AcademicSessionId == academicSessionId &&
                a.TradeId == tradeId &&
                a.Date == dateOnly);

        if (_tradeAccess.IsTradeHead && _tradeAccess.EffectiveBatchId.HasValue)
        {
            var batchId = _tradeAccess.EffectiveBatchId.Value;
            totalStudentsQuery = totalStudentsQuery.Where(s => s.BatchId == batchId);
            recordsQuery = recordsQuery.Where(a => a.BatchId == batchId);
        }

        var totalStudents = await totalStudentsQuery.CountAsync(ct);
        var records = await recordsQuery.ToListAsync(ct);

        var summary = new AttendanceSummaryDto
        {
            TradeId = tradeId,
            TradeName = trade.Name,
            TradeCode = trade.Code,
            Date = dateOnly,
            TotalStudents = totalStudents,
            PresentCount = records.Count(r => r.Status == AttendanceStatus.Present),
            AbsentCount = records.Count(r => r.Status == AttendanceStatus.Absent),
            LateCount = records.Count(r => r.Status == AttendanceStatus.Late),
            CLCount = records.Count(r => r.Status == AttendanceStatus.CL),
            ELCount = records.Count(r => r.Status == AttendanceStatus.EL),
            MLCount = records.Count(r => r.Status == AttendanceStatus.ML),
            HOCount = records.Count(r => r.Status == AttendanceStatus.HO),
            IsLocked = records.Any() && records.All(r => r.IsLocked),
            IsMarked = records.Any()
        };

        return Result<AttendanceSummaryDto>.Success(summary);
    }

    public async Task<Result<List<AttendanceSummaryDto>>> GetAttendanceSummaryRangeAsync(Guid tradeId, DateTime fromDate, DateTime toDate, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidateTradeAccessAsync(tradeId, ct);
        if (!accessCheck.IsSuccess)
            return Result<List<AttendanceSummaryDto>>.Failure(accessCheck.Error!);

        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

        if (instituteId is null || academicSessionId is null)
            return Result<List<AttendanceSummaryDto>>.Failure("Institute or Academic Session not found.");

        var fromDateOnly = fromDate.Date;
        var toDateOnly = toDate.Date;

        var trade = await _context.Trades
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.InstituteId == instituteId, ct);

        if (trade is null)
            return Result<List<AttendanceSummaryDto>>.Failure("Trade not found.");

        var activeBatchIdsForRange = await GetActiveBatchIdsForSessionAsync(tradeId, academicSessionId.Value, ct);

        var totalStudentsQuery = _context.Students
            .AsNoTracking()
            .Where(s =>
                s.InstituteId == instituteId &&
                s.TradeId == tradeId &&
                s.BatchId.HasValue && activeBatchIdsForRange.Contains(s.BatchId.Value) &&
                s.Status == StudentStatus.Active);

        var recordsQuery = _context.AttendanceRecords
            .AsNoTracking()
            .Where(a =>
                a.InstituteId == instituteId &&
                a.AcademicSessionId == academicSessionId &&
                a.TradeId == tradeId &&
                a.Date >= fromDateOnly &&
                a.Date <= toDateOnly);

        if (_tradeAccess.IsTradeHead && _tradeAccess.EffectiveBatchId.HasValue)
        {
            var batchId = _tradeAccess.EffectiveBatchId.Value;
            totalStudentsQuery = totalStudentsQuery.Where(s => s.BatchId == batchId);
            recordsQuery = recordsQuery.Where(a => a.BatchId == batchId);
        }

        var totalStudents = await totalStudentsQuery.CountAsync(ct);

        var records = await recordsQuery
            .GroupBy(a => a.Date)
            .Select(g => new AttendanceSummaryDto
            {
                TradeId = tradeId,
                TradeName = trade.Name,
                TradeCode = trade.Code,
                Date = g.Key,
                TotalStudents = totalStudents,
                PresentCount = g.Count(r => r.Status == AttendanceStatus.Present),
                AbsentCount = g.Count(r => r.Status == AttendanceStatus.Absent),
                LateCount = g.Count(r => r.Status == AttendanceStatus.Late),
                CLCount = g.Count(r => r.Status == AttendanceStatus.CL),
                ELCount = g.Count(r => r.Status == AttendanceStatus.EL),
                MLCount = g.Count(r => r.Status == AttendanceStatus.ML),
                HOCount = g.Count(r => r.Status == AttendanceStatus.HO),
                IsLocked = g.All(r => r.IsLocked),
                IsMarked = true
            })
            .OrderByDescending(s => s.Date)
            .ToListAsync(ct);

        return Result<List<AttendanceSummaryDto>>.Success(records);
    }

    public async Task<Result<AttendanceStudentSummaryDto>> GetStudentAttendanceSummaryAsync(Guid studentId, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(studentId, ct);
        if (!accessCheck.IsSuccess)
            return Result<AttendanceStudentSummaryDto>.Failure(accessCheck.Error!);

        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<AttendanceStudentSummaryDto>.Failure("Institute not found.");

        var student = await _context.Students
            .AsNoTracking()
            .Include(s => s.Trade)
            .FirstOrDefaultAsync(s => s.Id == studentId && s.InstituteId == instituteId, ct);

        if (student is null)
            return Result<AttendanceStudentSummaryDto>.Failure("Student not found.");

        var records = await _context.AttendanceRecords
            .AsNoTracking()
            .Where(a =>
                a.InstituteId == instituteId &&
                a.StudentId == studentId)
            .ToListAsync(ct);

        var totalWorkingDays = records.Count;
        var presentDays = records.Count(r => r.Status == AttendanceStatus.Present);
        var lateDays = records.Count(r => r.Status == AttendanceStatus.Late);
        var attendedDays = presentDays + lateDays;
        var percentage = totalWorkingDays > 0 ? Math.Round((decimal)attendedDays / totalWorkingDays * 100, 2) : 0;

        var summary = new AttendanceStudentSummaryDto
        {
            StudentId = studentId,
            StudentName = $"{student.FirstName} {student.LastName}",
            RollNumber = student.RollNumber,
            TradeCode = student.Trade?.Code,
            TotalWorkingDays = totalWorkingDays,
            PresentDays = presentDays,
            AbsentDays = records.Count(r => r.Status == AttendanceStatus.Absent),
            LateDays = lateDays,
            CLDays = records.Count(r => r.Status == AttendanceStatus.CL),
            ELDays = records.Count(r => r.Status == AttendanceStatus.EL),
            MLDays = records.Count(r => r.Status == AttendanceStatus.ML),
            HODays = records.Count(r => r.Status == AttendanceStatus.HO),
            AttendancePercentage = percentage
        };

        return Result<AttendanceStudentSummaryDto>.Success(summary);
    }

    public async Task<Result<AttendanceTradeReportDto>> GetTradeAttendanceReportAsync(Guid tradeId, DateTime fromDate, DateTime toDate, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidateTradeAccessAsync(tradeId, ct);
        if (!accessCheck.IsSuccess)
            return Result<AttendanceTradeReportDto>.Failure(accessCheck.Error!);

        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

        if (instituteId is null || academicSessionId is null)
            return Result<AttendanceTradeReportDto>.Failure("Institute or Academic Session not found.");

        var trade = await _context.Trades
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tradeId && t.InstituteId == instituteId, ct);

        if (trade is null)
            return Result<AttendanceTradeReportDto>.Failure("Trade not found.");

        var activeBatchIdsForReport = await GetActiveBatchIdsForSessionAsync(tradeId, academicSessionId.Value, ct);

        var studentsQuery = _context.Students
            .AsNoTracking()
            .Where(s =>
                s.InstituteId == instituteId &&
                s.TradeId == tradeId &&
                s.BatchId.HasValue && activeBatchIdsForReport.Contains(s.BatchId.Value) &&
                s.Status == StudentStatus.Active);

        var fromDateOnly = fromDate.Date;
        var toDateOnly = toDate.Date;

        var recordsQuery = _context.AttendanceRecords
            .AsNoTracking()
            .Where(a =>
                a.InstituteId == instituteId &&
                a.TradeId == tradeId &&
                a.Date >= fromDateOnly &&
                a.Date <= toDateOnly);

        if (_tradeAccess.IsTradeHead && _tradeAccess.EffectiveBatchId.HasValue)
        {
            var batchId = _tradeAccess.EffectiveBatchId.Value;
            studentsQuery = studentsQuery.Where(s => s.BatchId == batchId);
            recordsQuery = recordsQuery.Where(a => a.BatchId == batchId);
        }

        var students = await studentsQuery
            .OrderBy(s => s.RollNumber)
            .ToListAsync(ct);

        var records = await recordsQuery.ToListAsync(ct);

        var totalWorkingDays = records.Select(r => r.Date).Distinct().Count();

        var studentSummaries = students.Select(s =>
        {
            var studentRecords = records.Where(r => r.StudentId == s.Id).ToList();
            var present = studentRecords.Count(r => r.Status == AttendanceStatus.Present);
            var late = studentRecords.Count(r => r.Status == AttendanceStatus.Late);
            var attended = present + late;
            var percentage = totalWorkingDays > 0 ? Math.Round((decimal)attended / totalWorkingDays * 100, 2) : 0;

            return new AttendanceStudentSummaryDto
            {
                StudentId = s.Id,
                StudentName = $"{s.FirstName} {s.LastName}",
                RollNumber = s.RollNumber,
                TradeCode = trade.Code,
                TotalWorkingDays = totalWorkingDays,
                PresentDays = present,
                AbsentDays = studentRecords.Count(r => r.Status == AttendanceStatus.Absent),
                LateDays = late,
                CLDays = studentRecords.Count(r => r.Status == AttendanceStatus.CL),
                ELDays = studentRecords.Count(r => r.Status == AttendanceStatus.EL),
                MLDays = studentRecords.Count(r => r.Status == AttendanceStatus.ML),
                HODays = studentRecords.Count(r => r.Status == AttendanceStatus.HO),
                AttendancePercentage = percentage
            };
        }).ToList();

        var report = new AttendanceTradeReportDto
        {
            TradeId = tradeId,
            TradeName = trade.Name,
            TradeCode = trade.Code,
            FromDate = fromDateOnly,
            ToDate = toDateOnly,
            TotalWorkingDays = totalWorkingDays,
            Students = studentSummaries
        };

        return Result<AttendanceTradeReportDto>.Success(report);
    }

    public async Task<Result> MarkAttendanceAsync(MarkAttendanceRequest request, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);
        var userId = _currentUserService.UserId;

        if (instituteId is null || academicSessionId is null || userId is null)
            return Result.Failure("Institute, Academic Session, or User not found.");

        if (_tradeAccess.IsTradeHead)
        {
            if (!_tradeAccess.EffectiveTradeId.HasValue)
                return Result.Failure("TradeHead trade assignment is not configured.");
            request.TradeId = _tradeAccess.EffectiveTradeId.Value;
        }

        var accessCheck = await _tradeAccess.ValidateTradeAccessAsync(request.TradeId, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var trade = await _context.Trades
            .FirstOrDefaultAsync(t => t.Id == request.TradeId && t.InstituteId == instituteId, ct);

        if (trade is null)
            return Result.Failure("Trade not found.");

        if (trade.DraftStatus == Domain.Enums.DraftStatus.Archived)
            return Result.Failure("Cannot mark attendance for an archived trade.");

        var dateOnly = request.Date.Date;
        var effectiveBatchId = _tradeAccess.IsTradeHead ? _tradeAccess.EffectiveBatchId : null;

        if (effectiveBatchId.HasValue)
        {
            var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == effectiveBatchId.Value, ct);
            if (batch is null || !batch.IsActive)
                return Result.Failure("Cannot mark attendance for an archived batch.");
        }

        var existingRecordsQuery = _context.AttendanceRecords
            .Where(a =>
                a.InstituteId == instituteId &&
                a.AcademicSessionId == academicSessionId &&
                a.TradeId == request.TradeId &&
                a.Date == dateOnly);

        if (effectiveBatchId.HasValue)
            existingRecordsQuery = existingRecordsQuery.Where(a => a.BatchId == effectiveBatchId.Value);

        var existingRecords = await existingRecordsQuery.ToListAsync(ct);

        if (existingRecords.Any() && existingRecords.All(r => r.IsLocked))
            return Result.Failure("Attendance for this date is locked and cannot be modified.");

        var studentIds = request.Students.Select(s => s.StudentId).ToList();

        var validStudentsQuery = _context.Students
            .Where(s =>
                studentIds.Contains(s.Id) &&
                s.InstituteId == instituteId &&
                s.AcademicSessionId == academicSessionId &&
                s.TradeId == request.TradeId &&
                s.Status == StudentStatus.Active);

        if (effectiveBatchId.HasValue)
            validStudentsQuery = validStudentsQuery.Where(s => s.BatchId == effectiveBatchId.Value);

        var validStudents = await validStudentsQuery
            .Select(s => new { s.Id, s.BatchId })
            .ToListAsync(ct);

        var invalidIds = studentIds.Except(validStudents.Select(s => s.Id)).ToList();
        if (invalidIds.Any())
            return Result.Failure($"Invalid or inactive student IDs: {string.Join(", ", invalidIds)}");

        foreach (var item in request.Students)
        {
            var existing = existingRecords.FirstOrDefault(a => a.StudentId == item.StudentId);

            if (existing is not null)
            {
                if (existing.IsLocked)
                    continue;

                existing.Status = item.Status;
                existing.Remarks = item.Remarks;
                existing.MarkedBy = userId.Value;
            }
            else
            {
                var record = new AttendanceRecord
                {
                    InstituteId = instituteId.Value,
                    AcademicSessionId = academicSessionId.Value,
                    TradeId = request.TradeId,
                    BatchId = effectiveBatchId ?? validStudents.FirstOrDefault(s => s.Id == item.StudentId)?.BatchId ?? Guid.Empty,
                    StudentId = item.StudentId,
                    Date = dateOnly,
                    Status = item.Status,
                    Remarks = item.Remarks,
                    MarkedBy = userId.Value
                };

                await _context.AttendanceRecords.AddAsync(record, ct);
            }
        }

        var deletedRecords = existingRecords
            .Where(a => !a.IsLocked && !request.Students.Any(s => s.StudentId == a.StudentId))
            .ToList();

        _context.AttendanceRecords.RemoveRange(deletedRecords);

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.AttendanceMark, nameof(AttendanceRecord), null,
            new { TradeId = request.TradeId, Date = dateOnly, StudentCount = request.Students.Count },
            new { MarkedBy = userId }, ct);

        return Result.Success();
    }

    public async Task<Result> LockAttendanceAsync(Guid tradeId, DateTime date, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidateTradeAccessAsync(tradeId, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);
        var userId = _currentUserService.UserId;

        if (instituteId is null || academicSessionId is null || userId is null)
            return Result.Failure("Institute, Academic Session, or User not found.");

        var dateOnly = date.Date;

        var recordsQuery = _context.AttendanceRecords
            .Where(a =>
                a.InstituteId == instituteId &&
                a.AcademicSessionId == academicSessionId &&
                a.TradeId == tradeId &&
                a.Date == dateOnly);

        if (_tradeAccess.IsTradeHead && _tradeAccess.EffectiveBatchId.HasValue)
            recordsQuery = recordsQuery.Where(a => a.BatchId == _tradeAccess.EffectiveBatchId.Value);

        var records = await recordsQuery.ToListAsync(ct);

        if (!records.Any())
            return Result.Failure("No attendance records found for this date.");

        if (records.All(r => r.IsLocked))
            return Result.Failure("Attendance is already locked.");

        foreach (var record in records)
        {
            record.IsLocked = true;
            record.LockedAt = DateTime.UtcNow;
            record.LockedBy = userId.Value;
        }

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.DraftLock, nameof(AttendanceRecord), null,
            new { TradeId = tradeId, Date = dateOnly, RecordCount = records.Count }, null, ct);

        return Result.Success();
    }

    public async Task<Result> UnlockAttendanceAsync(Guid tradeId, DateTime date, string reason, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidateTradeAccessAsync(tradeId, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

        if (instituteId is null || academicSessionId is null)
            return Result.Failure("Institute or Academic Session not found.");

        var dateOnly = date.Date;

        var recordsQuery = _context.AttendanceRecords
            .Where(a =>
                a.InstituteId == instituteId &&
                a.AcademicSessionId == academicSessionId &&
                a.TradeId == tradeId &&
                a.Date == dateOnly);

        if (_tradeAccess.IsTradeHead && _tradeAccess.EffectiveBatchId.HasValue)
            recordsQuery = recordsQuery.Where(a => a.BatchId == _tradeAccess.EffectiveBatchId.Value);

        var records = await recordsQuery.ToListAsync(ct);

        if (!records.Any())
            return Result.Failure("No attendance records found for this date.");

        if (records.All(r => !r.IsLocked))
            return Result.Failure("Attendance is already unlocked.");

        foreach (var record in records)
        {
            record.IsLocked = false;
            record.LockedAt = null;
            record.LockedBy = null;
        }

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.DraftUnlock, nameof(AttendanceRecord), null,
            new { TradeId = tradeId, Date = dateOnly, RecordCount = records.Count, reason }, null, ct);

        return Result.Success();
    }
}
