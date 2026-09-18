using ITI.ERP.Application.Common.Helpers;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Holiday;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class HolidayService : IHolidayService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public HolidayService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<List<HolidayDto>>> GetHolidaysAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        if (!isSuperAdmin && (instituteId is null || academicSessionId is null))
            return Result<List<HolidayDto>>.Failure("Institute or Academic Session not found.");

        var fromDate = from.Date;
        var toDate = to.Date;

        var query = _context.Holidays
            .AsNoTracking()
            .Include(h => h.AcademicSession)
            .Where(h =>
                (!isSuperAdmin || h.InstituteId == instituteId) &&
                (!academicSessionId.HasValue || h.AcademicSessionId == academicSessionId) &&
                h.Date >= fromDate &&
                h.Date <= toDate);

        if (!isSuperAdmin)
            query = query.Where(h => h.InstituteId == instituteId && h.AcademicSessionId == academicSessionId);

        var holidays = await query
            .OrderBy(h => h.Date)
            .Select(h => new HolidayDto
            {
                Id = h.Id,
                Date = h.Date,
                Name = h.Name,
                SessionYear = h.AcademicSession.SessionYear,
                AcademicSessionId = h.AcademicSessionId
            })
            .ToListAsync(ct);

        return Result<List<HolidayDto>>.Success(holidays);
    }

    public async Task<Result<HolidayDto>> CreateHolidayAsync(CreateHolidayRequest request, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);
        var userId = _currentUserService.UserId;
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        if (!isSuperAdmin && (instituteId is null || academicSessionId is null))
            return Result<HolidayDto>.Failure("Institute or Academic Session not found.");

        if (isSuperAdmin && (!instituteId.HasValue || !academicSessionId.HasValue))
            return Result<HolidayDto>.Failure("Institute and Academic Session are required.");

        var dateOnly = request.Date.Date;

        var session = await _context.AcademicSessions
            .FirstOrDefaultAsync(s => s.Id == academicSessionId!.Value && !s.IsDeleted, ct);

        if (session is null)
            return Result<HolidayDto>.Failure("Academic Session not found.");

        if (dateOnly < session.StartDate.Date || dateOnly > session.EndDate.Date)
            return Result<HolidayDto>.Failure($"Date must be within the session range ({session.StartDate:dd MMM yyyy} to {session.EndDate:dd MMM yyyy}).");

        var exists = await _context.Holidays
            .AnyAsync(h =>
                h.InstituteId == instituteId!.Value &&
                h.AcademicSessionId == academicSessionId!.Value &&
                h.Date == dateOnly &&
                !h.IsDeleted, ct);

        if (exists)
            return Result<HolidayDto>.Failure("A holiday already exists for this date.");

        var holiday = new Holiday
        {
            InstituteId = instituteId!.Value,
            AcademicSessionId = academicSessionId!.Value,
            Date = dateOnly,
            Name = request.Name
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await _context.Holidays.AddAsync(holiday, ct);
            await _context.SaveChangesAsync(ct);

            await AutoMarkAttendanceHO(instituteId.Value, academicSessionId.Value, dateOnly, userId!.Value, ct);

            await _auditService.LogAsync(AuditAction.Create, nameof(Holiday), holiday.Id,
                new { holiday.Date, holiday.Name }, new { CreatedBy = userId }, ct);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return Result<HolidayDto>.Success(new HolidayDto
        {
            Id = holiday.Id,
            Date = holiday.Date,
            Name = holiday.Name,
            SessionYear = session.SessionYear,
            AcademicSessionId = holiday.AcademicSessionId
        });
    }

    public async Task<Result> DeleteHolidayAsync(Guid id, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);
        var userId = _currentUserService.UserId;
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        if (!isSuperAdmin && (instituteId is null || academicSessionId is null))
            return Result.Failure("Institute or Academic Session not found.");

        var holidayQuery = _context.Holidays
            .Where(h => h.Id == id && !h.IsDeleted);

        if (!isSuperAdmin)
            holidayQuery = holidayQuery.Where(h =>
                h.InstituteId == instituteId!.Value &&
                h.AcademicSessionId == academicSessionId!.Value);

        var holiday = await holidayQuery.FirstOrDefaultAsync(ct);

        if (holiday is null)
            return Result.Failure("Holiday not found.");

        var dateOnly = holiday.Date.Date;

        holiday.IsDeleted = true;

        var hoRecordsQuery = _context.AttendanceRecords
            .Where(a =>
                a.Date == dateOnly &&
                a.Status == AttendanceStatus.HO &&
                !a.IsLocked);

        if (!isSuperAdmin)
            hoRecordsQuery = hoRecordsQuery.Where(a =>
                a.InstituteId == instituteId!.Value &&
                a.AcademicSessionId == academicSessionId!.Value);

        var hoRecords = await hoRecordsQuery.ToListAsync(ct);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            holiday.IsDeleted = true;

            if (hoRecords.Count > 0)
                _context.AttendanceRecords.RemoveRange(hoRecords);

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Delete, nameof(Holiday), id,
                new { holiday.Date, holiday.Name, DeletedHOMarks = hoRecords.Count },
                new { DeletedBy = userId }, ct);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return Result.Success();
    }

    private async Task AutoMarkAttendanceHO(
        Guid instituteId, Guid academicSessionId, DateTime dateOnly, Guid userId, CancellationToken ct)
    {
        var activeStudentIds = await _context.Students
            .Where(s =>
                s.InstituteId == instituteId &&
                s.AcademicSessionId == academicSessionId &&
                s.Status == StudentStatus.Active)
            .Select(s => s.Id)
            .ToListAsync(ct);

        if (activeStudentIds.Count == 0) return;

        var existingRecords = await _context.AttendanceRecords
            .Where(a =>
                a.InstituteId == instituteId &&
                a.AcademicSessionId == academicSessionId &&
                a.Date == dateOnly)
            .ToListAsync(ct);

        var existingByStudent = existingRecords.ToDictionary(a => a.StudentId, a => a);

        foreach (var studentId in activeStudentIds)
        {
            if (existingByStudent.TryGetValue(studentId, out var existing))
            {
                if (!existing.IsLocked)
                {
                    existing.Status = AttendanceStatus.HO;
                    existing.MarkedBy = userId;
                }
            }
            else
            {
                var studentInfo = await _context.Students
                    .Where(s => s.Id == studentId)
                    .Select(s => new { s.TradeId, s.BatchId })
                    .FirstOrDefaultAsync(ct);

                if (studentInfo is null || studentInfo.TradeId == Guid.Empty) continue;
                if (studentInfo.BatchId is null) continue;

                var record = new AttendanceRecord
                {
                    InstituteId = instituteId,
                    AcademicSessionId = academicSessionId,
                    TradeId = studentInfo.TradeId,
                    BatchId = studentInfo.BatchId.Value,
                    StudentId = studentId,
                    Date = dateOnly,
                    Status = AttendanceStatus.HO,
                    MarkedBy = userId
                };

                await _context.AttendanceRecords.AddAsync(record, ct);
            }
        }

        await _context.SaveChangesAsync(ct);
    }
}
