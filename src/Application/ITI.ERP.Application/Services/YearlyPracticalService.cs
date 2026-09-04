using ITI.ERP.Application.Common.Helpers;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.YearlyPractical;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class YearlyPracticalService : IYearlyPracticalService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly ITradeAccessService _tradeAccess;

    private const int NSQF_TOTAL_MARKS = 100;

    public YearlyPracticalService(
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

    private async Task<int> GetPassMarksAsync(CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        if (instituteId is null) return 40;

        var query = _context.InstituteSettings
            .AsNoTracking();

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == instituteId.Value);

        var settings = await query.FirstOrDefaultAsync(ct);
        return settings?.PassMarksPercentage ?? 40;
    }

    private static int ComputeTotalObtained(PracticalMark m) =>
        m.SafetyConsciousness + m.WorkplaceHygiene + m.AttendancePunctuality +
        m.FollowInstructions + m.ApplicationKnowledge + m.SkillsToolsEquipment +
        m.SpeedDoingWork + m.QualityWorkmanship + m.Viva;

    public async Task<Result<PaginatedList<YearlyPracticalDto>>> GetYearlyPracticalsAsync(PaginationRequest request, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

        if (instituteId is null || academicSessionId is null)
            return Result<PaginatedList<YearlyPracticalDto>>.Failure("Institute or Academic Session not found.");

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.YearlyPracticals
            .AsNoTracking()
            .Include(p => p.Trade)
            .Where(p => p.AcademicSessionId == academicSessionId);

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        query = _tradeAccess.ApplyTradeFilter(query, p => p.TradeId);
        query = _tradeAccess.ApplyBatchFilter(query, p => p.BatchId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.Trade.Name.ToLower().Contains(searchTerm) ||
                p.Trade.Code.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "year" => request.SortDescending ? query.OrderByDescending(p => p.Year) : query.OrderBy(p => p.Year),
            _ => query.OrderByDescending(p => p.Year)
        };

        var paginatedList = await PaginatedList<YearlyPracticalDto>.CreateAsync(
            query.Select(p => new YearlyPracticalDto
            {
                Id = p.Id,
                InstituteId = p.InstituteId,
                AcademicSessionId = p.AcademicSessionId,
                TradeId = p.TradeId,
                BatchId = p.BatchId,
                TradeName = p.Trade.Name,
                TradeCode = p.Trade.Code,
                Year = p.Year,
                Name = p.Name,
                Description = p.Description,
                TotalMarks = NSQF_TOTAL_MARKS,
                PassMarks = 0,
                IsLocked = p.IsLocked,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }),
            request.PageNumber,
            request.PageSize);

        if (paginatedList.Items.Count > 0)
        {
            var passMarks = await GetPassMarksAsync(ct);
            var tradeIds = paginatedList.Items.Select(p => p.TradeId).Distinct().ToList();
            var years = paginatedList.Items.Select(p => p.Year).Distinct().ToList();
            var batchIds = paginatedList.Items.Select(p => p.BatchId).Distinct().ToList();

            var academicSessionIds = paginatedList.Items.Select(p => p.AcademicSessionId).Distinct().ToList();

            var monthlyMarksQuery = _context.PracticalMarks
                .AsNoTracking()
                .Include(m => m.MonthlyPractical)
                .Where(m =>
                    tradeIds.Contains(m.MonthlyPractical.TradeId) &&
                    years.Contains(m.MonthlyPractical.Year) &&
                    batchIds.Contains(m.MonthlyPractical.BatchId) &&
                    academicSessionIds.Contains(m.MonthlyPractical.AcademicSessionId));

            if (!isSuperAdmin) monthlyMarksQuery = monthlyMarksQuery.Where(m => m.MonthlyPractical.InstituteId == instituteId);

            var monthlyMarksByBatch = await monthlyMarksQuery
                .GroupBy(m => new { m.MonthlyPractical.TradeId, m.MonthlyPractical.BatchId, m.MonthlyPractical.Year, m.MonthlyPractical.AcademicSessionId })
                .Select(g => new { Key = $"{g.Key.TradeId}_{g.Key.BatchId}_{g.Key.Year}_{g.Key.AcademicSessionId}", Count = g.Select(m => m.StudentId).Distinct().Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

            var batchStudentCounts = await _context.Students
                .Where(s =>
                    s.Status == StudentStatus.Active &&
                    batchIds.Contains(s.BatchId ?? Guid.Empty) &&
                    tradeIds.Contains(s.TradeId) &&
                    academicSessionIds.Contains(s.AcademicSessionId))
                .GroupBy(s => new { s.TradeId, s.BatchId, s.AcademicSessionId })
                .Select(g => new { Key = $"{g.Key.TradeId}_{g.Key.BatchId!.Value}_{g.Key.AcademicSessionId}", Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

            foreach (var item in paginatedList.Items)
            {
                var studentKey = $"{item.TradeId}_{item.BatchId}_{item.AcademicSessionId}";
                item.TotalStudents = batchStudentCounts.GetValueOrDefault(studentKey, 0);
                var marksKey = $"{item.TradeId}_{item.BatchId}_{item.Year}_{item.AcademicSessionId}";
                item.MarksEnteredCount = monthlyMarksByBatch.GetValueOrDefault(marksKey, 0);
                item.PassMarks = passMarks;
            }
        }

        return Result<PaginatedList<YearlyPracticalDto>>.Success(paginatedList);
    }

    public async Task<Result<YearlyPracticalDto>> GetYearlyPracticalByIdAsync(Guid id, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<YearlyPracticalDto>.Failure("Institute not found.");

        IQueryable<YearlyPractical> query = _context.YearlyPracticals
            .AsNoTracking()
            .Include(p => p.Trade);

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (practical is null)
            return Result<YearlyPracticalDto>.Failure("Yearly practical not found.");

        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(id, isMonthly: false, ct);
        if (!accessCheck.IsSuccess)
            return Result<YearlyPracticalDto>.Failure(accessCheck.Error!);

        var passMarks = await GetPassMarksAsync(ct);

        var totalStudentsQuery = _context.Students
            .Where(s =>
                s.AcademicSessionId == practical.AcademicSessionId &&
                s.TradeId == practical.TradeId &&
                s.BatchId == practical.BatchId &&
                s.Status == StudentStatus.Active);

        if (!isSuperAdmin) totalStudentsQuery = totalStudentsQuery.Where(s => s.InstituteId == instituteId);

        var totalStudents = await totalStudentsQuery.CountAsync(ct);

        var marksEnteredCount = await _context.PracticalMarks
            .AsNoTracking()
            .Include(m => m.MonthlyPractical)
            .CountAsync(m =>
                m.MonthlyPractical.TradeId == practical.TradeId &&
                m.MonthlyPractical.BatchId == practical.BatchId &&
                m.MonthlyPractical.AcademicSessionId == practical.AcademicSessionId &&
                m.MonthlyPractical.Year == practical.Year, ct);

        return Result<YearlyPracticalDto>.Success(new YearlyPracticalDto
        {
            Id = practical.Id,
            InstituteId = practical.InstituteId,
            AcademicSessionId = practical.AcademicSessionId,
            TradeId = practical.TradeId,
            BatchId = practical.BatchId,
            TradeName = practical.Trade.Name,
            TradeCode = practical.Trade.Code,
            Year = practical.Year,
            Name = practical.Name,
            Description = practical.Description,
            TotalMarks = NSQF_TOTAL_MARKS,
            PassMarks = passMarks,
            IsLocked = practical.IsLocked,
            MarksEnteredCount = marksEnteredCount,
            TotalStudents = totalStudents,
            CreatedAt = practical.CreatedAt,
            UpdatedAt = practical.UpdatedAt
        });
    }

    public async Task<Result<YearlyPracticalDto>> CreateYearlyPracticalAsync(CreateYearlyPracticalRequest request, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

        if (instituteId is null || academicSessionId is null)
            return Result<YearlyPracticalDto>.Failure("Institute or Academic Session not found.");

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        Guid effectiveTradeId;
        Guid effectiveBatchId;
        if (_tradeAccess.IsTradeHead)
        {
            if (!_tradeAccess.EffectiveTradeId.HasValue)
                return Result<YearlyPracticalDto>.Failure("TradeHead trade assignment is not configured.");
            effectiveTradeId = _tradeAccess.EffectiveTradeId.Value;
            if (!_tradeAccess.EffectiveBatchId.HasValue)
                return Result<YearlyPracticalDto>.Failure("TradeHead batch assignment is not configured.");
            effectiveBatchId = _tradeAccess.EffectiveBatchId.Value;
        }
        else
        {
            effectiveTradeId = request.TradeId;
            if (!request.BatchId.HasValue)
                return Result<YearlyPracticalDto>.Failure("BatchId is required.");
            effectiveBatchId = request.BatchId.Value;
        }

        if (_tradeAccess.IsTradeHead)
        {
            var accessCheck = await _tradeAccess.ValidateTradeAccessAsync(request.TradeId, ct);
            if (!accessCheck.IsSuccess)
                return Result<YearlyPracticalDto>.Failure(accessCheck.Error!);
        }

        if (request.Year < 2000 || request.Year > 2100)
            return Result<YearlyPracticalDto>.Failure("Year must be between 2000 and 2100.");

        var tradeQuery = _context.Trades
            .Where(t => t.Id == effectiveTradeId && !t.IsDeleted);

        if (!isSuperAdmin) tradeQuery = tradeQuery.Where(t => t.InstituteId == instituteId);

        if (!await tradeQuery.AnyAsync(ct))
            return Result<YearlyPracticalDto>.Failure("Trade not found.");

        var duplicateQuery = _context.YearlyPracticals
            .Where(p =>
                p.AcademicSessionId == academicSessionId &&
                p.TradeId == effectiveTradeId &&
                p.Year == request.Year);

        if (!isSuperAdmin) duplicateQuery = duplicateQuery.Where(p => p.InstituteId == instituteId);

        if (await duplicateQuery.AnyAsync(ct))
            return Result<YearlyPracticalDto>.Failure("A yearly practical already exists for this trade in this year.");

        if (!academicSessionId.HasValue) return Result<YearlyPracticalDto>.Failure("No active academic session found.");

        var passMarks = await GetPassMarksAsync(ct);

        var practical = new YearlyPractical
        {
            InstituteId = instituteId.Value,
            AcademicSessionId = academicSessionId.Value,
            TradeId = effectiveTradeId,
            BatchId = effectiveBatchId,
            Year = request.Year,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            TotalMarks = NSQF_TOTAL_MARKS,
            PassMarks = passMarks
        };

        await _context.YearlyPracticals.AddAsync(practical, ct);
        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.Create, nameof(YearlyPractical), practical.Id, null, practical, ct);

        practical = await _context.YearlyPracticals
            .AsNoTracking()
            .Include(p => p.Trade)
            .FirstOrDefaultAsync(p => p.Id == practical.Id, ct);

        return Result<YearlyPracticalDto>.Success(new YearlyPracticalDto
        {
            Id = practical!.Id,
            InstituteId = practical.InstituteId,
            AcademicSessionId = practical.AcademicSessionId,
            TradeId = practical.TradeId,
            BatchId = practical.BatchId,
            TradeName = practical.Trade.Name,
            TradeCode = practical.Trade.Code,
            Year = practical.Year,
            Name = practical.Name,
            Description = practical.Description,
            TotalMarks = NSQF_TOTAL_MARKS,
            PassMarks = passMarks,
            IsLocked = practical.IsLocked,
            CreatedAt = practical.CreatedAt,
            UpdatedAt = practical.UpdatedAt
        });
    }

    public async Task<Result<YearlyPracticalDto>> UpdateYearlyPracticalAsync(Guid id, UpdateYearlyPracticalRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<YearlyPracticalDto>.Failure("Institute not found.");

        IQueryable<YearlyPractical> query = _context.YearlyPracticals
            .Include(p => p.Trade);

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (practical is null)
            return Result<YearlyPracticalDto>.Failure("Yearly practical not found.");

        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(id, isMonthly: false, ct);
        if (!accessCheck.IsSuccess)
            return Result<YearlyPracticalDto>.Failure(accessCheck.Error!);

        if (practical.IsLocked)
            return Result<YearlyPracticalDto>.Failure("Cannot edit a locked practical.");

        var oldValues = new { practical.Name, practical.Description };

        if (request.Name is not null) practical.Name = request.Name.Trim();
        if (request.Description is not null) practical.Description = request.Description.Trim();

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.Update, nameof(YearlyPractical), practical.Id, oldValues, practical, ct);

        var passMarks = await GetPassMarksAsync(ct);

        return Result<YearlyPracticalDto>.Success(new YearlyPracticalDto
        {
            Id = practical.Id,
            InstituteId = practical.InstituteId,
            AcademicSessionId = practical.AcademicSessionId,
            TradeId = practical.TradeId,
            BatchId = practical.BatchId,
            TradeName = practical.Trade.Name,
            TradeCode = practical.Trade.Code,
            Year = practical.Year,
            Name = practical.Name,
            Description = practical.Description,
            TotalMarks = NSQF_TOTAL_MARKS,
            PassMarks = passMarks,
            IsLocked = practical.IsLocked,
            CreatedAt = practical.CreatedAt,
            UpdatedAt = practical.UpdatedAt
        });
    }

    public async Task<Result> DeleteYearlyPracticalAsync(Guid id, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(id, isMonthly: false, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        var userId = _currentUserService.UserId;

        if (instituteId is null || userId is null)
            return Result.Failure("Institute or User not found.");

        IQueryable<YearlyPractical> query = _context.YearlyPracticals;

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (practical is null)
            return Result.Failure("Yearly practical not found.");

        if (practical.IsLocked)
            return Result.Failure("Cannot delete a locked practical.");

        var hasMarks = await _context.YearlyPracticalMarks
            .AnyAsync(m => m.YearlyPracticalId == id, ct);

        if (hasMarks)
            return Result.Failure("Cannot delete practical that has marks entered. Remove all marks first.");

        practical.IsDeleted = true;

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.Delete, nameof(YearlyPractical), practical.Id,
            new { practical.Name, practical.Year }, null, ct);

        return Result.Success();
    }

    public async Task<Result<List<YearlyPracticalMarkDto>>> GetYearlyPracticalMarksAsync(Guid yearlyPracticalId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<List<YearlyPracticalMarkDto>>.Failure("Institute not found.");

        IQueryable<YearlyPractical> query = _context.YearlyPracticals
            .AsNoTracking();

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == yearlyPracticalId, ct);

        if (practical is null)
            return Result<List<YearlyPracticalMarkDto>>.Failure("Yearly practical not found.");

        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(yearlyPracticalId, isMonthly: false, ct);
        if (!accessCheck.IsSuccess)
            return Result<List<YearlyPracticalMarkDto>>.Failure(accessCheck.Error!);

        var passMarks = await GetPassMarksAsync(ct);

        var studentsQuery = _context.Students
            .AsNoTracking()
            .Where(s =>
                s.AcademicSessionId == practical.AcademicSessionId &&
                s.TradeId == practical.TradeId &&
                s.BatchId == practical.BatchId &&
                s.Status == StudentStatus.Active);

        if (!isSuperAdmin) studentsQuery = studentsQuery.Where(s => s.InstituteId == instituteId);

        var students = await studentsQuery
            .OrderBy(s => s.RollNumber)
            .Select(s => new { s.Id, Name = $"{s.FirstName} {s.LastName}", s.RollNumber })
            .ToListAsync(ct);

        var monthlyPracticalMarks = await _context.PracticalMarks
            .AsNoTracking()
            .Include(m => m.MonthlyPractical)
            .Where(m =>
                m.MonthlyPractical.TradeId == practical.TradeId &&
                m.MonthlyPractical.BatchId == practical.BatchId &&
                m.MonthlyPractical.AcademicSessionId == practical.AcademicSessionId &&
                m.MonthlyPractical.Year == practical.Year)
            .ToListAsync(ct);

        var result = students.Select(s =>
        {
            var studentMarks = monthlyPracticalMarks.Where(m => m.StudentId == s.Id).ToList();

            var monthlyAverages = studentMarks
                .GroupBy(m => new { m.MonthlyPractical.Month, m.MonthlyPractical.Year })
                .Select(g => new MonthlyAverageResultDto
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    PracticalCount = g.Count(),
                    AverageObtained = (decimal)Math.Round(g.Average(m => (decimal)ComputeTotalObtained(m)), 2),
                    TotalMarks = NSQF_TOTAL_MARKS,
                    PassMarks = passMarks,
                    IsPassed = g.Average(m => (decimal)ComputeTotalObtained(m)) >= passMarks
                })
                .OrderBy(a => a.Month)
                .ToList();

            var annualAverage = monthlyAverages.Any()
                ? Math.Round(monthlyAverages.Average(a => a.AverageObtained), 2)
                : 0m;

            return new YearlyPracticalMarkDto
            {
                StudentId = s.Id,
                StudentName = s.Name,
                RollNumber = s.RollNumber,
                AnnualAverage = annualAverage,
                TotalMarks = NSQF_TOTAL_MARKS,
                PassMarks = passMarks,
                IsPassed = annualAverage >= passMarks,
                MonthlyAverages = monthlyAverages
            };
        }).ToList();

        return Result<List<YearlyPracticalMarkDto>>.Success(result);
    }

    public async Task<Result> SubmitYearlyPracticalMarksAsync(SubmitYearlyPracticalMarksRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result.Failure("Institute not found.");

        IQueryable<YearlyPractical> query = _context.YearlyPracticals;

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == request.YearlyPracticalId, ct);

        if (practical is null)
            return Result.Failure("Yearly practical not found.");

        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(request.YearlyPracticalId, isMonthly: false, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        return Result.Success();
    }

    public async Task<Result> LockYearlyPracticalAsync(Guid yearlyPracticalId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        var userId = _currentUserService.UserId;

        if (instituteId is null || userId is null)
            return Result.Failure("Institute or User not found.");

        IQueryable<YearlyPractical> query = _context.YearlyPracticals;

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == yearlyPracticalId, ct);

        if (practical is null)
            return Result.Failure("Yearly practical not found.");

        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(yearlyPracticalId, isMonthly: false, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        if (practical.IsLocked)
            return Result.Failure("Practical is already locked.");

        practical.IsLocked = true;
        practical.LockedAt = DateTime.UtcNow;
        practical.LockedBy = userId.Value;

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.DraftLock, nameof(YearlyPractical), practical.Id,
            null, null, ct);

        return Result.Success();
    }

    public async Task<Result> UnlockYearlyPracticalAsync(Guid yearlyPracticalId, string reason, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result.Failure("Institute not found.");

        IQueryable<YearlyPractical> query = _context.YearlyPracticals;

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == yearlyPracticalId, ct);

        if (practical is null)
            return Result.Failure("Yearly practical not found.");

        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(yearlyPracticalId, isMonthly: false, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        if (!practical.IsLocked)
            return Result.Failure("Practical is already unlocked.");

        practical.IsLocked = false;
        practical.LockedAt = null;
        practical.LockedBy = null;

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.DraftUnlock, nameof(YearlyPractical), practical.Id,
            new { reason }, null, ct);

        return Result.Success();
    }

    public async Task<Result<YearlyPracticalReportDto>> GetYearlyPracticalReportAsync(Guid yearlyPracticalId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<YearlyPracticalReportDto>.Failure("Institute not found.");

        IQueryable<YearlyPractical> query = _context.YearlyPracticals
            .AsNoTracking()
            .Include(p => p.Trade);

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == yearlyPracticalId, ct);

        if (practical is null)
            return Result<YearlyPracticalReportDto>.Failure("Yearly practical not found.");

        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(yearlyPracticalId, isMonthly: false, ct);
        if (!accessCheck.IsSuccess)
            return Result<YearlyPracticalReportDto>.Failure(accessCheck.Error!);

        var passMarks = await GetPassMarksAsync(ct);

        var studentsQuery = _context.Students
            .AsNoTracking()
            .Where(s =>
                s.AcademicSessionId == practical.AcademicSessionId &&
                s.TradeId == practical.TradeId &&
                s.BatchId == practical.BatchId &&
                s.Status == StudentStatus.Active);

        if (!isSuperAdmin) studentsQuery = studentsQuery.Where(s => s.InstituteId == instituteId);

        var students = await studentsQuery
            .OrderBy(s => s.RollNumber)
            .Select(s => new { s.Id, Name = $"{s.FirstName} {s.LastName}", s.RollNumber })
            .ToListAsync(ct);

        var monthlyPracticalMarks = await _context.PracticalMarks
            .AsNoTracking()
            .Include(m => m.MonthlyPractical)
            .Where(m =>
                m.MonthlyPractical.TradeId == practical.TradeId &&
                m.MonthlyPractical.BatchId == practical.BatchId &&
                m.MonthlyPractical.AcademicSessionId == practical.AcademicSessionId &&
                m.MonthlyPractical.Year == practical.Year)
            .ToListAsync(ct);

        var studentMarks = students.Select(s =>
        {
            var marks = monthlyPracticalMarks.Where(m => m.StudentId == s.Id).ToList();
            var monthlyAverages = marks
                .GroupBy(m => new { m.MonthlyPractical.Month, m.MonthlyPractical.Year })
                .Select(g => (decimal)g.Average(m => (decimal)ComputeTotalObtained(m)))
                .ToList();
            var annualAverage = monthlyAverages.Any() ? Math.Round(monthlyAverages.Average(), 2) : 0m;
            return new { s.Id, s.Name, s.RollNumber, AnnualAverage = annualAverage };
        }).ToList();

        var passedCount = studentMarks.Count(s => s.AnnualAverage >= passMarks);
        var failedCount = studentMarks.Count - passedCount;
        var average = studentMarks.Any() ? Math.Round(studentMarks.Average(s => s.AnnualAverage), 2) : 0m;
        var highest = studentMarks.Any() ? studentMarks.Max(s => s.AnnualAverage) : 0m;
        var lowest = studentMarks.Any() ? studentMarks.Min(s => s.AnnualAverage) : 0m;
        var passPercentage = studentMarks.Any() ? Math.Round((decimal)passedCount / studentMarks.Count * 100, 2) : 0;

        return Result<YearlyPracticalReportDto>.Success(new YearlyPracticalReportDto
        {
            YearlyPracticalId = yearlyPracticalId,
            PracticalName = practical.Name,
            TradeName = practical.Trade.Name,
            TradeCode = practical.Trade.Code,
            Year = practical.Year,
            TotalMarks = NSQF_TOTAL_MARKS,
            PassMarks = passMarks,
            TotalStudents = studentMarks.Count,
            PassedCount = passedCount,
            FailedCount = failedCount,
            AverageMarks = average,
            HighestMarks = highest,
            LowestMarks = lowest,
            PassPercentage = passPercentage,
            Marks = new List<YearlyPracticalMarkDto>()
        });
    }

    public async Task<Result<StudentYearlyPerformanceDto>> GetStudentYearlyPerformanceAsync(Guid studentId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<StudentYearlyPerformanceDto>.Failure("Institute not found.");

        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(studentId, ct);
        if (!accessCheck.IsSuccess)
            return Result<StudentYearlyPerformanceDto>.Failure(accessCheck.Error!);

        IQueryable<Student> studentQuery = _context.Students
            .AsNoTracking()
            .Include(s => s.Trade);

        if (!isSuperAdmin) studentQuery = studentQuery.Where(s => s.InstituteId == instituteId);

        var student = await studentQuery.FirstOrDefaultAsync(s => s.Id == studentId, ct);

        if (student is null)
            return Result<StudentYearlyPerformanceDto>.Failure("Student not found.");

        var passMarksPercentage = await GetPassMarksAsync(ct);

        var monthlyPracticalMarks = await _context.PracticalMarks
            .AsNoTracking()
            .Include(m => m.MonthlyPractical)
            .Where(m =>
                m.StudentId == studentId &&
                m.MonthlyPractical.AcademicSessionId == student.AcademicSessionId &&
                m.MonthlyPractical.TradeId == student.TradeId &&
                m.MonthlyPractical.BatchId == student.BatchId)
            .OrderBy(m => m.MonthlyPractical.Month)
            .ToListAsync(ct);

        var monthlyPracticals = monthlyPracticalMarks.Select(m =>
        {
            var totalObtained = ComputeTotalObtained(m);
            return new MonthlyPracticalResultDto
            {
                MonthlyPracticalId = m.MonthlyPracticalId,
                PracticalName = m.MonthlyPractical.Name,
                ProfessionalSkillName = m.MonthlyPractical.ProfessionalSkillName,
                Month = m.MonthlyPractical.Month,
                Year = m.MonthlyPractical.Year,
                TotalObtained = totalObtained,
                TotalMarks = NSQF_TOTAL_MARKS,
                PassMarks = passMarksPercentage,
                IsPassed = totalObtained >= passMarksPercentage
            };
        }).ToList();

        var monthlyAverages = monthlyPracticals
            .GroupBy(m => new { m.Month, m.Year })
            .Select(g => new MonthlyAverageResultDto
            {
                Month = g.Key.Month,
                Year = g.Key.Year,
                PracticalCount = g.Count(),
                AverageObtained = (decimal)Math.Round(g.Average(m => m.TotalObtained), 2),
                TotalMarks = NSQF_TOTAL_MARKS,
                PassMarks = passMarksPercentage,
                IsPassed = g.Average(m => m.TotalObtained) >= passMarksPercentage
            })
            .OrderBy(a => a.Month)
            .ToList();

        var monthlyAverage = monthlyAverages.Any()
            ? Math.Round(monthlyAverages.Average(a => a.AverageObtained), 2)
            : 0m;

        var overallAverage = monthlyAverages.Any()
            ? Math.Round(monthlyAverages.Average(a => a.AverageObtained), 2)
            : 0m;

        return Result<StudentYearlyPerformanceDto>.Success(new StudentYearlyPerformanceDto
        {
            StudentId = studentId,
            StudentName = $"{student.FirstName} {student.LastName}",
            RollNumber = student.RollNumber,
            TradeCode = student.Trade?.Code,
            MonthlyPracticals = monthlyPracticals,
            MonthlyAverages = monthlyAverages,
            YearlyPractical = null,
            MonthlyPracticalAverage = monthlyAverage,
            OverallPracticalAverage = overallAverage
        });
    }

    private static readonly int[] AcademicMonthOrder = [8, 9, 10, 11, 12, 1, 2, 3, 4, 5, 6, 7];

    private static string GetMonthName(int month) => month switch
    {
        1 => "January", 2 => "February", 3 => "March", 4 => "April",
        5 => "May", 6 => "June", 7 => "July", 8 => "August",
        9 => "September", 10 => "October", 11 => "November", 12 => "December",
        _ => $"Month {month}"
    };

    private static int GetAcademicYear(int month, int year) => month >= 8 ? year : year - 1;

    private static Dictionary<int, MonthlyManualEntry> ParseManualEntries(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, MonthlyManualEntry>>(json) ?? new();
        }
        catch { return new(); }
    }

    private static string SerializeManualEntries(Dictionary<int, MonthlyManualEntry> entries)
    {
        return System.Text.Json.JsonSerializer.Serialize(entries);
    }

    public async Task<Result<List<YearlyPracticalStudentListDto>>> GetYearlyPracticalStudentsAsync(Guid yearlyPracticalId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        if (instituteId is null)
            return Result<List<YearlyPracticalStudentListDto>>.Failure("Institute not found.");

        IQueryable<YearlyPractical> query = _context.YearlyPracticals
            .AsNoTracking();

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == yearlyPracticalId, ct);

        if (practical is null)
            return Result<List<YearlyPracticalStudentListDto>>.Failure("Yearly practical not found.");

        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(yearlyPracticalId, isMonthly: false, ct);
        if (!accessCheck.IsSuccess)
            return Result<List<YearlyPracticalStudentListDto>>.Failure(accessCheck.Error!);

        var studentsQuery = _context.Students
            .AsNoTracking()
            .Where(s =>
                s.AcademicSessionId == practical.AcademicSessionId &&
                s.TradeId == practical.TradeId &&
                s.BatchId == practical.BatchId &&
                s.Status == StudentStatus.Active);

        if (!isSuperAdmin) studentsQuery = studentsQuery.Where(s => s.InstituteId == instituteId);

        var students = await studentsQuery
            .OrderBy(s => s.RollNumber)
            .Select(s => new { s.Id, Name = $"{s.FirstName} {s.LastName}", s.RollNumber })
            .ToListAsync(ct);

        var monthlyPracticalMarks = await _context.PracticalMarks
            .AsNoTracking()
            .Include(m => m.MonthlyPractical)
            .Where(m =>
                m.MonthlyPractical.TradeId == practical.TradeId &&
                m.MonthlyPractical.BatchId == practical.BatchId &&
                m.MonthlyPractical.AcademicSessionId == practical.AcademicSessionId &&
                m.MonthlyPractical.Year == practical.Year)
            .ToListAsync(ct);

        var yearlyMarks = await _context.YearlyPracticalMarks
            .AsNoTracking()
            .Where(ym => ym.YearlyPracticalId == yearlyPracticalId)
            .ToListAsync(ct);

        var result = students.Select(s =>
        {
            var studentMarks = monthlyPracticalMarks.Where(m => m.StudentId == s.Id).ToList();
            var studentYearlyMark = yearlyMarks.FirstOrDefault(ym => ym.StudentId == s.Id);

            var monthlyPRs = AcademicMonthOrder.Select(month =>
            {
                var monthMarks = studentMarks.Where(m => m.MonthlyPractical.Month == month).ToList();
                if (monthMarks.Count == 0)
                    return new MonthlyPrSummaryDto { Month = month, MonthName = GetMonthName(month), PR = null, PracticalCount = null };

                var avg = Math.Round(monthMarks.Average(m => (decimal)ComputeTotalObtained(m)), 1);
                return new MonthlyPrSummaryDto { Month = month, MonthName = GetMonthName(month), PR = avg, PracticalCount = monthMarks.Count };
            }).ToList();

            var manualEntries = ParseManualEntries(studentYearlyMark?.MonthlyManualEntries);
            decimal annualTotal = 0;
            foreach (var pr in monthlyPRs)
            {
                manualEntries.TryGetValue(pr.Month, out var me);
                bool hasData = pr.PR.HasValue;
                decimal rowTotal = pr.PR ?? 0;
                if (me != null)
                {
                    rowTotal += (me.PartA ?? 0) + (me.PartB ?? 0) + (me.VocationalScience ?? 0) + (me.EngDrawing ?? 0);
                    if (me.PartA.HasValue || me.PartB.HasValue || me.VocationalScience.HasValue || me.EngDrawing.HasValue)
                        hasData = true;
                }
                if (hasData) annualTotal += rowTotal;
            }

            return new YearlyPracticalStudentListDto
            {
                StudentId = s.Id,
                StudentName = s.Name,
                RollNumber = s.RollNumber,
                MonthlyPRs = monthlyPRs,
                AnnualTotal = annualTotal,
                AnnualRemark = studentYearlyMark?.AnnualRemark
            };
        }).ToList();

        return Result<List<YearlyPracticalStudentListDto>>.Success(result);
    }

    public async Task<Result<YearlyPracticalStudentDetailDto>> GetYearlyPracticalStudentDetailAsync(Guid yearlyPracticalId, Guid studentId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        if (instituteId is null)
            return Result<YearlyPracticalStudentDetailDto>.Failure("Institute not found.");

        IQueryable<YearlyPractical> query = _context.YearlyPracticals
            .AsNoTracking();

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == yearlyPracticalId, ct);

        if (practical is null)
            return Result<YearlyPracticalStudentDetailDto>.Failure("Yearly practical not found.");

        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(yearlyPracticalId, isMonthly: false, ct);
        if (!accessCheck.IsSuccess)
            return Result<YearlyPracticalStudentDetailDto>.Failure(accessCheck.Error!);

        IQueryable<Student> studentQuery = _context.Students
            .AsNoTracking();

        if (!isSuperAdmin) studentQuery = studentQuery.Where(s => s.InstituteId == instituteId);

        var student = await studentQuery.FirstOrDefaultAsync(s =>
                s.Id == studentId &&
                s.TradeId == practical.TradeId &&
                s.BatchId == practical.BatchId, ct);

        if (student is null)
            return Result<YearlyPracticalStudentDetailDto>.Failure("Student not found or does not belong to this trade/batch.");

        var monthlyPracticalMarks = await _context.PracticalMarks
            .AsNoTracking()
            .Include(m => m.MonthlyPractical)
            .Where(m =>
                m.StudentId == studentId &&
                m.MonthlyPractical.TradeId == practical.TradeId &&
                m.MonthlyPractical.BatchId == practical.BatchId &&
                m.MonthlyPractical.AcademicSessionId == practical.AcademicSessionId &&
                m.MonthlyPractical.Year == practical.Year)
            .ToListAsync(ct);

        var studentYearlyMark = await _context.YearlyPracticalMarks
            .AsNoTracking()
            .FirstOrDefaultAsync(ym => ym.YearlyPracticalId == yearlyPracticalId && ym.StudentId == studentId, ct);

        var manualEntries = ParseManualEntries(studentYearlyMark?.MonthlyManualEntries);

        decimal annualTotal = 0;

        var months = AcademicMonthOrder.Select(month =>
        {
            var monthMarks = monthlyPracticalMarks.Where(m => m.MonthlyPractical.Month == month).ToList();
            decimal? pr = monthMarks.Count > 0
                ? Math.Round(monthMarks.Average(m => (decimal)ComputeTotalObtained(m)), 1)
                : null;

            manualEntries.TryGetValue(month, out var entry);

            int? partA = entry?.PartA;
            int? partB = entry?.PartB;
            int? vocSci = entry?.VocationalScience;
            int? engDraw = entry?.EngDrawing;

            decimal? rowTotal = null;
            if (pr.HasValue || partA.HasValue || partB.HasValue || vocSci.HasValue || engDraw.HasValue)
            {
                rowTotal = (pr ?? 0) + (partA ?? 0) + (partB ?? 0) + (vocSci ?? 0) + (engDraw ?? 0);
                annualTotal += rowTotal.Value;
            }

            return new YearlyPracticalMonthRowDto
            {
                Month = month,
                MonthName = GetMonthName(month),
                PR = pr,
                PartA = partA,
                PartB = partB,
                VocationalScience = vocSci,
                EngDrawing = engDraw,
                Total = rowTotal,
                GISig = entry?.GISig,
                PVPSig = entry?.PVPSig,
                Remark = entry?.Remark
            };
        }).ToList();

        return Result<YearlyPracticalStudentDetailDto>.Success(new YearlyPracticalStudentDetailDto
        {
            StudentId = studentId,
            StudentName = $"{student.FirstName} {student.LastName}",
            RollNumber = student.RollNumber,
            Months = months,
            AnnualTotal = annualTotal,
            AnnualRemark = studentYearlyMark?.AnnualRemark,
            IsLocked = practical.IsLocked
        });
    }

    public async Task<Result> SaveStudentYearlyEntriesAsync(Guid yearlyPracticalId, Guid studentId, SaveYearlyEntriesRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        var userId = _currentUserService.UserId;

        if (instituteId is null || userId is null)
            return Result.Failure("Institute or User not found.");

        IQueryable<YearlyPractical> query = _context.YearlyPracticals;

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == yearlyPracticalId, ct);

        if (practical is null)
            return Result.Failure("Yearly practical not found.");

        if (practical.IsLocked)
            return Result.Failure("Yearly practical is locked.");

        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(yearlyPracticalId, isMonthly: false, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        IQueryable<Student> studentQuery = _context.Students;

        if (!isSuperAdmin) studentQuery = studentQuery.Where(s => s.InstituteId == instituteId);

        var student = await studentQuery.FirstOrDefaultAsync(s =>
                s.Id == studentId &&
                s.TradeId == practical.TradeId &&
                s.BatchId == practical.BatchId, ct);

        if (student is null)
            return Result.Failure("Student not found or does not belong to this trade/batch.");

        var mark = await _context.YearlyPracticalMarks
            .FirstOrDefaultAsync(ym => ym.YearlyPracticalId == yearlyPracticalId && ym.StudentId == studentId, ct);

        var manualEntries = ParseManualEntries(mark?.MonthlyManualEntries);

        if (request.PartA is < 0 or > 100)
            return Result.Failure("PartA must be between 0 and 100.");
        if (request.PartB is < 0 or > 50)
            return Result.Failure("PartB must be between 0 and 50.");
        if (request.VocationalScience is < 0 or > 50)
            return Result.Failure("Vocational Science must be between 0 and 50.");
        if (request.EngDrawing is < 0 or > 50)
            return Result.Failure("Engineering Drawing must be between 0 and 50.");

        var entry = new MonthlyManualEntry
        {
            PartA = request.PartA,
            PartB = request.PartB,
            VocationalScience = request.VocationalScience,
            EngDrawing = request.EngDrawing,
            GISig = request.GISig,
            PVPSig = request.PVPSig,
            Remark = request.Remark
        };

        manualEntries[request.Month] = entry;

        if (mark is null)
        {
            mark = new YearlyPracticalMark
            {
                InstituteId = instituteId.Value,
                YearlyPracticalId = yearlyPracticalId,
                StudentId = studentId,
                MarksObtained = 0,
                MarkedBy = userId.Value,
                MonthlyManualEntries = SerializeManualEntries(manualEntries)
            };
            await _context.YearlyPracticalMarks.AddAsync(mark, ct);
        }
        else
        {
            mark.MonthlyManualEntries = SerializeManualEntries(manualEntries);
            mark.MarkedBy = userId.Value;
        }

        var monthlyMarks = await _context.PracticalMarks
            .AsNoTracking()
            .Include(m => m.MonthlyPractical)
            .Where(m =>
                m.StudentId == studentId &&
                m.MonthlyPractical.TradeId == practical.TradeId &&
                m.MonthlyPractical.BatchId == practical.BatchId &&
                m.MonthlyPractical.AcademicSessionId == practical.AcademicSessionId &&
                m.MonthlyPractical.Year == practical.Year)
            .ToListAsync(ct);

        var monthlyPrAverages = monthlyMarks
            .GroupBy(m => m.MonthlyPractical.Month)
            .ToDictionary(
                g => g.Key,
                g => (decimal?)Math.Round(g.Average(m => (decimal)ComputeTotalObtained(m)), 1));

        mark.AnnualTotal = RecalculateAnnualTotal(manualEntries, monthlyPrAverages);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static decimal RecalculateAnnualTotal(
        Dictionary<int, MonthlyManualEntry> entries,
        Dictionary<int, decimal?> monthlyPrAverages)
    {
        // Iterate ALL months that have either PR data OR manual entries
        var allMonths = entries.Keys.Union(monthlyPrAverages.Keys);
        decimal total = 0;
        foreach (var month in allMonths)
        {
            entries.TryGetValue(month, out var e);
            monthlyPrAverages.TryGetValue(month, out var pr);
            bool hasData = pr.HasValue || (e != null && (e.PartA.HasValue || e.PartB.HasValue || e.VocationalScience.HasValue || e.EngDrawing.HasValue));
            if (hasData)
            {
                total += (pr ?? 0) + (e?.PartA ?? 0) + (e?.PartB ?? 0) + (e?.VocationalScience ?? 0) + (e?.EngDrawing ?? 0);
            }
        }
        return total;
    }
}

public class MonthlyManualEntry
{
    public int? PartA { get; set; }
    public int? PartB { get; set; }
    public int? VocationalScience { get; set; }
    public int? EngDrawing { get; set; }
    public string? GISig { get; set; }
    public string? PVPSig { get; set; }
    public string? Remark { get; set; }
}
