using ITI.ERP.Application.Common.Helpers;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Mappers;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Batch;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class BatchService : IBatchService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public BatchService(IApplicationDbContext context, ICurrentUserService currentUserService, IAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<PaginatedList<BatchDto>>> GetBatchesAsync(Guid? instituteId, Guid? tradeId, Guid? academicSessionId, string? sessionYear, bool? isDeleted, PaginationRequest request, CancellationToken ct)
    {
        var currentUserInstituteId = _currentUserService.InstituteId;

        if (_currentUserService.HasRole(RoleConstants.Admin))
        {
            // SuperAdmin: can filter by instituteId param or see all
        }
        else if (_currentUserService.HasRole(RoleConstants.InstituteAdmin))
        {
            if (currentUserInstituteId is null)
                return Result<PaginatedList<BatchDto>>.Failure("Institute not found.");
        }
        else if (_currentUserService.HasRole(RoleConstants.TradeHead))
        {
            if (currentUserInstituteId is null)
                return Result<PaginatedList<BatchDto>>.Failure("Institute not found.");
        }
        else
        {
            return Result<PaginatedList<BatchDto>>.Failure("Access denied.");
        }

        IQueryable<Batch> baseQuery = _context.Batches
            .AsNoTracking()
            .Include(b => b.Trade)
            .Include(b => b.StartAcademicSession);

        if (isDeleted == true)
            baseQuery = baseQuery.IgnoreQueryFilters();

        var query = baseQuery.Where(b => isDeleted == true ? b.IsDeleted : !b.IsDeleted);

        if (_currentUserService.HasRole(RoleConstants.Admin))
        {
            if (instituteId.HasValue)
                query = query.Where(b => b.InstituteId == instituteId.Value);
        }
        else if (currentUserInstituteId.HasValue)
        {
            query = query.Where(b => b.InstituteId == currentUserInstituteId.Value);
        }

        if (tradeId.HasValue)
            query = query.Where(b => b.TradeId == tradeId.Value);

        if (!string.IsNullOrEmpty(sessionYear))
        {
            var sessionIds = await AcademicSessionHelper.ResolveSessionIdsByYearAsync(
                sessionYear, instituteId, _context, _currentUserService, ct);
            if (sessionIds.Count > 0)
                query = query.Where(b => sessionIds.Contains(b.StartAcademicSessionId));
            else
                query = query.Where(b => false);
        }
        else if (academicSessionId.HasValue)
        {
            query = query.Where(b => b.StartAcademicSessionId == academicSessionId.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(b => b.Trade!.Name)
            .ThenBy(b => b.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var batchIds = items.Select(b => b.Id).ToList();
        var studentCounts = await _context.Students
            .Where(s => s.BatchId.HasValue && batchIds.Contains(s.BatchId.Value) && s.Status == StudentStatus.Active)
            .GroupBy(s => s.BatchId)
            .Select(g => new { BatchId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var countLookup = studentCounts.Where(x => x.BatchId.HasValue).ToDictionary(x => x.BatchId!.Value, x => x.Count);

        var evaluationSession = await AcademicSessionHelper.ResolveSessionOrDefaultAsync(
            academicSessionId, _context, _currentUserService, ct);

        var dtos = items.Select(b =>
        {
            var dto = b.ToDto();
            dto.StudentCount = countLookup.GetValueOrDefault(b.Id, 0);
            if (evaluationSession is not null && b.Trade is not null)
            {
                var result = BatchYearLevelCalculator.Calculate(b, b.Trade, evaluationSession);
                dto.ComputedStatus = result.Status;
                dto.ComputedYearLevel = result.YearLevel.HasValue ? (int)result.YearLevel.Value : null;
                dto.ComputedYearLevelLabel = result.Label;
            }
            return dto;
        }).ToList();

        return Result<PaginatedList<BatchDto>>.Success(new PaginatedList<BatchDto>(dtos, totalCount, request.PageNumber, request.PageSize));
    }

    public async Task<Result<BatchDto>> GetBatchByIdAsync(Guid id, Guid? academicSessionId, CancellationToken ct)
    {
        var batch = await _context.Batches
            .AsNoTracking()
            .Include(b => b.Trade)
            .Include(b => b.StartAcademicSession)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

        if (batch is null)
            return Result<BatchDto>.Failure("Batch not found.");

        var accessCheck = ValidateBatchAccess(batch);
        if (accessCheck is not null)
            return Result<BatchDto>.Failure(accessCheck);

        var dto = batch.ToDto();

        dto.StudentCount = await _context.Students
            .CountAsync(s => s.BatchId == id && s.Status == StudentStatus.Active, ct);

        if (batch.Trade is not null)
        {
            var evaluationSession = await AcademicSessionHelper.ResolveSessionOrDefaultAsync(
                academicSessionId, _context, _currentUserService, ct);
            if (evaluationSession is not null)
            {
                var result = BatchYearLevelCalculator.Calculate(batch, batch.Trade, evaluationSession);
                dto.ComputedStatus = result.Status;
                dto.ComputedYearLevel = result.YearLevel.HasValue ? (int)result.YearLevel.Value : null;
                dto.ComputedYearLevelLabel = result.Label;
            }
        }

        return Result<BatchDto>.Success(dto);
    }

    public async Task<Result<List<BatchDto>>> GetBatchesByTradeAsync(Guid tradeId, Guid? academicSessionId, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null && !_currentUserService.HasRole(RoleConstants.Admin))
            return Result<List<BatchDto>>.Failure("Institute not found.");

        var query = _context.Batches
            .AsNoTracking()
            .Include(b => b.Trade)
            .Include(b => b.StartAcademicSession)
            .Where(b => b.TradeId == tradeId && !b.IsDeleted);

        if (!_currentUserService.HasRole(RoleConstants.Admin) && instituteId.HasValue)
            query = query.Where(b => b.InstituteId == instituteId.Value);

        var batches = await query
            .OrderBy(b => b.Name)
            .ToListAsync(ct);

        var evaluationSession = await AcademicSessionHelper.ResolveSessionOrDefaultAsync(
            academicSessionId, _context, _currentUserService, ct);

        var dtos = batches.Select(b =>
        {
            var dto = b.ToDto();
            if (evaluationSession is not null && b.Trade is not null)
            {
                var result = BatchYearLevelCalculator.Calculate(b, b.Trade, evaluationSession);
                dto.ComputedStatus = result.Status;
                dto.ComputedYearLevel = result.YearLevel.HasValue ? (int)result.YearLevel.Value : null;
                dto.ComputedYearLevelLabel = result.Label;
            }
            return dto;
        }).ToList();

        return Result<List<BatchDto>>.Success(dtos);
    }

    public async Task<Result<BatchDto>> CreateBatchAsync(CreateBatchRequest request, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;

        if (_currentUserService.HasRole(RoleConstants.Admin))
            return Result<BatchDto>.Failure("SuperAdmin cannot create batches.");

        if (instituteId is null)
            return Result<BatchDto>.Failure("Institute not found.");

        var trade = await _context.Trades
            .FirstOrDefaultAsync(t => t.Id == request.TradeId && !t.IsDeleted, ct);

        if (trade is null)
            return Result<BatchDto>.Failure("Trade not found.");

        if (trade.InstituteId != instituteId)
            return Result<BatchDto>.Failure("Trade does not belong to your institute.");

        if (trade.DraftStatus == DraftStatus.Archived)
            return Result<BatchDto>.Failure("Cannot create batch under an archived trade.");

        var academicSession = await _context.AcademicSessions
            .FirstOrDefaultAsync(s => s.Id == request.StartAcademicSessionId && !s.IsDeleted, ct);

        if (academicSession is null)
            return Result<BatchDto>.Failure("Academic session not found.");

        if (academicSession.InstituteId != instituteId)
            return Result<BatchDto>.Failure("Academic session does not belong to your institute.");

        if (request.StartDate < academicSession.StartDate || request.StartDate > academicSession.EndDate)
            return Result<BatchDto>.Failure("Batch start date must fall within the selected academic session.");

        var duplicateExists = await _context.Batches
            .AnyAsync(b => b.InstituteId == instituteId.Value
                && b.TradeId == request.TradeId
                && b.StartAcademicSessionId == request.StartAcademicSessionId
                && b.Name == request.Name
                && !b.IsDeleted, ct);

        if (duplicateExists)
            return Result<BatchDto>.Failure("A batch with this name already exists for this trade and session.");

        if (request.Capacity.HasValue && trade.TotalSeats > 0)
        {
            var existingCapacitySum = await _context.Batches
                .Where(b => b.TradeId == request.TradeId
                    && b.StartAcademicSessionId == request.StartAcademicSessionId
                    && !b.IsDeleted
                    && b.Capacity.HasValue)
                .SumAsync(b => b.Capacity!.Value, ct);

            var remaining = trade.TotalSeats - existingCapacitySum;
            if (request.Capacity.Value > remaining)
            {
                return Result<BatchDto>.Failure(
                    $"Only {remaining} seat{(remaining != 1 ? "s" : "")} available for this trade. " +
                    $"Batch capacity of {request.Capacity.Value} exceeds the remaining seats. " +
                    $"Trade total: {trade.TotalSeats}, already allocated: {existingCapacitySum}.");
            }
        }

        var batch = new Batch
        {
            InstituteId = instituteId.Value,
            TradeId = request.TradeId,
            StartAcademicSessionId = request.StartAcademicSessionId,
            StartDate = request.StartDate,
            Name = request.Name.Trim(),
            Code = request.Code?.Trim(),
            Capacity = request.Capacity,
            IsActive = true
        };

        await _context.Batches.AddAsync(batch, ct);
        await _context.SaveChangesAsync(ct);

        batch.Trade = trade;
        batch.StartAcademicSession = academicSession;

        var dto = batch.ToDto();
        var calcResult = BatchYearLevelCalculator.Calculate(batch, trade, academicSession);
        dto.ComputedStatus = calcResult.Status;
        dto.ComputedYearLevel = calcResult.YearLevel.HasValue ? (int)calcResult.YearLevel.Value : null;
        dto.ComputedYearLevelLabel = calcResult.Label;

        return Result<BatchDto>.Success(dto);
    }

    public async Task<Result<BatchDto>> UpdateBatchAsync(Guid id, UpdateBatchRequest request, Guid? academicSessionId, CancellationToken ct)
    {
        var batch = await _context.Batches
            .Include(b => b.Trade)
            .Include(b => b.StartAcademicSession)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

        if (batch is null)
            return Result<BatchDto>.Failure("Batch not found.");

        var accessCheck = ValidateBatchAccess(batch);
        if (accessCheck is not null)
            return Result<BatchDto>.Failure(accessCheck);

        if (request.Name is not null)
        {
            var duplicateExists = await _context.Batches
                .AnyAsync(b => b.InstituteId == batch.InstituteId
                    && b.TradeId == batch.TradeId
                    && b.StartAcademicSessionId == batch.StartAcademicSessionId
                    && b.Name == request.Name.Trim()
                    && b.Id != id
                    && !b.IsDeleted, ct);

            if (duplicateExists)
                return Result<BatchDto>.Failure("A batch with this name already exists for this trade and session.");

            batch.Name = request.Name.Trim();
        }

        if (request.Code is not null)
            batch.Code = request.Code.Trim();

        if (request.Capacity.HasValue && request.Capacity.Value != batch.Capacity)
        {
            var trade = batch.Trade ?? await _context.Trades.FindAsync(batch.TradeId);
            if (trade is not null && trade.TotalSeats > 0)
            {
                var otherBatchesCapacitySum = await _context.Batches
                    .Where(b => b.TradeId == batch.TradeId
                        && b.StartAcademicSessionId == batch.StartAcademicSessionId
                        && b.Id != id
                        && !b.IsDeleted
                        && b.Capacity.HasValue)
                    .SumAsync(b => b.Capacity!.Value, ct);

                var remaining = trade.TotalSeats - otherBatchesCapacitySum;
                if (request.Capacity.Value > remaining)
                {
                    return Result<BatchDto>.Failure(
                        $"Only {remaining} seat{(remaining != 1 ? "s" : "")} available for this trade. " +
                        $"Batch capacity of {request.Capacity.Value} exceeds the remaining seats. " +
                        $"Trade total: {trade.TotalSeats}, already allocated by other batches: {otherBatchesCapacitySum}.");
                }
            }

            batch.Capacity = request.Capacity;
        }

        if (request.IsActive.HasValue)
            batch.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync(ct);

        batch.Trade ??= await _context.Trades.AsNoTracking().FirstOrDefaultAsync(t => t.Id == batch.TradeId, ct);

        var dto = batch.ToDto();
        if (batch.Trade is not null)
        {
            var evaluationSession = await AcademicSessionHelper.ResolveSessionOrDefaultAsync(
                academicSessionId, _context, _currentUserService, ct);
            if (evaluationSession is not null)
            {
                var result = BatchYearLevelCalculator.Calculate(batch, batch.Trade, evaluationSession);
                dto.ComputedStatus = result.Status;
                dto.ComputedYearLevel = result.YearLevel.HasValue ? (int)result.YearLevel.Value : null;
                dto.ComputedYearLevelLabel = result.Label;
            }
        }

        return Result<BatchDto>.Success(dto);
    }

    public async Task<Result> DeleteBatchAsync(Guid id, CancellationToken ct)
    {
        var batch = await _context.Batches
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

        if (batch is null)
            return Result.Failure("Batch not found.");

        var accessCheck = ValidateBatchAccess(batch);
        if (accessCheck is not null)
            return Result.Failure(accessCheck);

        var now = DateTime.UtcNow;
        var userId = _currentUserService.UserId!.Value;

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            batch.IsDeleted = true;
            batch.DeletedAt = now;
            batch.DeletedBy = userId;

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Archive, nameof(Batch), batch.Id,
                new { batch.Name, batch.DeletedAt, DeletedBy = userId },
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

    private string? ValidateBatchAccess(Batch batch)
    {
        if (_currentUserService.HasRole(RoleConstants.Admin))
            return null;

        var instituteId = _currentUserService.InstituteId;
        if (instituteId is null)
            return "Institute not found.";

        if (batch.InstituteId != instituteId)
            return "Access denied. Batch does not belong to your institute.";

        return null;
    }

    public async Task<Result> ArchiveBatchAsync(Guid id, CancellationToken ct)
    {
        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

        if (batch is null)
            return Result.Failure("Batch not found.");

        var accessCheck = ValidateBatchAccess(batch);
        if (accessCheck is not null)
            return Result.Failure(accessCheck);

        if (!batch.IsActive)
            return Result.Failure("Batch is already archived.");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            batch.IsActive = false;

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Archive, nameof(Batch), batch.Id,
                new { batch.IsActive },
                new { IsActive = false }, ct);
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

    public async Task<Result<BatchArchiveImpactDto>> GetBatchArchiveImpactAsync(Guid id, CancellationToken ct)
    {
        var batch = await _context.Batches
            .Include(b => b.Trade)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

        if (batch is null)
            return Result<BatchArchiveImpactDto>.Failure("Batch not found.");

        var impact = new BatchArchiveImpactDto
        {
            BatchName = batch.Name,
            TradeName = batch.Trade?.Name ?? string.Empty,
            Students = await _context.Students.CountAsync(s => s.BatchId == id && !s.IsDeleted, ct),
            AttendanceRecords = await _context.AttendanceRecords.CountAsync(a => a.BatchId == id && !a.IsDeleted, ct),
            MonthlyPracticals = await _context.MonthlyPracticals.CountAsync(p => p.BatchId == id && !p.IsDeleted, ct),
            YearlyPracticals = await _context.YearlyPracticals.CountAsync(p => p.BatchId == id && !p.IsDeleted, ct),
            UserRoles = await _context.UserRoles.CountAsync(ur => ur.BatchId == id, ct),
        };

        return Result<BatchArchiveImpactDto>.Success(impact);
    }

    public async Task<Result> RestoreBatchAsync(Guid id, CancellationToken ct)
    {
        var batch = await _context.Batches
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted, ct);

        if (batch is null)
            return Result.Failure("Deleted batch not found.");

        var accessCheck = ValidateBatchAccess(batch);
        if (accessCheck is not null)
            return Result.Failure(accessCheck);

        var trade = await _context.Trades.FirstOrDefaultAsync(t => t.Id == batch.TradeId, ct);
        if (trade is null || trade.DraftStatus == DraftStatus.Archived)
            return Result.Failure("Cannot restore batch because parent trade is archived.");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            batch.IsDeleted = false;
            batch.DeletedAt = null;
            batch.DeletedBy = null;

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Restore, nameof(Batch), batch.Id,
                new { batch.Name },
                new { IsDeleted = false }, ct);
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

    public async Task<Result<BatchDeleteImpactDto>> GetBatchDeleteImpactAsync(Guid id, CancellationToken ct)
    {
        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

        if (batch is null)
            return Result<BatchDeleteImpactDto>.Failure("Batch not found.");

        var students = await _context.Students
            .Where(s => s.BatchId == id && !s.IsDeleted)
            .Include(s => s.AcademicSession)
            .ToListAsync(ct);

        var retentionExpiry = DateTime.UtcNow;
        var protectedCount = students.Count(s => s.AcademicSession.EndDate.AddYears(1) > retentionExpiry);
        var eligibleCount = students.Count - protectedCount;

        return Result<BatchDeleteImpactDto>.Success(new BatchDeleteImpactDto
        {
            BatchName = batch.Name,
            TotalStudents = students.Count,
            ProtectedStudents = protectedCount,
            EligibleStudents = eligibleCount,
            CanDelete = protectedCount == 0,
            BlockReason = protectedCount > 0
                ? $"Cannot permanently delete batch {batch.Name} because {protectedCount} student(s) are still within their retention period. Archive the batch instead."
                : null,
        });
    }

    public async Task<Result> PermanentDeleteBatchAsync(Guid id, CancellationToken ct)
    {
        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, ct);

        if (batch is null)
            return Result.Failure("Batch not found.");

        var impactResult = await GetBatchDeleteImpactAsync(id, ct);
        if (!impactResult.IsSuccess)
            return Result.Failure(impactResult.Error!);

        var impact = impactResult.Value!;
        if (!impact.CanDelete)
            return Result.Failure(impact.BlockReason);

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var studentIds = await _context.Students
                .Where(s => s.BatchId == id && !s.IsDeleted)
                .Select(s => s.Id)
                .ToListAsync(ct);

            foreach (var studentId in studentIds)
            {
                await _context.PracticalMarks.Where(m => m.StudentId == studentId).ExecuteDeleteAsync(ct);
                await _context.YearlyPracticalMarks.Where(m => m.StudentId == studentId).ExecuteDeleteAsync(ct);
                await _context.AttendanceRecords.Where(a => a.StudentId == studentId).ExecuteDeleteAsync(ct);
                await _context.Students.Where(s => s.Id == studentId).ExecuteDeleteAsync(ct);
            }

            await _context.AttendanceRecords.Where(a => a.BatchId == id).ExecuteDeleteAsync(ct);
            await _context.MonthlyPracticals.Where(p => p.BatchId == id).ExecuteDeleteAsync(ct);
            await _context.YearlyPracticals.Where(p => p.BatchId == id).ExecuteDeleteAsync(ct);
            await _context.UserRoles.Where(ur => ur.BatchId == id).ExecuteDeleteAsync(ct);
            await _context.Batches.Where(b => b.Id == id).ExecuteDeleteAsync(ct);

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Delete, nameof(Batch), batch.Id,
                new { batch.Name, impact.TotalStudents, impact.ProtectedStudents },
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

    public async Task<Result<BatchPurgeResultDto>> PermanentDeleteBatchInternalAsync(Guid id, CancellationToken ct)
    {
        var batch = await _context.Batches
            .Include(b => b.StartAcademicSession)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted, ct);

        if (batch is null)
            return Result<BatchPurgeResultDto>.Failure("Deleted batch not found.");

        var retentionExpiry = DateTime.UtcNow;
        if (batch.StartAcademicSession.EndDate.AddYears(1) > retentionExpiry)
        {
            return Result<BatchPurgeResultDto>.Failure(
                $"Batch {batch.Name} is still within its retention period " +
                $"(session ends {batch.StartAcademicSession.EndDate:yyyy-MM-dd}, " +
                $"retention expires {batch.StartAcademicSession.EndDate.AddYears(1):yyyy-MM-dd}). Skipping.");
        }

        var result = new BatchPurgeResultDto { BatchName = batch.Name };

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var students = await _context.Students
                .Where(s => s.BatchId == id && !s.IsDeleted)
                .Include(s => s.AcademicSession)
                .ToListAsync(ct);

            foreach (var student in students)
            {
                var practicalsDeleted = await _context.PracticalMarks
                    .Where(m => m.StudentId == student.Id).ExecuteDeleteAsync(ct);
                var yearlyPracticalsDeleted = await _context.YearlyPracticalMarks
                    .Where(m => m.StudentId == student.Id).ExecuteDeleteAsync(ct);
                var attendanceDeleted = await _context.AttendanceRecords
                    .Where(a => a.StudentId == student.Id).ExecuteDeleteAsync(ct);
                await _context.Students
                    .Where(s => s.Id == student.Id).ExecuteDeleteAsync(ct);

                result.StudentsDeleted++;
            }

            var monthlyPracticals = await _context.MonthlyPracticals
                .Where(p => p.BatchId == id && !p.IsDeleted)
                .Include(p => p.AcademicSession)
                .ToListAsync(ct);

            foreach (var mp in monthlyPracticals)
            {
                if (mp.AcademicSession.EndDate.AddYears(1) > retentionExpiry)
                {
                    result.MonthlyPracticalsSkipped++;
                    result.SkipReasons.Add(
                        $"MonthlyPractical {mp.Name} skipped: session retention not expired " +
                        $"(expires {mp.AcademicSession.EndDate.AddYears(1):yyyy-MM-dd}).");
                }
                else
                {
                    await _context.MonthlyPracticals
                        .Where(p => p.Id == mp.Id).ExecuteDeleteAsync(ct);
                    result.MonthlyPracticalsDeleted++;
                }
            }

            var yearlyPracticals = await _context.YearlyPracticals
                .Where(p => p.BatchId == id && !p.IsDeleted)
                .Include(p => p.AcademicSession)
                .ToListAsync(ct);

            foreach (var yp in yearlyPracticals)
            {
                if (yp.AcademicSession.EndDate.AddYears(1) > retentionExpiry)
                {
                    result.YearlyPracticalsSkipped++;
                    result.SkipReasons.Add(
                        $"YearlyPractical {yp.Name} skipped: session retention not expired " +
                        $"(expires {yp.AcademicSession.EndDate.AddYears(1):yyyy-MM-dd}).");
                }
                else
                {
                    await _context.YearlyPracticals
                        .Where(p => p.Id == yp.Id).ExecuteDeleteAsync(ct);
                    result.YearlyPracticalsDeleted++;
                }
            }

            var userRolesDeleted = await _context.UserRoles
                .Where(ur => ur.BatchId == id).ExecuteDeleteAsync(ct);
            result.UserRolesDeleted = userRolesDeleted;

            await _context.Batches
                .Where(b => b.Id == id).ExecuteDeleteAsync(ct);

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Delete, nameof(Batch), batch.Id,
                new
                {
                    batch.Name,
                    result.StudentsDeleted,
                    result.MonthlyPracticalsDeleted,
                    result.MonthlyPracticalsSkipped,
                    result.YearlyPracticalsDeleted,
                    result.YearlyPracticalsSkipped,
                    result.UserRolesDeleted,
                    result.SkipReasons
                },
                null, ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return Result<BatchPurgeResultDto>.Success(result);
    }
}
