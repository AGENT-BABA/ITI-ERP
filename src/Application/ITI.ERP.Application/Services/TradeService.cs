using ClosedXML.Excel;
using ITI.ERP.Application.Common.Helpers;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Mappers;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Trade;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class TradeService : ITradeService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public TradeService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<PaginatedList<TradeDto>>> GetTradesAsync(PaginationRequest request, CancellationToken ct = default)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        IQueryable<Trade> query = _context.Trades
            .AsNoTracking()
            .Include(t => t.HeadUser);

        if (!isSuperAdmin)
            query = query.Where(t => t.InstituteId == _currentUserService.InstituteId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(searchTerm) ||
                t.Code.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
            "code" => request.SortDescending ? query.OrderByDescending(t => t.Code) : query.OrderBy(t => t.Code),
            _ => query.OrderBy(t => t.Name)
        };

        var paginatedList = await PaginatedList<TradeDto>.CreateAsync(
            query.Select(t => t.ToDto()),
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<TradeDto>>.Success(paginatedList);
    }

    public async Task<Result<TradeDto>> GetTradeByIdAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        IQueryable<Trade> query = _context.Trades
            .AsNoTracking()
            .Include(t => t.HeadUser)
            .Where(t => t.Id == id);

        if (!isSuperAdmin)
            query = query.Where(t => t.InstituteId == _currentUserService.InstituteId);

        var trade = await query.FirstOrDefaultAsync(ct);

        if (trade is null)
            return Result<TradeDto>.Failure("Trade not found.");

        return Result<TradeDto>.Success(trade.ToDto());
    }

    public async Task<Result<TradeDto>> CreateTradeAsync(CreateTradeRequest request, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<TradeDto>.Failure("Institute not found.");

        if (await _context.Trades.AnyAsync(t =>
            t.InstituteId == instituteId &&
            t.Code == request.Code, ct))
            return Result<TradeDto>.Failure("Trade with this code already exists.");

        var trade = new Trade
        {
            InstituteId = instituteId.Value,
            Name = request.Name?.Trim() ?? string.Empty,
            Code = request.Code?.Trim() ?? string.Empty,
            DurationInMonths = request.DurationInMonths,
            TotalSeats = request.TotalSeats,
            HeadUserId = request.HeadUserId
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await _context.Trades.AddAsync(trade, ct);
            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(Domain.Enums.AuditAction.Create, nameof(Trade), trade.Id, null, trade, ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return Result<TradeDto>.Success(trade.ToDto());
    }

    public async Task<Result<TradeDto>> UpdateTradeAsync(Guid id, UpdateTradeRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        IQueryable<Trade> query = _context.Trades
            .Where(t => t.Id == id);

        if (!isSuperAdmin)
            query = query.Where(t => t.InstituteId == _currentUserService.InstituteId);

        var trade = await query.FirstOrDefaultAsync(ct);

        if (trade is null)
            return Result<TradeDto>.Failure("Trade not found.");

        if (request.Code is not null && await _context.Trades.AnyAsync(t =>
            t.InstituteId == trade.InstituteId &&
            t.Code == request.Code &&
            t.Id != id, ct))
            return Result<TradeDto>.Failure("Trade with this code already exists.");

        var oldValues = new
        {
            trade.Name,
            trade.Code,
            trade.DurationInMonths,
            trade.TotalSeats
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            if (request.Name is not null) trade.Name = request.Name.Trim();
            if (request.Code is not null) trade.Code = request.Code.Trim();
            if (request.DurationInMonths.HasValue) trade.DurationInMonths = request.DurationInMonths.Value;
            if (request.TotalSeats.HasValue) trade.TotalSeats = request.TotalSeats.Value;

            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(Domain.Enums.AuditAction.Update, nameof(Trade), trade.Id, oldValues, trade, ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        trade = await _context.Trades.AsNoTracking().Include(t => t.HeadUser).FirstOrDefaultAsync(t => t.Id == id, ct);
        return Result<TradeDto>.Success(trade!.ToDto());
    }

    public async Task<Result> AssignHeadAsync(Guid tradeId, Guid userId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        IQueryable<Trade> query = _context.Trades
            .Where(t => t.Id == tradeId);

        if (!isSuperAdmin)
            query = query.Where(t => t.InstituteId == _currentUserService.InstituteId);

        var trade = await query.FirstOrDefaultAsync(ct);

        if (trade is null)
            return Result.Failure("Trade not found.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.InstituteId == trade.InstituteId && !u.IsDeleted, ct);

        if (user is null)
            return Result.Failure("User not found.");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var oldHeadUserId = trade.HeadUserId;
            trade.HeadUserId = userId;

            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(Domain.Enums.AuditAction.Update, nameof(Trade), trade.Id, new { HeadUserId = oldHeadUserId }, new { HeadUserId = userId }, ct);
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

    public async Task<Result> DeleteTradeAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        IQueryable<Trade> query = _context.Trades
            .Where(t => t.Id == id);

        if (!isSuperAdmin)
            query = query.Where(t => t.InstituteId == _currentUserService.InstituteId);

        var trade = await query.FirstOrDefaultAsync(ct);

        if (trade is null)
            return Result.Failure("Trade not found.");

        var hasLinkedStudents = await _context.Students.AnyAsync(s => s.TradeId == id && !s.IsDeleted, ct);
        if (hasLinkedStudents)
            return Result.Failure("Cannot delete trade with linked Students. Remove all Students first.");

        var hasLinkedAttendance = await _context.AttendanceRecords.AnyAsync(a => a.TradeId == id && !a.IsDeleted, ct);
        if (hasLinkedAttendance)
            return Result.Failure("Cannot delete trade with linked Attendance records. Remove all Attendance first.");

        var hasLinkedMonthlyPracticals = await _context.MonthlyPracticals.AnyAsync(p => p.TradeId == id && !p.IsDeleted, ct);
        if (hasLinkedMonthlyPracticals)
            return Result.Failure("Cannot delete trade with linked Monthly Practicals. Remove all Monthly Practicals first.");

        var hasLinkedYearlyPracticals = await _context.YearlyPracticals.AnyAsync(p => p.TradeId == id && !p.IsDeleted, ct);
        if (hasLinkedYearlyPracticals)
            return Result.Failure("Cannot delete trade with linked Yearly Practicals. Remove all Yearly Practicals first.");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            trade.IsDeleted = true;

            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(Domain.Enums.AuditAction.Delete, nameof(Trade), trade.Id, trade, null, ct);
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

    public async Task<Result<byte[]>> ExportTradesAsync(CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (!isSuperAdmin && instituteId is null)
            return Result<byte[]>.Failure("Institute not found.");

        IQueryable<Trade> query = _context.Trades
            .AsNoTracking()
            .Include(t => t.HeadUser);

        if (!isSuperAdmin)
            query = query.Where(t => t.InstituteId == instituteId!.Value && !t.IsDeleted);
        else
            query = query.Where(t => !t.IsDeleted);

        var trades = await query.OrderBy(t => t.Name).ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Trades");

        var headers = new[] { "TradeCode", "TradeName", "TotalSeats", "DurationMonths", "TradeHead", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        for (int row = 0; row < trades.Count; row++)
        {
            var trade = trades[row];
            worksheet.Cell(row + 2, 1).Value = trade.Code;
            worksheet.Cell(row + 2, 2).Value = trade.Name;
            worksheet.Cell(row + 2, 3).Value = trade.TotalSeats;
            worksheet.Cell(row + 2, 4).Value = trade.DurationInMonths;
            worksheet.Cell(row + 2, 5).Value = trade.HeadUser != null
                ? $"{trade.HeadUser.FirstName} {trade.HeadUser.LastName}".Trim()
                : string.Empty;
            worksheet.Cell(row + 2, 6).Value = trade.DraftStatus.ToString();
        }

        worksheet.Column(1).Width = 15;
        worksheet.Column(2).Width = 35;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).Width = 18;
        worksheet.Column(5).Width = 25;
        worksheet.Column(6).Width = 15;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Result<byte[]>.Success(stream.ToArray());
    }

    public async Task<Result<PaginatedList<TradeDto>>> GetTradesByInstituteAsync(Guid instituteId, PaginationRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        if (!isSuperAdmin && instituteId != _currentUserService.InstituteId)
            return Result<PaginatedList<TradeDto>>.Failure("Cross-institute access denied.");

        IQueryable<Trade> query = _context.Trades
            .AsNoTracking()
            .Include(t => t.HeadUser)
            .Where(t => t.InstituteId == instituteId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(searchTerm) ||
                t.Code.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
            "code" => request.SortDescending ? query.OrderByDescending(t => t.Code) : query.OrderBy(t => t.Code),
            _ => query.OrderBy(t => t.Name)
        };

        var paginatedList = await PaginatedList<TradeDto>.CreateAsync(
            query.Select(t => t.ToDto()),
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<TradeDto>>.Success(paginatedList);
    }

    public async Task<Result> ArchiveTradeAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        IQueryable<Trade> query = _context.Trades.Where(t => t.Id == id);

        if (!isSuperAdmin)
            query = query.Where(t => t.InstituteId == _currentUserService.InstituteId);

        var trade = await query.FirstOrDefaultAsync(ct);

        if (trade is null)
            return Result.Failure("Trade not found.");

        if (trade.DraftStatus == DraftStatus.Archived)
            return Result.Failure("Trade is already archived.");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            trade.PreviousDraftStatus = trade.DraftStatus;
            trade.DraftStatus = DraftStatus.Archived;
            trade.ArchivedAt = DateTime.UtcNow;
            trade.ArchivedBy = _currentUserService.UserId;

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Archive, nameof(Trade), trade.Id,
                new { DraftStatus = trade.PreviousDraftStatus },
                new { DraftStatus = DraftStatus.Archived }, ct);
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

    public async Task<Result<TradeArchiveImpactDto>> GetTradeArchiveImpactAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        IQueryable<Trade> query = _context.Trades.Where(t => t.Id == id);

        if (!isSuperAdmin)
            query = query.Where(t => t.InstituteId == _currentUserService.InstituteId);

        var trade = await query.FirstOrDefaultAsync(ct);

        if (trade is null)
            return Result<TradeArchiveImpactDto>.Failure("Trade not found.");

        var impact = new TradeArchiveImpactDto
        {
            TradeName = trade.Name,
            TradeCode = trade.Code,
            Batches = await _context.Batches.CountAsync(b => b.TradeId == id && !b.IsDeleted, ct),
            Students = await _context.Students.CountAsync(s => s.TradeId == id && !s.IsDeleted, ct),
            AttendanceRecords = await _context.AttendanceRecords.CountAsync(a => a.TradeId == id && !a.IsDeleted, ct),
            MonthlyPracticals = await _context.MonthlyPracticals.CountAsync(p => p.TradeId == id && !p.IsDeleted, ct),
            YearlyPracticals = await _context.YearlyPracticals.CountAsync(p => p.TradeId == id && !p.IsDeleted, ct),
            UserRoles = await _context.UserRoles.CountAsync(ur => ur.TradeId == id, ct),
        };

        return Result<TradeArchiveImpactDto>.Success(impact);
    }

    public async Task<Result> RestoreTradeAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        IQueryable<Trade> query = _context.Trades.Where(t => t.Id == id);

        if (!isSuperAdmin)
            query = query.Where(t => t.InstituteId == _currentUserService.InstituteId);

        var trade = await query.FirstOrDefaultAsync(ct);

        if (trade is null)
            return Result.Failure("Trade not found.");

        if (trade.DraftStatus != DraftStatus.Archived)
            return Result.Failure("Trade is not archived.");

        var previousStatus = trade.PreviousDraftStatus ?? DraftStatus.Finalized;

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            trade.DraftStatus = previousStatus;
            trade.PreviousDraftStatus = null;
            trade.ArchivedAt = null;
            trade.ArchivedBy = null;

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Restore, nameof(Trade), trade.Id,
                new { DraftStatus = DraftStatus.Archived },
                new { DraftStatus = previousStatus }, ct);
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

    public async Task<Result<TradeDeleteImpactDto>> GetTradeDeleteImpactAsync(Guid id, CancellationToken ct)
    {
        var trade = await _context.Trades.FirstOrDefaultAsync(t => t.Id == id, ct);

        if (trade is null)
            return Result<TradeDeleteImpactDto>.Failure("Trade not found.");

        var students = await _context.Students
            .Where(s => s.TradeId == id && !s.IsDeleted)
            .Include(s => s.AcademicSession)
            .ToListAsync(ct);

        var retentionExpiry = DateTime.UtcNow;
        var protectedCount = students.Count(s => s.AcademicSession.EndDate.AddYears(1) > retentionExpiry);
        var eligibleCount = students.Count - protectedCount;

        return Result<TradeDeleteImpactDto>.Success(new TradeDeleteImpactDto
        {
            TradeName = trade.Name,
            TradeCode = trade.Code,
            TotalStudents = students.Count,
            ProtectedStudents = protectedCount,
            EligibleStudents = eligibleCount,
            CanDelete = protectedCount == 0,
            BlockReason = protectedCount > 0
                ? $"Cannot permanently delete {trade.Name} because {protectedCount} student(s) are still within their retention period. Archive the Trade instead."
                : null,
        });
    }

    public async Task<Result> PermanentDeleteTradeAsync(Guid id, CancellationToken ct)
    {
        var trade = await _context.Trades.FirstOrDefaultAsync(t => t.Id == id, ct);

        if (trade is null)
            return Result.Failure("Trade not found.");

        var impactResult = await GetTradeDeleteImpactAsync(id, ct);
        if (!impactResult.IsSuccess)
            return Result.Failure(impactResult.Error!);

        var impact = impactResult.Value!;
        if (!impact.CanDelete)
            return Result.Failure(impact.BlockReason);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var studentIds = await _context.Students
                .Where(s => s.TradeId == id && !s.IsDeleted)
                .Select(s => s.Id)
                .ToListAsync(ct);

            foreach (var studentId in studentIds)
            {
                await _context.PracticalMarks.Where(m => m.StudentId == studentId).ExecuteDeleteAsync(ct);
                await _context.YearlyPracticalMarks.Where(m => m.StudentId == studentId).ExecuteDeleteAsync(ct);
                await _context.AttendanceRecords.Where(a => a.StudentId == studentId).ExecuteDeleteAsync(ct);
                await _context.Students.Where(s => s.Id == studentId).ExecuteDeleteAsync(ct);
            }

            await _context.MonthlyPracticals.Where(p => p.TradeId == id).ExecuteDeleteAsync(ct);
            await _context.YearlyPracticals.Where(p => p.TradeId == id).ExecuteDeleteAsync(ct);
            await _context.AttendanceRecords.Where(a => a.TradeId == id).ExecuteDeleteAsync(ct);
            await _context.Batches.Where(b => b.TradeId == id).ExecuteDeleteAsync(ct);
            await _context.UserRoles.Where(ur => ur.TradeId == id).ExecuteDeleteAsync(ct);
            await _context.Trades.Where(t => t.Id == id).ExecuteDeleteAsync(ct);

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Delete, nameof(Trade), trade.Id,
                new { TradeName = trade.Name, TradeCode = trade.Code, impact.TotalStudents, impact.ProtectedStudents },
                null, ct);
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
