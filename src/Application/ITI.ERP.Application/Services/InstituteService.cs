using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Institute;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class InstituteService : IInstituteService
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public InstituteService(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Result<PaginatedList<InstituteDto>>> GetInstitutesAsync(PaginationRequest request, CancellationToken ct)
    {
        var query = _context.Institutes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(searchTerm) ||
                i.GRNumber.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(i => i.Name) : query.OrderBy(i => i.Name),
            "grnumber" => request.SortDescending ? query.OrderByDescending(i => i.GRNumber) : query.OrderBy(i => i.GRNumber),
            _ => query.OrderBy(i => i.Name)
        };

        var paginatedList = await PaginatedList<InstituteDto>.CreateAsync(
            query.Select(i => new InstituteDto
            {
                Id = i.Id,
                GRNumber = i.GRNumber,
                Name = i.Name,
                Address = i.Address,
                City = i.City,
                District = i.District,
                State = i.State,
                Phone = i.Phone,
                Email = i.Email,
                LogoPath = i.LogoPath,
                IsActive = i.IsActive,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }),
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<InstituteDto>>.Success(paginatedList);
    }

    public async Task<Result<InstituteDto>> GetInstituteByIdAsync(Guid id, CancellationToken ct)
    {
        var institute = await _context.Institutes
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (institute is null)
            return Result<InstituteDto>.Failure("Institute not found.");

        return Result<InstituteDto>.Success(new InstituteDto
        {
            Id = institute.Id,
            GRNumber = institute.GRNumber,
            Name = institute.Name,
            Address = institute.Address,
            City = institute.City,
            District = institute.District,
            State = institute.State,
            Phone = institute.Phone,
            Email = institute.Email,
            LogoPath = institute.LogoPath,
            IsActive = institute.IsActive,
            CreatedAt = institute.CreatedAt,
            UpdatedAt = institute.UpdatedAt
        });
    }

    public async Task<Result<InstituteDto>> CreateInstituteAsync(CreateInstituteRequest request, CancellationToken ct)
    {
        if (await _context.Institutes.AnyAsync(i => i.GRNumber == request.GRNumber, ct))
            return Result<InstituteDto>.Failure("Institute with this GR Number already exists.");

        var institute = new Institute
        {
            GRNumber = request.GRNumber?.Trim() ?? string.Empty,
            Name = request.Name?.Trim() ?? string.Empty,
            Address = request.Address?.Trim(),
            City = request.City?.Trim(),
            District = request.District?.Trim(),
            State = request.State?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            LogoPath = request.LogoPath?.Trim(),
            IsActive = true
        };

        await _context.Institutes.AddAsync(institute, ct);
        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(Domain.Enums.AuditAction.Create, nameof(Institute), institute.Id, null, institute, ct);

        await _context.SaveChangesAsync(ct);

        return Result<InstituteDto>.Success(new InstituteDto
        {
            Id = institute.Id,
            GRNumber = institute.GRNumber,
            Name = institute.Name,
            Address = institute.Address,
            City = institute.City,
            District = institute.District,
            State = institute.State,
            Phone = institute.Phone,
            Email = institute.Email,
            LogoPath = institute.LogoPath,
            IsActive = institute.IsActive,
            CreatedAt = institute.CreatedAt,
            UpdatedAt = institute.UpdatedAt
        });
    }

    public async Task<Result<InstituteDto>> UpdateInstituteAsync(Guid id, UpdateInstituteRequest request, CancellationToken ct)
    {
        var institute = await _context.Institutes.FindAsync(new object[] { id }, ct);
        if (institute is null)
            return Result<InstituteDto>.Failure("Institute not found.");

        if (!string.IsNullOrWhiteSpace(request.GRNumber) &&
            await _context.Institutes.AnyAsync(i => i.GRNumber == request.GRNumber && i.Id != id, ct))
            return Result<InstituteDto>.Failure("Institute with this GR Number already exists.");

        var oldValues = new
        {
            institute.Name,
            institute.Address,
            institute.City,
            institute.District,
            institute.State,
            institute.Phone,
            institute.Email,
            institute.LogoPath,
            institute.IsActive
        };

        if (request.GRNumber is not null) institute.GRNumber = request.GRNumber;
        if (request.Name is not null) institute.Name = request.Name;
        if (request.Address is not null) institute.Address = request.Address;
        if (request.City is not null) institute.City = request.City;
        if (request.District is not null) institute.District = request.District;
        if (request.State is not null) institute.State = request.State;
        if (request.Phone is not null) institute.Phone = request.Phone;
        if (request.Email is not null) institute.Email = request.Email;
        if (request.LogoPath is not null) institute.LogoPath = request.LogoPath;
        if (request.IsActive is not null) institute.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(Domain.Enums.AuditAction.Update, nameof(Institute), institute.Id, oldValues, institute, ct);

        await _context.SaveChangesAsync(ct);

        return Result<InstituteDto>.Success(new InstituteDto
        {
            Id = institute.Id,
            GRNumber = institute.GRNumber,
            Name = institute.Name,
            Address = institute.Address,
            City = institute.City,
            District = institute.District,
            State = institute.State,
            Phone = institute.Phone,
            Email = institute.Email,
            LogoPath = institute.LogoPath,
            IsActive = institute.IsActive,
            CreatedAt = institute.CreatedAt,
            UpdatedAt = institute.UpdatedAt
        });
    }

    public async Task<Result> DeleteInstituteAsync(Guid id, CancellationToken ct)
    {
        var institute = await _context.Institutes.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (institute is null)
            return Result.Failure("Institute not found.");

        var instituteName = institute.Name;
        var instituteGR = institute.GRNumber;

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            // ── Phase 1: Leaf entities (depend on entities in later phases) ──

            // Fetch IDs first — needed by dependent queries below
            var userIds = await _context.Users.IgnoreQueryFilters()
                .Where(u => u.InstituteId == id).Select(u => u.Id).ToListAsync(ct);

            var instituteRoleIds = await _context.Roles.IgnoreQueryFilters()
                .Where(r => r.InstituteId == id).Select(r => r.Id).ToListAsync(ct);

            var practicalMarks = await _context.PracticalMarks.IgnoreQueryFilters()
                .Where(pm => pm.InstituteId == id).ToListAsync(ct);
            _context.PracticalMarks.RemoveRange(practicalMarks);

            var yearlyPracticalMarks = await _context.YearlyPracticalMarks.IgnoreQueryFilters()
                .Where(ym => ym.InstituteId == id).ToListAsync(ct);
            _context.YearlyPracticalMarks.RemoveRange(yearlyPracticalMarks);

            var attendanceRecords = await _context.AttendanceRecords.IgnoreQueryFilters()
                .Where(a => a.InstituteId == id).ToListAsync(ct);
            _context.AttendanceRecords.RemoveRange(attendanceRecords);

            var studentImports = await _context.StudentImportHistories.IgnoreQueryFilters()
                .Where(s => s.InstituteId == id).ToListAsync(ct);
            _context.StudentImportHistories.RemoveRange(studentImports);

            var tradeImports = await _context.TradeImportHistories.IgnoreQueryFilters()
                .Where(t => t.InstituteId == id).ToListAsync(ct);
            _context.TradeImportHistories.RemoveRange(tradeImports);

            var auditLogs = await _context.AuditLogs.IgnoreQueryFilters()
                .Where(a => a.InstituteId == id
                    || (a.UserId.HasValue && userIds.Contains(a.UserId.Value)))
                .ToListAsync(ct);
            _context.AuditLogs.RemoveRange(auditLogs);

            var holidays = await _context.Holidays.IgnoreQueryFilters()
                .Where(h => h.InstituteId == id).ToListAsync(ct);
            _context.Holidays.RemoveRange(holidays);

            var settings = await _context.InstituteSettings.IgnoreQueryFilters()
                .Where(s => s.InstituteId == id).ToListAsync(ct);
            _context.InstituteSettings.RemoveRange(settings);

            if (userIds.Count > 0)
            {
                var refreshTokens = await _context.RefreshTokens.IgnoreQueryFilters()
                    .Where(rt => userIds.Contains(rt.UserId)).ToListAsync(ct);
                _context.RefreshTokens.RemoveRange(refreshTokens);
            }

            if (instituteRoleIds.Count > 0)
            {
                var rolePermissions = await _context.RolePermissions.IgnoreQueryFilters()
                    .Where(rp => instituteRoleIds.Contains(rp.RoleId)).ToListAsync(ct);
                _context.RolePermissions.RemoveRange(rolePermissions);
            }

            await _context.SaveChangesAsync(ct);

            // ── Phase 2: Mid-level entities ──
            // Order: leaf-most first → Batches last (depended on by Students, UserRoles, Attendance, Practicals)
            var monthlyPracticals = await _context.MonthlyPracticals.IgnoreQueryFilters()
                .Where(p => p.InstituteId == id).ToListAsync(ct);
            _context.MonthlyPracticals.RemoveRange(monthlyPracticals);

            var yearlyPracticals = await _context.YearlyPracticals.IgnoreQueryFilters()
                .Where(p => p.InstituteId == id).ToListAsync(ct);
            _context.YearlyPracticals.RemoveRange(yearlyPracticals);

            var students = await _context.Students.IgnoreQueryFilters()
                .Where(s => s.InstituteId == id).ToListAsync(ct);
            _context.Students.RemoveRange(students);

            // UserRoles BEFORE Batches (UserRole.BatchId → Batch with Restrict)
            var userRoles = await _context.UserRoles.IgnoreQueryFilters()
                .Where(ur => ur.InstituteId == id).ToListAsync(ct);
            _context.UserRoles.RemoveRange(userRoles);

            // Batches last in Phase 2 (Students, UserRoles reference BatchId with Restrict)
            var batches = await _context.Batches.IgnoreQueryFilters()
                .Where(b => b.InstituteId == id).ToListAsync(ct);
            _context.Batches.RemoveRange(batches);

            await _context.SaveChangesAsync(ct);

            // ── Phase 3: Trades (depend on AcademicSession + Institute) ──
            var trades = await _context.Trades.IgnoreQueryFilters()
                .Where(t => t.InstituteId == id).ToListAsync(ct);
            _context.Trades.RemoveRange(trades);

            await _context.SaveChangesAsync(ct);

            // ── Phase 4: Audit log ──
            await _auditService.LogAsync(Domain.Enums.AuditAction.Delete, nameof(Institute), id, new { Name = instituteName, GRNumber = instituteGR }, null, ct);
            await _context.SaveChangesAsync(ct);

            // ── Phase 4.5: Re-clean AuditLogs (Phase 4 added one with InstituteId/UserId Restrict FKs) ──
            var remainingAuditLogs = await _context.AuditLogs.IgnoreQueryFilters()
                .Where(a => a.InstituteId == id || (a.UserId.HasValue && userIds.Contains(a.UserId.Value)))
                .ToListAsync(ct);
            _context.AuditLogs.RemoveRange(remainingAuditLogs);
            await _context.SaveChangesAsync(ct);

            // ── Phase 5: Higher-level entities (depend on Institute only) ──
            var sessions = await _context.AcademicSessions.IgnoreQueryFilters()
                .Where(s => s.InstituteId == id).ToListAsync(ct);
            _context.AcademicSessions.RemoveRange(sessions);

            var users = await _context.Users.IgnoreQueryFilters()
                .Where(u => u.InstituteId == id).ToListAsync(ct);
            _context.Users.RemoveRange(users);

            if (instituteRoleIds.Count > 0)
            {
                var roles = await _context.Roles.IgnoreQueryFilters()
                    .Where(r => r.InstituteId == id).ToListAsync(ct);
                _context.Roles.RemoveRange(roles);
            }

            await _context.SaveChangesAsync(ct);

            // ── Phase 6: The Institute itself ──
            _context.Institutes.Remove(institute);
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
