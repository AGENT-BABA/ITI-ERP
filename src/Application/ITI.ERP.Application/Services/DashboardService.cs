using ITI.ERP.Application.Common.Helpers;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Dashboard;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITradeAccessService _tradeAccess;

    public DashboardService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITradeAccessService tradeAccess)
    {
        _context = context;
        _currentUserService = currentUserService;
        _tradeAccess = tradeAccess;
    }

    public async Task<Result<DashboardSummaryDto>> GetDashboardSummaryAsync(CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);
        var effectiveTradeId = _tradeAccess.EffectiveTradeId;
        var effectiveBatchId = _tradeAccess.EffectiveBatchId;
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        if (instituteId is null && !isSuperAdmin)
            return Result<DashboardSummaryDto>.Failure("Institute not found.");

        var studentQuery = _context.Students
            .AsNoTracking();

        if (!isSuperAdmin)
            studentQuery = studentQuery.Where(s => s.InstituteId == instituteId);

        if (effectiveTradeId.HasValue)
            studentQuery = studentQuery.Where(s => s.TradeId == effectiveTradeId.Value);

        if (_tradeAccess.IsTradeHead && effectiveBatchId.HasValue)
            studentQuery = studentQuery.Where(s => s.BatchId == effectiveBatchId.Value);

        var counts = await studentQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalStudents = g.Count(),
                ActiveStudents = g.Count(s => s.Status == StudentStatus.Active)
            })
            .FirstOrDefaultAsync(ct) ?? new { TotalStudents = 0, ActiveStudents = 0 };

        var tradeQuery = _context.Trades
            .AsNoTracking();

        if (!isSuperAdmin)
            tradeQuery = tradeQuery.Where(t => t.InstituteId == instituteId);

        if (effectiveTradeId.HasValue)
            tradeQuery = tradeQuery.Where(t => t.Id == effectiveTradeId.Value);

        var tradeCount = 0;
        var userCount = 0;
        var instituteCount = 0;
        var sessionTotals = 0;
        var sessionActive = 0;
        string? tradeName = null;

        if (effectiveTradeId.HasValue)
        {
            var trade = await _context.Trades.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == effectiveTradeId.Value, ct);
            tradeName = trade?.Name;
        }
        else
        {
            tradeCount = await tradeQuery.CountAsync(ct);
            instituteCount = isSuperAdmin
                ? await _context.Institutes.AsNoTracking().CountAsync(i => i.IsActive, ct)
                : 0;
            userCount = await _context.Users
                .AsNoTracking()
                .CountAsync(u => isSuperAdmin || u.InstituteId == instituteId, ct);

            var sessionQuery = _context.AcademicSessions
                .AsNoTracking();

            if (!isSuperAdmin)
                sessionQuery = sessionQuery.Where(s => s.InstituteId == instituteId);

            var sessionCounts = await sessionQuery
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Active = g.Count(s => s.IsActive)
                })
                .FirstOrDefaultAsync(ct);
            sessionTotals = sessionCounts?.Total ?? 0;
            sessionActive = sessionCounts?.Active ?? 0;
        }

        var monthlyPracticalCount = 0;
        var yearlyPracticalCount = 0;

        if (academicSessionId.HasValue)
        {
            var mpQuery = _context.MonthlyPracticals
                .AsNoTracking();

            if (!isSuperAdmin)
                mpQuery = mpQuery.Where(p => p.InstituteId == instituteId);

            mpQuery = mpQuery.Where(p => p.AcademicSessionId == academicSessionId.Value);

            var ypQuery = _context.YearlyPracticals
                .AsNoTracking();

            if (!isSuperAdmin)
                ypQuery = ypQuery.Where(p => p.InstituteId == instituteId);

            ypQuery = ypQuery.Where(p => p.AcademicSessionId == academicSessionId.Value);

            if (effectiveTradeId.HasValue)
            {
                mpQuery = mpQuery.Where(p => p.TradeId == effectiveTradeId.Value);
                ypQuery = ypQuery.Where(p => p.TradeId == effectiveTradeId.Value);
            }

            monthlyPracticalCount = await mpQuery.CountAsync(ct);
            yearlyPracticalCount = await ypQuery.CountAsync(ct);
        }

        var overallAttendancePercentage = 0m;
        var monthlyPracticalPassPercentage = 0m;
        var yearlyPracticalPassPercentage = 0m;

        if (academicSessionId.HasValue)
        {
            var attendanceQuery = _context.AttendanceRecords
                .AsNoTracking();

            if (!isSuperAdmin)
                attendanceQuery = attendanceQuery.Where(a => a.InstituteId == instituteId);

            attendanceQuery = attendanceQuery.Where(a => a.AcademicSessionId == academicSessionId.Value);

            if (effectiveTradeId.HasValue)
                attendanceQuery = attendanceQuery.Where(a => a.TradeId == effectiveTradeId.Value);

            if (_tradeAccess.IsTradeHead && effectiveBatchId.HasValue)
                attendanceQuery = attendanceQuery.Where(a => a.BatchId == effectiveBatchId.Value);

            var attendanceStats = await attendanceQuery
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Present = g.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late)
                })
                .FirstOrDefaultAsync(ct) ?? new { Total = 0, Present = 0 };

            overallAttendancePercentage = attendanceStats.Total > 0
                ? Math.Round((decimal)attendanceStats.Present / attendanceStats.Total * 100, 2)
                : 0;

            if (monthlyPracticalCount > 0)
            {
                var monthlyMarkQuery = _context.PracticalMarks
                    .AsNoTracking()
                    .Where(m => m.MonthlyPractical.AcademicSessionId == academicSessionId.Value);

                if (!isSuperAdmin)
                    monthlyMarkQuery = monthlyMarkQuery.Where(m => m.MonthlyPractical.InstituteId == instituteId);

                if (effectiveTradeId.HasValue)
                    monthlyMarkQuery = monthlyMarkQuery.Where(m => m.MonthlyPractical.TradeId == effectiveTradeId.Value);

                if (_tradeAccess.IsTradeHead && effectiveBatchId.HasValue)
                    monthlyMarkQuery = monthlyMarkQuery.Where(m => m.MonthlyPractical.BatchId == effectiveBatchId.Value);

                var monthlyMarks = await monthlyMarkQuery.Select(m => new
                {
                    m.SafetyConsciousness, m.WorkplaceHygiene, m.AttendancePunctuality,
                    m.FollowInstructions, m.ApplicationKnowledge, m.SkillsToolsEquipment,
                    m.SpeedDoingWork, m.QualityWorkmanship, m.Viva
                }).ToListAsync(ct);

                var settingsQuery = _context.InstituteSettings
                    .AsNoTracking();

                if (!isSuperAdmin)
                    settingsQuery = settingsQuery.Where(s => s.InstituteId == instituteId);

                var settings = await settingsQuery.FirstOrDefaultAsync(ct);
                var passMarksThreshold = settings?.PassMarksPercentage ?? 40;

                var monthlyMarkStats = new
                {
                    Total = monthlyMarks.Count,
                    Passed = monthlyMarks.Count(m =>
                        (m.SafetyConsciousness + m.WorkplaceHygiene + m.AttendancePunctuality +
                         m.FollowInstructions + m.ApplicationKnowledge + m.SkillsToolsEquipment +
                         m.SpeedDoingWork + m.QualityWorkmanship + m.Viva) >= passMarksThreshold)
                };

                monthlyPracticalPassPercentage = monthlyMarkStats.Total > 0
                    ? Math.Round((decimal)monthlyMarkStats.Passed / monthlyMarkStats.Total * 100, 2)
                    : 0;
            }

            if (yearlyPracticalCount > 0)
            {
                var yearlyMarkQuery = _context.YearlyPracticalMarks
                    .AsNoTracking()
                    .Where(m => m.YearlyPractical.AcademicSessionId == academicSessionId.Value);

                if (!isSuperAdmin)
                    yearlyMarkQuery = yearlyMarkQuery.Where(m => m.YearlyPractical.InstituteId == instituteId);

                if (effectiveTradeId.HasValue)
                    yearlyMarkQuery = yearlyMarkQuery.Where(m => m.YearlyPractical.TradeId == effectiveTradeId.Value);

                var yearlyMarkStats = await yearlyMarkQuery
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Passed = g.Count(m => (m.AnnualTotal ?? 0) >= m.YearlyPractical.PassMarks)
                    })
                    .FirstOrDefaultAsync(ct) ?? new { Total = 0, Passed = 0 };

                yearlyPracticalPassPercentage = yearlyMarkStats.Total > 0
                    ? Math.Round((decimal)yearlyMarkStats.Passed / yearlyMarkStats.Total * 100, 2)
                    : 0;
            }
        }

        var tradeSeatOccupancy = new List<TradeSeatOccupancyDto>();
        var recentActivities = new List<RecentActivityDto>();
        var attendanceTrends = new List<AttendanceTrendDto>();
        var studentStatusDistribution = new List<StudentStatusDistributionDto>();

        if (instituteId.HasValue)
        {
            tradeSeatOccupancy = await GetTradeSeatOccupancyInternalAsync(instituteId.Value, academicSessionId, effectiveTradeId, effectiveBatchId, ct);
            recentActivities = await GetRecentActivitiesInternalAsync(instituteId.Value, 10, ct);
            attendanceTrends = await GetAttendanceTrendInternalAsync(instituteId.Value, academicSessionId, effectiveTradeId, 30, ct);
            studentStatusDistribution = await GetStudentStatusDistributionAsync(instituteId.Value, effectiveTradeId, effectiveBatchId, ct);
        }

        return Result<DashboardSummaryDto>.Success(new DashboardSummaryDto
        {
            TotalInstitutes = instituteCount,
            TotalStudents = counts.TotalStudents,
            ActiveStudents = counts.ActiveStudents,
            TotalTrades = tradeCount,
            TotalUsers = userCount,
            TotalAcademicSessions = sessionTotals,
            ActiveAcademicSessions = sessionActive,
            OverallAttendancePercentage = overallAttendancePercentage,
            TotalMonthlyPracticals = monthlyPracticalCount,
            TotalYearlyPracticals = yearlyPracticalCount,
            MonthlyPracticalPassPercentage = monthlyPracticalPassPercentage,
            YearlyPracticalPassPercentage = yearlyPracticalPassPercentage,
            TradeName = tradeName,
            TradeSeatOccupancy = tradeSeatOccupancy,
            RecentActivities = recentActivities,
            AttendanceTrends = attendanceTrends,
            StudentStatusDistribution = studentStatusDistribution
        });
    }

    public async Task<Result<List<AttendanceTrendDto>>> GetAttendanceTrendAsync(int days, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);
        var effectiveTradeId = _tradeAccess.EffectiveTradeId;

        if (instituteId is null)
            return Result<List<AttendanceTrendDto>>.Failure("Institute not found.");

        var trends = await GetAttendanceTrendInternalAsync(instituteId.Value, academicSessionId, effectiveTradeId, days, ct);
        return Result<List<AttendanceTrendDto>>.Success(trends);
    }

    public async Task<Result<List<TradeSeatOccupancyDto>>> GetTradeSeatOccupancyAsync(CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);
        var effectiveTradeId = _tradeAccess.EffectiveTradeId;
        var effectiveBatchId = _tradeAccess.EffectiveBatchId;

        if (instituteId is null)
            return Result<List<TradeSeatOccupancyDto>>.Failure("Institute not found.");

        var occupancy = await GetTradeSeatOccupancyInternalAsync(instituteId.Value, academicSessionId, effectiveTradeId, effectiveBatchId, ct);
        return Result<List<TradeSeatOccupancyDto>>.Success(occupancy);
    }

    public async Task<Result<List<RecentActivityDto>>> GetRecentActivitiesAsync(int count, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<List<RecentActivityDto>>.Failure("Institute not found.");

        var activities = await GetRecentActivitiesInternalAsync(instituteId.Value, count, ct);
        return Result<List<RecentActivityDto>>.Success(activities);
    }

    private async Task<List<TradeSeatOccupancyDto>> GetTradeSeatOccupancyInternalAsync(
        Guid instituteId, Guid? academicSessionId, Guid? effectiveTradeId, Guid? effectiveBatchId, CancellationToken ct)
    {
        var query = _context.Trades
            .AsNoTracking()
            .Where(t => t.InstituteId == instituteId);

        if (effectiveTradeId.HasValue)
            query = query.Where(t => t.Id == effectiveTradeId.Value);

        var tradeIds = await query.Select(t => t.Id).ToListAsync(ct);

        var studentCountsQuery = _context.Students
            .AsNoTracking()
            .Where(s => s.InstituteId == instituteId &&
                        tradeIds.Contains(s.TradeId) &&
                        s.Status == StudentStatus.Active);

        if (effectiveBatchId.HasValue)
            studentCountsQuery = studentCountsQuery.Where(s => s.BatchId == effectiveBatchId.Value);

        var studentCounts = await studentCountsQuery
            .GroupBy(s => s.TradeId)
            .Select(g => new { TradeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TradeId, x => x.Count, ct);

        var tradeData = await query
            .Select(t => new { t.Id, t.Name, t.Code, t.TotalSeats })
            .ToListAsync(ct);

        var result = tradeData.Select(t =>
        {
            var occupiedSeats = studentCounts.GetValueOrDefault(t.Id, 0);
            var occupancyPercentage = t.TotalSeats > 0
                ? Math.Round((decimal)occupiedSeats / t.TotalSeats * 100, 2)
                : 0;

            return new TradeSeatOccupancyDto
            {
                TradeId = t.Id,
                TradeName = t.Name,
                TradeCode = t.Code,
                TotalSeats = t.TotalSeats,
                OccupiedSeats = occupiedSeats,
                OccupancyPercentage = occupancyPercentage
            };
        }).OrderByDescending(t => t.OccupancyPercentage).ToList();

        return result;
    }

    private async Task<List<RecentActivityDto>> GetRecentActivitiesInternalAsync(
        Guid instituteId, int count, CancellationToken ct)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.InstituteId == instituteId)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .Select(a => new RecentActivityDto
            {
                Action = a.Action.ToString(),
                EntityName = a.EntityName,
                UserName = a.UserName,
                Timestamp = a.Timestamp,
                Description = a.NewValues
            })
            .ToListAsync(ct);
    }

    private async Task<List<AttendanceTrendDto>> GetAttendanceTrendInternalAsync(
        Guid instituteId, Guid? academicSessionId, Guid? effectiveTradeId, int days, CancellationToken ct)
    {
        var fromDate = DateTime.UtcNow.Date.AddDays(-days);

        var query = _context.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.InstituteId == instituteId && a.Date >= fromDate);

        if (academicSessionId.HasValue)
            query = query.Where(a => a.AcademicSessionId == academicSessionId.Value);

        if (effectiveTradeId.HasValue)
            query = query.Where(a => a.TradeId == effectiveTradeId.Value);

        var effectiveBatchId = _tradeAccess.EffectiveBatchId;
        if (_tradeAccess.IsTradeHead && effectiveBatchId.HasValue)
            query = query.Where(a => a.BatchId == effectiveBatchId.Value);

        var records = await query.GroupBy(a => a.Date)
            .Select(g => new AttendanceTrendDto
            {
                Date = g.Key,
                PresentCount = g.Count(r => r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late),
                AbsentCount = g.Count(r => r.Status == AttendanceStatus.Absent || r.Status == AttendanceStatus.CL
                    || r.Status == AttendanceStatus.EL || r.Status == AttendanceStatus.ML || r.Status == AttendanceStatus.HO),
                TotalCount = g.Count()
            })
            .OrderBy(t => t.Date)
            .ToListAsync(ct);

        foreach (var trend in records)
        {
            trend.AttendancePercentage = trend.TotalCount > 0
                ? Math.Round((decimal)trend.PresentCount / trend.TotalCount * 100, 2)
                : 0;
        }

        return records;
    }

    private async Task<List<StudentStatusDistributionDto>> GetStudentStatusDistributionAsync(
        Guid instituteId, Guid? effectiveTradeId, Guid? effectiveBatchId, CancellationToken ct)
    {
        var query = _context.Students
            .AsNoTracking()
            .Where(s => s.InstituteId == instituteId);

        if (effectiveTradeId.HasValue)
            query = query.Where(s => s.TradeId == effectiveTradeId.Value);

        if (effectiveBatchId.HasValue)
            query = query.Where(s => s.BatchId == effectiveBatchId.Value);

        var totalStudents = await query.CountAsync(ct);

        if (totalStudents == 0)
            return new List<StudentStatusDistributionDto>();

        var distribution = await query
            .GroupBy(s => s.Status)
            .Select(g => new StudentStatusDistributionDto
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync(ct);

        foreach (var item in distribution)
        {
            item.Percentage = Math.Round((decimal)item.Count / totalStudents * 100, 2);
        }

        return distribution.OrderByDescending(d => d.Count).ToList();
    }
}
