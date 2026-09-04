using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Mappers;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.AcademicSession;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class AcademicSessionService : IAcademicSessionService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public AcademicSessionService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<PaginatedList<AcademicSessionDto>>> GetAcademicSessionsAsync(PaginationRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.AcademicSessions
            .AsNoTracking();

        if (!isSuperAdmin) query = query.Where(a => a.InstituteId == _currentUserService.InstituteId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(a => a.SessionYear.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "sessionyear" => request.SortDescending ? query.OrderByDescending(a => a.SessionYear) : query.OrderBy(a => a.SessionYear),
            "startdate" => request.SortDescending ? query.OrderByDescending(a => a.StartDate) : query.OrderBy(a => a.StartDate),
            _ => query.OrderByDescending(a => a.SessionYear)
        };

        var paginatedList = await PaginatedList<AcademicSessionDto>.CreateAsync(
            query.Select(a => a.ToDto()),
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<AcademicSessionDto>>.Success(paginatedList);
    }

    public async Task<Result<AcademicSessionDto>> GetAcademicSessionByIdAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.AcademicSessions
            .AsNoTracking()
            .Where(a => a.Id == id);

        if (!isSuperAdmin) query = query.Where(a => a.InstituteId == _currentUserService.InstituteId);

        var session = await query.FirstOrDefaultAsync(ct);

        if (session is null)
            return Result<AcademicSessionDto>.Failure("Academic session not found.");

        return Result<AcademicSessionDto>.Success(session.ToDto());
    }

    public async Task<Result<AcademicSessionDto>> CreateAcademicSessionAsync(CreateAcademicSessionRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        Guid instituteId;
        if (isSuperAdmin)
        {
            if (request.InstituteId is null)
                return Result<AcademicSessionDto>.Failure("InstituteId is required for SuperAdmin.");

            var instituteExists = await _context.Institutes.AnyAsync(i => i.Id == request.InstituteId.Value && i.IsActive, ct);
            if (!instituteExists)
                return Result<AcademicSessionDto>.Failure("Institute not found or is inactive.");

            instituteId = request.InstituteId.Value;
        }
        else
        {
            if (_currentUserService.InstituteId is null)
                return Result<AcademicSessionDto>.Failure("Institute not found.");

            instituteId = _currentUserService.InstituteId.Value;
        }

        if (await _context.AcademicSessions.AnyAsync(a =>
            a.InstituteId == instituteId && a.SessionYear == request.SessionYear, ct))
            return Result<AcademicSessionDto>.Failure("Academic session with this year already exists.");

        var session = new AcademicSession
        {
            InstituteId = instituteId,
            SessionYear = request.SessionYear?.Trim() ?? string.Empty,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = false,
            IsLocked = false
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await _context.AcademicSessions.AddAsync(session, ct);
            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(Domain.Enums.AuditAction.Create, nameof(AcademicSession), session.Id, null, session, ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return Result<AcademicSessionDto>.Success(session.ToDto());
    }

    public async Task<Result<AcademicSessionDto>> UpdateAcademicSessionAsync(Guid id, UpdateAcademicSessionRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.AcademicSessions
            .Where(a => a.Id == id);

        if (!isSuperAdmin) query = query.Where(a => a.InstituteId == _currentUserService.InstituteId);

        var session = await query.FirstOrDefaultAsync(ct);

        if (session is null)
            return Result<AcademicSessionDto>.Failure("Academic session not found.");

        if (session.IsLocked)
            return Result<AcademicSessionDto>.Failure("Cannot update a locked session.");

        if (request.SessionYear is not null && await _context.AcademicSessions.AnyAsync(a =>
            a.InstituteId == session.InstituteId && a.SessionYear == request.SessionYear && a.Id != id, ct))
            return Result<AcademicSessionDto>.Failure("Academic session with this year already exists.");

        var oldValues = new
        {
            session.SessionYear,
            session.StartDate,
            session.EndDate
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            if (request.SessionYear is not null) session.SessionYear = request.SessionYear;
            if (request.StartDate.HasValue) session.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) session.EndDate = request.EndDate.Value;

            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(Domain.Enums.AuditAction.Update, nameof(AcademicSession), session.Id, oldValues, session, ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return Result<AcademicSessionDto>.Success(session.ToDto());
    }

    public async Task<Result> ActivateSessionAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.AcademicSessions
            .Where(a => a.Id == id);

        if (!isSuperAdmin) query = query.Where(a => a.InstituteId == _currentUserService.InstituteId);

        var session = await query.FirstOrDefaultAsync(ct);

        if (session is null)
            return Result.Failure("Academic session not found.");

        if (session.IsLocked)
            return Result.Failure("Cannot activate a locked session.");

        var activeSessions = await _context.AcademicSessions
            .Where(a => a.InstituteId == session.InstituteId && a.IsActive && a.Id != id)
            .ToListAsync(ct);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            foreach (var activeSession in activeSessions)
            {
                activeSession.IsActive = false;
            }

            session.IsActive = true;

            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(Domain.Enums.AuditAction.Update, nameof(AcademicSession), session.Id, new { IsActive = false }, new { IsActive = true }, ct);
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

    public async Task<Result> LockSessionAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.AcademicSessions
            .Where(a => a.Id == id);

        if (!isSuperAdmin) query = query.Where(a => a.InstituteId == _currentUserService.InstituteId);

        var session = await query.FirstOrDefaultAsync(ct);

        if (session is null)
            return Result.Failure("Academic session not found.");

        if (session.IsLocked)
            return Result.Failure("Session is already locked.");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            session.IsLocked = true;

            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(Domain.Enums.AuditAction.Update, nameof(AcademicSession), session.Id, new { IsLocked = false }, new { IsLocked = true }, ct);
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

    public async Task<Result> DeleteAcademicSessionAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.AcademicSessions
            .Where(a => a.Id == id);

        if (!isSuperAdmin) query = query.Where(a => a.InstituteId == _currentUserService.InstituteId);

        var session = await query.FirstOrDefaultAsync(ct);

        if (session is null)
            return Result.Failure("Academic session not found.");

        if (session.IsLocked)
            return Result.Failure("Cannot delete a locked session.");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var attendanceRecords = await _context.AttendanceRecords.IgnoreQueryFilters()
                .Where(a => a.AcademicSessionId == id).ToListAsync(ct);
            foreach (var r in attendanceRecords) r.IsDeleted = true;

            var monthlyPracticals = await _context.MonthlyPracticals.IgnoreQueryFilters()
                .Where(p => p.AcademicSessionId == id).ToListAsync(ct);
            foreach (var p in monthlyPracticals) p.IsDeleted = true;

            var yearlyPracticals = await _context.YearlyPracticals.IgnoreQueryFilters()
                .Where(p => p.AcademicSessionId == id).ToListAsync(ct);
            foreach (var p in yearlyPracticals) p.IsDeleted = true;

            var students = await _context.Students.IgnoreQueryFilters()
                .Where(s => s.AcademicSessionId == id).ToListAsync(ct);
            foreach (var s in students) s.IsDeleted = true;

            session.IsDeleted = true;

            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(Domain.Enums.AuditAction.Delete, nameof(AcademicSession), session.Id, session, null, ct);
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
}
