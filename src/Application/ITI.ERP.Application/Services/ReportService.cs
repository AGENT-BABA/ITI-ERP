using ITI.ERP.Application.Common.Helpers;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Reports;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITradeAccessService _tradeAccess;

    public ReportService(IApplicationDbContext context, ICurrentUserService currentUserService, ITradeAccessService tradeAccess)
    {
        _context = context;
        _currentUserService = currentUserService;
        _tradeAccess = tradeAccess;
    }

    private async Task<int> GetAttendanceThresholdAsync(Guid? instituteId, CancellationToken ct)
    {
        var settingsQuery = _context.InstituteSettings.AsNoTracking();
        if (instituteId.HasValue)
            settingsQuery = settingsQuery.Where(s => s.InstituteId == instituteId.Value);
        var settings = await settingsQuery.FirstOrDefaultAsync(ct);
        return settings?.AttendanceThresholdPercentage ?? 75;
    }

    public async Task<Result<StudentAttendanceReportDto>> GetStudentAttendanceReportAsync(Guid studentId, DateTime fromDate, DateTime toDate, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        if (instituteId is null && !isSuperAdmin)
            return Result<StudentAttendanceReportDto>.Failure("Institute not found.");

        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(studentId, ct);
        if (!accessCheck.IsSuccess)
            return Result<StudentAttendanceReportDto>.Failure(accessCheck.Error!);

        var studentQuery = _context.Students
            .AsNoTracking().Include(s => s.Trade).Include(s => s.AcademicSession)
            .Where(s => s.Id == studentId);

        if (!isSuperAdmin)
            studentQuery = studentQuery.Where(s => s.InstituteId == instituteId);

        var student = await studentQuery.FirstOrDefaultAsync(ct);

        if (student is null)
            return Result<StudentAttendanceReportDto>.Failure("Student not found.");

        var recordsQuery = _context.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.StudentId == studentId && a.Date >= fromDate.Date && a.Date <= toDate.Date);

        if (!isSuperAdmin)
            recordsQuery = recordsQuery.Where(a => a.InstituteId == instituteId);

        var records = await recordsQuery.OrderBy(a => a.Date).ToListAsync(ct);

        var totalDaysInMonth = DateTime.DaysInMonth(fromDate.Year, fromDate.Month);
        var holidaysQuery = _context.Holidays
            .AsNoTracking()
            .Where(h => h.AcademicSessionId == student.AcademicSessionId
                     && h.Date.Month == fromDate.Month && h.Date.Year == fromDate.Year
                     && !h.IsDeleted);
        if (!isSuperAdmin)
            holidaysQuery = holidaysQuery.Where(h => h.InstituteId == instituteId);
        var holidaysInMonth = await holidaysQuery.CountAsync(ct);
        var totalWorkingDays = totalDaysInMonth - holidaysInMonth;
        var presentDays = records.Count(r => r.Status == AttendanceStatus.Present);
        var lateDays = records.Count(r => r.Status == AttendanceStatus.Late);
        var percentage = totalWorkingDays > 0 ? Math.Round((decimal)(presentDays + lateDays) / totalWorkingDays * 100, 2) : 0;
        var threshold = await GetAttendanceThresholdAsync(instituteId, ct);

        return Result<StudentAttendanceReportDto>.Success(new StudentAttendanceReportDto
        {
            StudentId = studentId, StudentName = $"{student.FirstName} {student.LastName}",
            RollNumber = student.RollNumber, TradeCode = student.Trade?.Code, TradeName = student.Trade?.Name,
            FromDate = fromDate.Date, ToDate = toDate.Date, TotalWorkingDays = totalWorkingDays,
            PresentDays = presentDays, AbsentDays = records.Count(r => r.Status == AttendanceStatus.Absent),
            LateDays = lateDays, CLDays = records.Count(r => r.Status == AttendanceStatus.CL),
            ELDays = records.Count(r => r.Status == AttendanceStatus.EL),
            MLDays = records.Count(r => r.Status == AttendanceStatus.ML),
            HODays = records.Count(r => r.Status == AttendanceStatus.HO),
            AttendancePercentage = percentage,
            AttendanceThresholdPercentage = threshold,
            DailyRecords = records.Select(r => new DailyAttendanceDto { Date = r.Date, Status = r.Status.ToString(), Remarks = r.Remarks }).ToList()
        });
    }

    public async Task<Result<TradeAttendanceReportDto>> GetTradeAttendanceReportAsync(Guid tradeId, DateTime fromDate, DateTime toDate, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);
        if ((instituteId is null && !isSuperAdmin) || academicSessionId is null)
            return Result<TradeAttendanceReportDto>.Failure("Institute or Academic Session not found.");

        var tradeAccessCheck = await _tradeAccess.ValidateTradeAccessAsync(tradeId, ct);
        if (!tradeAccessCheck.IsSuccess)
            return Result<TradeAttendanceReportDto>.Failure(tradeAccessCheck.Error!);

        var tradeQuery = _context.Trades.AsNoTracking()
            .Where(t => t.Id == tradeId);

        if (!isSuperAdmin)
            tradeQuery = tradeQuery.Where(t => t.InstituteId == instituteId);

        var trade = await tradeQuery.FirstOrDefaultAsync(ct);
        if (trade is null) return Result<TradeAttendanceReportDto>.Failure("Trade not found.");

        var targetSession = await _context.AcademicSessions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == academicSessionId, ct);
        if (targetSession is null)
            return Result<TradeAttendanceReportDto>.Failure("Academic session not found.");

        var allBatchesForTrade = await _context.Batches
            .AsNoTracking()
            .Include(b => b.Trade)
            .Include(b => b.StartAcademicSession)
            .Where(b => b.TradeId == tradeId && !b.IsDeleted)
            .ToListAsync(ct);

        var activeBatchIds = allBatchesForTrade
            .Where(b => BatchYearLevelCalculator.IsBatchActiveInSession(b, b.Trade, targetSession))
            .Select(b => b.Id)
            .ToList();

        var studentsQuery = _context.Students.AsNoTracking()
            .Where(s => s.TradeId == tradeId
                && s.BatchId.HasValue && activeBatchIds.Contains(s.BatchId.Value)
                && s.Status == StudentStatus.Active);

        if (!isSuperAdmin)
            studentsQuery = studentsQuery.Where(s => s.InstituteId == instituteId);

        var effectiveBatchId = _tradeAccess.EffectiveBatchId;
        if (_tradeAccess.IsTradeHead && effectiveBatchId.HasValue)
        {
            studentsQuery = studentsQuery.Where(s => s.BatchId == effectiveBatchId.Value);
        }

        var students = await studentsQuery.OrderBy(s => s.RollNumber).ToListAsync(ct);

        var recordsQuery = _context.AttendanceRecords.AsNoTracking()
            .Where(a => a.TradeId == tradeId && a.AcademicSessionId == academicSessionId && a.Date >= fromDate.Date && a.Date <= toDate.Date);

        if (!isSuperAdmin)
            recordsQuery = recordsQuery.Where(a => a.InstituteId == instituteId);

        if (_tradeAccess.IsTradeHead && effectiveBatchId.HasValue)
        {
            recordsQuery = recordsQuery.Where(a => a.BatchId == effectiveBatchId.Value);
        }

        var records = await recordsQuery.ToListAsync(ct);

        var totalDaysInMonth = DateTime.DaysInMonth(fromDate.Year, fromDate.Month);
        var holidaysQuery = _context.Holidays
            .AsNoTracking()
            .Where(h => h.AcademicSessionId == academicSessionId
                     && h.Date.Month == fromDate.Month && h.Date.Year == fromDate.Year
                     && !h.IsDeleted);
        if (!isSuperAdmin)
            holidaysQuery = holidaysQuery.Where(h => h.InstituteId == instituteId);
        var holidaysInMonth = await holidaysQuery.CountAsync(ct);
        var totalWorkingDays = totalDaysInMonth - holidaysInMonth;

        var studentSummaries = students.Select(s =>
        {
            var sr = records.Where(r => r.StudentId == s.Id).ToList();
            var present = sr.Count(r => r.Status == AttendanceStatus.Present);
            var late = sr.Count(r => r.Status == AttendanceStatus.Late);
            var pct = totalWorkingDays > 0 ? Math.Round((decimal)(present + late) / totalWorkingDays * 100, 2) : 0;
            return new StudentAttendanceSummaryDto
            {
                StudentId = s.Id, StudentName = $"{s.FirstName} {s.LastName}", RollNumber = s.RollNumber,
                TotalDays = sr.Count, PresentDays = present, AbsentDays = sr.Count(r => r.Status == AttendanceStatus.Absent),
                LateDays = late, CLDays = sr.Count(r => r.Status == AttendanceStatus.CL),
                ELDays = sr.Count(r => r.Status == AttendanceStatus.EL), MLDays = sr.Count(r => r.Status == AttendanceStatus.ML),
                HODays = sr.Count(r => r.Status == AttendanceStatus.HO), AttendancePercentage = pct
            };
        }).ToList();

        var avg = studentSummaries.Any() ? Math.Round(studentSummaries.Average(s => s.AttendancePercentage), 2) : 0;
        var threshold = await GetAttendanceThresholdAsync(instituteId, ct);

        return Result<TradeAttendanceReportDto>.Success(new TradeAttendanceReportDto
        {
            TradeId = tradeId, TradeName = trade.Name, TradeCode = trade.Code,
            FromDate = fromDate.Date, ToDate = toDate.Date, TotalWorkingDays = totalWorkingDays,
            AttendanceThresholdPercentage = threshold,
            Students = studentSummaries,
            Summary = new TradeAttendanceSummaryDto
            {
                TotalStudents = students.Count, AverageAttendance = avg,
                HighestAttendanceDays = studentSummaries.Any() ? studentSummaries.Max(s => s.TotalDays) : 0,
                LowestAttendanceDays = studentSummaries.Any() ? studentSummaries.Min(s => s.TotalDays) : 0,
                StudentsAboveThreshold = studentSummaries.Count(s => s.AttendancePercentage >= threshold),
                StudentsBelowThreshold = studentSummaries.Count(s => s.AttendancePercentage < threshold)
            }
        });
    }

    public async Task<Result<MonthlyPracticalReportDto>> GetMonthlyPracticalReportAsync(Guid monthlyPracticalId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        if (instituteId is null && !isSuperAdmin) return Result<MonthlyPracticalReportDto>.Failure("Institute not found.");

        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(monthlyPracticalId, isMonthly: true, ct);
        if (!accessCheck.IsSuccess) return Result<MonthlyPracticalReportDto>.Failure(accessCheck.Error!);

        var practicalQuery = _context.MonthlyPracticals.AsNoTracking().Include(p => p.Trade)
            .Where(p => p.Id == monthlyPracticalId);

        if (!isSuperAdmin)
            practicalQuery = practicalQuery.Where(p => p.InstituteId == instituteId);

        var practical = await practicalQuery.FirstOrDefaultAsync(ct);
        if (practical is null) return Result<MonthlyPracticalReportDto>.Failure("Monthly practical not found.");

        var marks = await _context.PracticalMarks.AsNoTracking().Include(m => m.Student)
            .Where(m => m.MonthlyPracticalId == monthlyPracticalId).OrderBy(m => m.Student.RollNumber).ToListAsync(ct);

        var passMarks = 40;
        var settingsQuery = _context.InstituteSettings.AsNoTracking();
        if (!isSuperAdmin)
            settingsQuery = settingsQuery.Where(s => s.InstituteId == instituteId);
        var settings = await settingsQuery.FirstOrDefaultAsync(ct);
        if (settings is not null) passMarks = settings.PassMarksPercentage;

        var totals = marks.Select(m => m.SafetyConsciousness + m.WorkplaceHygiene + m.AttendancePunctuality +
            m.FollowInstructions + m.ApplicationKnowledge + m.SkillsToolsEquipment +
            m.SpeedDoingWork + m.QualityWorkmanship + m.Viva).ToList();
        var passed = totals.Count(t => t >= passMarks);
        var avg = totals.Any() ? (decimal)Math.Round(totals.Average(), 2) : 0;

        return Result<MonthlyPracticalReportDto>.Success(new MonthlyPracticalReportDto
        {
            MonthlyPracticalId = monthlyPracticalId, PracticalName = practical.Name,
            TradeName = practical.Trade?.Name, TradeCode = practical.Trade?.Code,
            Month = practical.Month, Year = practical.Year, TotalMarks = 100,
            PassMarks = passMarks, TotalStudents = marks.Count, PassedCount = passed,
            FailedCount = marks.Count - passed, AverageMarks = avg,
            HighestMarks = totals.Any() ? totals.Max() : 0,
            LowestMarks = totals.Any() ? totals.Min() : 0,
            PassPercentage = totals.Any() ? Math.Round((decimal)passed / totals.Count * 100, 2) : 0,
            Marks = marks.Select((m, i) => new PracticalMarkReportDto
            {
                StudentId = m.StudentId, StudentName = $"{m.Student.FirstName} {m.Student.LastName}",
                RollNumber = m.Student.RollNumber, MarksObtained = totals[i],
                IsPassed = totals[i] >= passMarks
            }).ToList()
        });
    }

    public async Task<Result<YearlyPracticalReportDto>> GetYearlyPracticalReportAsync(Guid yearlyPracticalId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        if (instituteId is null && !isSuperAdmin) return Result<YearlyPracticalReportDto>.Failure("Institute not found.");

        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(yearlyPracticalId, isMonthly: false, ct);
        if (!accessCheck.IsSuccess) return Result<YearlyPracticalReportDto>.Failure(accessCheck.Error!);

        var practicalQuery = _context.YearlyPracticals.AsNoTracking().Include(p => p.Trade)
            .Where(p => p.Id == yearlyPracticalId);

        if (!isSuperAdmin)
            practicalQuery = practicalQuery.Where(p => p.InstituteId == instituteId);

        var practical = await practicalQuery.FirstOrDefaultAsync(ct);
        if (practical is null) return Result<YearlyPracticalReportDto>.Failure("Yearly practical not found.");

        var marks = await _context.YearlyPracticalMarks.AsNoTracking().Include(m => m.Student)
            .Where(m => m.YearlyPracticalId == yearlyPracticalId).OrderBy(m => m.Student.RollNumber).ToListAsync(ct);

        var passed = marks.Count(m => (m.AnnualTotal ?? 0) >= practical.PassMarks);
        var avg = marks.Any() ? Math.Round(marks.Average(m => m.AnnualTotal ?? 0), 2) : 0;

        return Result<YearlyPracticalReportDto>.Success(new YearlyPracticalReportDto
        {
            YearlyPracticalId = yearlyPracticalId, PracticalName = practical.Name,
            TradeName = practical.Trade?.Name, TradeCode = practical.Trade?.Code,
            Year = practical.Year, TotalMarks = practical.TotalMarks,
            PassMarks = practical.PassMarks, TotalStudents = marks.Count, PassedCount = passed,
            FailedCount = marks.Count - passed, AverageMarks = avg,
            HighestMarks = marks.Any() ? marks.Max(m => m.AnnualTotal ?? 0) : 0,
            LowestMarks = marks.Any() ? marks.Min(m => m.AnnualTotal ?? 0) : 0,
            PassPercentage = marks.Any() ? Math.Round((decimal)passed / marks.Count * 100, 2) : 0,
            Marks = marks.Select(m => new PracticalMarkReportDto
            {
                StudentId = m.StudentId, StudentName = $"{m.Student.FirstName} {m.Student.LastName}",
                RollNumber = m.Student.RollNumber, MarksObtained = m.AnnualTotal ?? 0,
                IsPassed = (m.AnnualTotal ?? 0) >= practical.PassMarks
            }).ToList()
        });
    }

    public async Task<Result<StudentPerformanceReportDto>> GetStudentPerformanceReportAsync(Guid studentId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        if (instituteId is null && !isSuperAdmin) return Result<StudentPerformanceReportDto>.Failure("Institute not found.");

        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(studentId, ct);
        if (!accessCheck.IsSuccess) return Result<StudentPerformanceReportDto>.Failure(accessCheck.Error!);

        var studentQuery = _context.Students.AsNoTracking().Include(s => s.Trade)
            .Where(s => s.Id == studentId);

        if (!isSuperAdmin)
            studentQuery = studentQuery.Where(s => s.InstituteId == instituteId);

        var student = await studentQuery.FirstOrDefaultAsync(ct);
        if (student is null) return Result<StudentPerformanceReportDto>.Failure("Student not found.");

        var recordsQuery = _context.AttendanceRecords.AsNoTracking()
            .Where(a => a.StudentId == studentId && a.AcademicSessionId == student.AcademicSessionId);

        if (!isSuperAdmin)
            recordsQuery = recordsQuery.Where(a => a.InstituteId == instituteId);

        var records = await recordsQuery.ToListAsync(ct);

        var totalWorkingDays = records.Count;
        var presentDays = records.Count(r => r.Status == AttendanceStatus.Present);
        var lateDays = records.Count(r => r.Status == AttendanceStatus.Late);
        var attPct = totalWorkingDays > 0 ? Math.Round((decimal)(presentDays + lateDays) / totalWorkingDays * 100, 2) : 0;

        var monthlyMarks = await _context.PracticalMarks.AsNoTracking().Include(m => m.MonthlyPractical)
            .Where(m => m.StudentId == studentId && m.MonthlyPractical.AcademicSessionId == student.AcademicSessionId && m.MonthlyPractical.TradeId == student.TradeId)
            .OrderBy(m => m.MonthlyPractical.Month).ToListAsync(ct);

        var passMarksPercentage = 40;
        var settingsQuery = _context.InstituteSettings.AsNoTracking();
        if (!isSuperAdmin)
            settingsQuery = settingsQuery.Where(s => s.InstituteId == instituteId);
        var settings = await settingsQuery.FirstOrDefaultAsync(ct);
        if (settings is not null) passMarksPercentage = settings.PassMarksPercentage;

        var monthlyPracticals = monthlyMarks.Select(m =>
        {
            var totalObtained = m.SafetyConsciousness + m.WorkplaceHygiene + m.AttendancePunctuality +
                m.FollowInstructions + m.ApplicationKnowledge + m.SkillsToolsEquipment +
                m.SpeedDoingWork + m.QualityWorkmanship + m.Viva;
            return new MonthlyPracticalResultDto
            {
                PracticalName = m.MonthlyPractical.Name, Month = m.MonthlyPractical.Month, Year = m.MonthlyPractical.Year,
                TotalObtained = totalObtained, TotalMarks = 100,
                PassMarks = passMarksPercentage, IsPassed = totalObtained >= passMarksPercentage
            };
        }).ToList();

        var yearlyMark = await _context.YearlyPracticalMarks.AsNoTracking().Include(m => m.YearlyPractical)
            .FirstOrDefaultAsync(m => m.StudentId == studentId && m.YearlyPractical.AcademicSessionId == student.AcademicSessionId && m.YearlyPractical.TradeId == student.TradeId, ct);

        YearlyPracticalResultDto? yearlyPractical = yearlyMark is not null ? new YearlyPracticalResultDto
        {
            PracticalName = yearlyMark.YearlyPractical.Name, MarksObtained = yearlyMark.AnnualTotal ?? 0,
            TotalMarks = yearlyMark.YearlyPractical.TotalMarks, PassMarks = yearlyMark.YearlyPractical.PassMarks,
            IsPassed = (yearlyMark.AnnualTotal ?? 0) >= yearlyMark.YearlyPractical.PassMarks
        } : null;

        var monthlyAvg = monthlyPracticals.Any() ? Math.Round((decimal)monthlyPracticals.Average(m => m.TotalObtained), 2) : 0;
        var allPcts = new List<decimal>(monthlyPracticals.Select(m => (decimal)m.TotalObtained));
        if (yearlyPractical is not null) allPcts.Add((decimal)(yearlyPractical.MarksObtained / yearlyPractical.TotalMarks * 100));
        var overallAvg = allPcts.Any() ? Math.Round(allPcts.Average(), 2) : 0;

        return Result<StudentPerformanceReportDto>.Success(new StudentPerformanceReportDto
        {
            StudentId = studentId, StudentName = $"{student.FirstName} {student.LastName}",
            RollNumber = student.RollNumber, TradeCode = student.Trade?.Code, TradeName = student.Trade?.Name,
            Attendance = new StudentAttendanceSummaryDto
            {
                StudentId = studentId, StudentName = $"{student.FirstName} {student.LastName}", RollNumber = student.RollNumber,
                TotalDays = totalWorkingDays, PresentDays = presentDays, AbsentDays = records.Count(r => r.Status == AttendanceStatus.Absent),
                LateDays = lateDays, CLDays = records.Count(r => r.Status == AttendanceStatus.CL),
                ELDays = records.Count(r => r.Status == AttendanceStatus.EL), MLDays = records.Count(r => r.Status == AttendanceStatus.ML),
                HODays = records.Count(r => r.Status == AttendanceStatus.HO), AttendancePercentage = attPct
            },
            MonthlyPracticals = monthlyPracticals, YearlyPractical = yearlyPractical,
            OverallAttendancePercentage = attPct, MonthlyPracticalAverage = monthlyAvg, OverallPracticalAverage = overallAvg
        });
    }

    public async Task<Result<InstituteSummaryReportDto>> GetInstituteSummaryReportAsync(Guid instituteId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var currentUserInstituteId = _currentUserService.InstituteId;

        if (!isSuperAdmin && currentUserInstituteId != instituteId)
            return Result<InstituteSummaryReportDto>.Failure("Access denied: cannot view report for another institute.");

        var institute = await _context.Institutes.AsNoTracking().FirstOrDefaultAsync(i => i.Id == instituteId, ct);
        if (institute is null)
            return Result<InstituteSummaryReportDto>.Failure("Institute not found.");

        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);
        var effectiveTradeId = _tradeAccess.EffectiveTradeId;

        var session = academicSessionId.HasValue
            ? await _context.AcademicSessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == academicSessionId.Value, ct)
            : null;

        var studentQuery = _context.Students.AsNoTracking()
            .Where(s => s.InstituteId == instituteId);

        if (academicSessionId.HasValue && session is not null)
        {
            var allBatchesForInstitute = await _context.Batches
                .AsNoTracking()
                .Include(b => b.Trade)
                .Include(b => b.StartAcademicSession)
                .Where(b => b.InstituteId == instituteId && !b.IsDeleted)
                .ToListAsync(ct);

            var activeBatchIds = allBatchesForInstitute
                .Where(b => BatchYearLevelCalculator.IsBatchActiveInSession(b, b.Trade, session))
                .Select(b => b.Id)
                .ToList();

            studentQuery = studentQuery.Where(s => s.BatchId.HasValue && activeBatchIds.Contains(s.BatchId.Value));
        }

        if (effectiveTradeId.HasValue)
            studentQuery = studentQuery.Where(s => s.TradeId == effectiveTradeId.Value);

        var effectiveBatchId = _tradeAccess.EffectiveBatchId;
        if (_tradeAccess.IsTradeHead && effectiveBatchId.HasValue)
            studentQuery = studentQuery.Where(s => s.BatchId == effectiveBatchId.Value);

        var totalStudents = await studentQuery.CountAsync(ct);
        var activeStudents = await studentQuery.CountAsync(s => s.Status == StudentStatus.Active, ct);

        var tradeQuery = _context.Trades.AsNoTracking()
            .Where(t => t.InstituteId == instituteId);
        if (effectiveTradeId.HasValue)
            tradeQuery = tradeQuery.Where(t => t.Id == effectiveTradeId.Value);
        var totalTrades = await tradeQuery.CountAsync(ct);

        var totalUsers = await _context.Users.AsNoTracking()
            .CountAsync(u => u.InstituteId == instituteId, ct);

        var attQuery = _context.AttendanceRecords.AsNoTracking()
            .Where(a => a.InstituteId == instituteId);
        if (academicSessionId.HasValue)
            attQuery = attQuery.Where(a => a.AcademicSessionId == academicSessionId.Value);
        if (effectiveTradeId.HasValue)
            attQuery = attQuery.Where(a => a.TradeId == effectiveTradeId.Value);
        if (_tradeAccess.IsTradeHead && effectiveBatchId.HasValue)
            attQuery = attQuery.Where(a => a.BatchId == effectiveBatchId.Value);

        var attRecords = await attQuery.ToListAsync(ct);

        var totalAtt = attRecords.Count;
        var presentAtt = attRecords.Count(r => r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late);
        var attPct = totalAtt > 0 ? Math.Round((decimal)presentAtt / totalAtt * 100, 2) : 0;

        var trades = await tradeQuery.ToListAsync(ct);
        var tradeIds = trades.Select(t => t.Id).ToList();

        var studentCountsByTradeQuery = _context.Students
            .AsNoTracking()
            .Where(s => tradeIds.Contains(s.TradeId) && s.InstituteId == instituteId && s.Status == StudentStatus.Active);

        if (academicSessionId.HasValue && session is not null)
        {
            var allBatchesForTrades = await _context.Batches
                .AsNoTracking()
                .Include(b => b.Trade)
                .Include(b => b.StartAcademicSession)
                .Where(b => tradeIds.Contains(b.TradeId) && b.InstituteId == instituteId && !b.IsDeleted)
                .ToListAsync(ct);

            var activeTradeBatchIds = allBatchesForTrades
                .Where(b => BatchYearLevelCalculator.IsBatchActiveInSession(b, b.Trade, session))
                .Select(b => b.Id)
                .ToHashSet();

            studentCountsByTradeQuery = studentCountsByTradeQuery.Where(s => s.BatchId.HasValue && activeTradeBatchIds.Contains(s.BatchId.Value));
        }

        var studentCountsByTrade = await studentCountsByTradeQuery
            .GroupBy(s => s.TradeId)
            .Select(g => new { TradeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.TradeId, g => g.Count, ct);

        var attendanceByTrade = attRecords
            .GroupBy(a => a.TradeId)
            .ToDictionary(
                g => g.Key,
                g => (
                    Total: g.Count(),
                    Present: g.Count(r => r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late)
                ));

        var tradeSummaries = new List<TradeSummaryDto>();
        foreach (var trade in trades)
        {
            var tradeStudents = studentCountsByTrade.GetValueOrDefault(trade.Id, 0);
            var attStats = attendanceByTrade.GetValueOrDefault(trade.Id, (Total: 0, Present: 0));
            var tradeAttPct = attStats is { Total: > 0 }
                ? Math.Round((decimal)attStats.Present / attStats.Total * 100, 2)
                : 0;

            tradeSummaries.Add(new TradeSummaryDto
            {
                TradeId = trade.Id, TradeName = trade.Name, TradeCode = trade.Code,
                TotalStudents = tradeStudents, ActiveStudents = tradeStudents, TotalSeats = trade.TotalSeats,
                AttendancePercentage = tradeAttPct
            });
        }

        IQueryable<PracticalMark> pracMarkQuery = _context.PracticalMarks
            .Include(m => m.MonthlyPractical)
            .Where(m => m.MonthlyPractical.InstituteId == instituteId);
        if (academicSessionId.HasValue)
            pracMarkQuery = pracMarkQuery.Where(m => m.MonthlyPractical.AcademicSessionId == academicSessionId.Value);
        if (effectiveTradeId.HasValue)
            pracMarkQuery = pracMarkQuery.Where(m => m.MonthlyPractical.TradeId == effectiveTradeId.Value);

        var pracs = academicSessionId.HasValue
            ? await pracMarkQuery.ToListAsync(ct)
            : new List<Domain.Entities.PracticalMark>();

        var pracPassMarks = 40;
        var pracSettings = await _context.InstituteSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.InstituteId == instituteId, ct);
        if (pracSettings is not null) pracPassMarks = pracSettings.PassMarksPercentage;

        var pracPass = pracs.Count(m =>
            (m.SafetyConsciousness + m.WorkplaceHygiene + m.AttendancePunctuality +
             m.FollowInstructions + m.ApplicationKnowledge + m.SkillsToolsEquipment +
             m.SpeedDoingWork + m.QualityWorkmanship + m.Viva) >= pracPassMarks);
        var pracPct = pracs.Any() ? Math.Round((decimal)pracPass / pracs.Count * 100, 2) : 0;
        var attThreshold = await GetAttendanceThresholdAsync(instituteId, ct);

        return Result<InstituteSummaryReportDto>.Success(new InstituteSummaryReportDto
        {
            InstituteName = institute.Name, GRNumber = institute.GRNumber, SessionYear = session?.SessionYear,
            TotalStudents = totalStudents, ActiveStudents = activeStudents, TotalTrades = totalTrades,
            TotalUsers = totalUsers, OverallAttendancePercentage = attPct,
            AttendanceThresholdPercentage = attThreshold,
            OverallPracticalPassPercentage = pracPct, Trades = tradeSummaries
        });
    }

    public async Task<Result<ProgressiveAttendanceReportDto>> GetProgressiveAttendanceReportAsync(Guid studentId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        if (instituteId is null && !isSuperAdmin)
            return Result<ProgressiveAttendanceReportDto>.Failure("Institute not found.");

        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(studentId, ct);
        if (!accessCheck.IsSuccess)
            return Result<ProgressiveAttendanceReportDto>.Failure(accessCheck.Error!);

        var student = await _context.Students
            .AsNoTracking()
            .Include(s => s.Trade)
            .Include(s => s.AcademicSession)
            .FirstOrDefaultAsync(s => s.Id == studentId && !s.IsDeleted, ct);

        if (student is null)
            return Result<ProgressiveAttendanceReportDto>.Failure("Student not found.");

        if (!isSuperAdmin && student.InstituteId != instituteId)
            return Result<ProgressiveAttendanceReportDto>.Failure("Access denied.");

        var academicSession = student.AcademicSession;
        if (academicSession is null)
            return Result<ProgressiveAttendanceReportDto>.Failure("Academic session not found for this student.");

        var sessionStart = academicSession.StartDate.Date;
        var sessionEnd = academicSession.EndDate.Date;
        var currentDate = DateTime.UtcNow.Date;
        var currentMonth = currentDate.Month;
        var currentYear = currentDate.Year;

        var recordsQuery = _context.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.StudentId == studentId && a.Date >= sessionStart && a.Date <= sessionEnd);

        if (!isSuperAdmin)
            recordsQuery = recordsQuery.Where(a => a.InstituteId == instituteId);

        var records = await recordsQuery.ToListAsync(ct);

        var holidaysQuery = _context.Holidays
            .AsNoTracking()
            .Where(h => h.AcademicSessionId == academicSession.Id && !h.IsDeleted);
        if (!isSuperAdmin)
            holidaysQuery = holidaysQuery.Where(h => h.InstituteId == instituteId);
        var holidays = await holidaysQuery.Select(h => h.Date.Date).ToListAsync(ct);
        var holidaysByMonth = holidays.GroupBy(h => new { h.Month, h.Year })
            .ToDictionary(g => g.Key, g => g.Count());

        var months = new List<MonthlyAttendanceBreakdownDto>();
        var sessionStartDate = new DateTime(academicSession.StartDate.Year, academicSession.StartDate.Month, 1);
        var iterateDate = sessionStartDate;

        while (iterateDate <= sessionEnd && iterateDate <= currentDate)
        {
            var monthStart = iterateDate;
            var monthEnd = iterateDate.AddMonths(1).AddDays(-1);
            var isCurrentMonth = monthStart.Month == currentMonth && monthStart.Year == currentYear;

            var monthRecords = records.Where(r => r.Date.Date >= monthStart.Date && r.Date.Date <= monthEnd.Date).ToList();

            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
            var holidayKey = new { Month = monthStart.Month, Year = monthStart.Year };
            var holidaysThisMonth = holidaysByMonth.GetValueOrDefault(holidayKey, 0);
            var workingDays = daysInMonth - holidaysThisMonth;
            var present = monthRecords.Count(r => r.Status == AttendanceStatus.Present);
            var absent = monthRecords.Count(r => r.Status == AttendanceStatus.Absent);
            var late = monthRecords.Count(r => r.Status == AttendanceStatus.Late);
            var cl = monthRecords.Count(r => r.Status == AttendanceStatus.CL);
            var el = monthRecords.Count(r => r.Status == AttendanceStatus.EL);
            var ml = monthRecords.Count(r => r.Status == AttendanceStatus.ML);
            var ho = monthRecords.Count(r => r.Status == AttendanceStatus.HO);

            var pct = workingDays > 0 ? Math.Round((decimal)(present + late) / workingDays * 100, 2) : 0;

            months.Add(new MonthlyAttendanceBreakdownDto
            {
                Month = iterateDate.Month,
                MonthName = iterateDate.ToString("MMMM yyyy"),
                WorkingDays = workingDays,
                Present = present,
                Absent = absent,
                Late = late,
                CL = cl,
                EL = el,
                ML = ml,
                HO = ho,
                AttendancePercentage = pct
            });

            iterateDate = iterateDate.AddMonths(1);
        }

        var totalWorkingDays = months.Sum(m => m.WorkingDays);
        var totalPresent = records.Count(r => r.Status == AttendanceStatus.Present);
        var totalAbsent = records.Count(r => r.Status == AttendanceStatus.Absent);
        var totalLate = records.Count(r => r.Status == AttendanceStatus.Late);
        var totalCL = records.Count(r => r.Status == AttendanceStatus.CL);
        var totalEL = records.Count(r => r.Status == AttendanceStatus.EL);
        var totalML = records.Count(r => r.Status == AttendanceStatus.ML);
        var totalHO = records.Count(r => r.Status == AttendanceStatus.HO);
        var cumulativePct = totalWorkingDays > 0 ? Math.Round((decimal)(totalPresent + totalLate) / totalWorkingDays * 100, 2) : 0;
        var threshold = await GetAttendanceThresholdAsync(instituteId, ct);

        return Result<ProgressiveAttendanceReportDto>.Success(new ProgressiveAttendanceReportDto
        {
            StudentId = studentId,
            StudentName = $"{student.FirstName} {student.LastName}",
            RollNumber = student.RollNumber,
            TradeCode = student.Trade?.Code,
            TradeName = student.Trade?.Name,
            SessionYear = academicSession.SessionYear,
            SessionStartDate = sessionStart,
            AttendanceThresholdPercentage = threshold,
            Months = months,
            Cumulative = new CumulativeAttendanceDto
            {
                TotalWorkingDays = totalWorkingDays,
                TotalPresent = totalPresent,
                TotalAbsent = totalAbsent,
                TotalLate = totalLate,
                TotalCL = totalCL,
                TotalEL = totalEL,
                TotalML = totalML,
                TotalHO = totalHO,
                CumulativePercentage = cumulativePct
            }
        });
    }

    public async Task<Result<ProgressCardDto>> GetProgressCardAsync(Guid studentId, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        if (instituteId is null && !isSuperAdmin)
            return Result<ProgressCardDto>.Failure("Institute not found.");

        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(studentId, ct);
        if (!accessCheck.IsSuccess)
            return Result<ProgressCardDto>.Failure(accessCheck.Error!);

        var student = await _context.Students
            .AsNoTracking()
            .Include(s => s.Trade)
            .Include(s => s.Batch).ThenInclude(b => b!.StartAcademicSession)
            .Include(s => s.AcademicSession)
            .FirstOrDefaultAsync(s => s.Id == studentId && !s.IsDeleted, ct);

        if (student is null)
            return Result<ProgressCardDto>.Failure("Student not found.");
        if (!isSuperAdmin && student.InstituteId != instituteId)
            return Result<ProgressCardDto>.Failure("Access denied.");
        if (student.BatchId is null || student.Batch is null)
            return Result<ProgressCardDto>.Failure("Student is not assigned to a batch.");

        var institute = await _context.Institutes.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == student.InstituteId, ct);
        var settings = await _context.InstituteSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.InstituteId == student.InstituteId, ct);

        var currentSession = student.AcademicSession;
        if (currentSession is null)
            return Result<ProgressCardDto>.Failure("Academic session not found.");

        var yearResult = BatchYearLevelCalculator.Calculate(student.Batch, student.Trade!, currentSession);
        var yearLevel = yearResult.YearLevel != null ? (int)yearResult.YearLevel.Value : 1;

        var startMonth = student.Batch.StartDate.Month;
        var startYear = student.Batch.StartDate.Year;
        var monthPeriods = Enumerable.Range(0, 12).Select(i =>
        {
            var m = ((startMonth - 1 + i) % 12) + 1;
            var y = startYear + ((startMonth - 1 + i) / 12);
            return (Month: m, Year: y, MonthName: new DateTime(y, m, 1).ToString("MMM"));
        }).ToList();

        var monthlyPracticals = await _context.MonthlyPracticals.AsNoTracking()
            .Where(mp => mp.BatchId == student.BatchId!.Value
                      && mp.TradeId == student.TradeId
                      && mp.AcademicSessionId == student.AcademicSessionId)
            .OrderBy(mp => mp.Month).ThenBy(mp => mp.Year)
            .ToListAsync(ct);

        var practicalMarks = await _context.PracticalMarks.AsNoTracking()
            .Include(pm => pm.MonthlyPractical)
            .Where(pm => pm.StudentId == studentId
                      && pm.MonthlyPractical.BatchId == student.BatchId!.Value
                      && pm.MonthlyPractical.TradeId == student.TradeId
                      && pm.MonthlyPractical.AcademicSessionId == student.AcademicSessionId)
            .ToListAsync(ct);

        var attendanceRecords = await _context.AttendanceRecords.AsNoTracking()
            .Where(a => a.StudentId == studentId
                      && a.AcademicSessionId == student.AcademicSessionId
                      && a.InstituteId == student.InstituteId)
            .ToListAsync(ct);

        var holidays = await _context.Holidays.AsNoTracking()
            .Where(h => h.AcademicSessionId == student.AcademicSessionId
                      && h.InstituteId == student.InstituteId
                      && !h.IsDeleted)
            .ToListAsync(ct);
        var holidaysByDate = holidays.Select(h => h.Date.Date).ToHashSet();

        static int ComputePracticalTotal(PracticalMark? mark)
        {
            if (mark is null) return 0;
            return mark.SafetyConsciousness + mark.WorkplaceHygiene + mark.AttendancePunctuality
                 + mark.FollowInstructions + mark.ApplicationKnowledge + mark.SkillsToolsEquipment
                 + mark.SpeedDoingWork + mark.QualityWorkmanship + mark.Viva;
        }

        var monthlyPracticalDtos = monthlyPracticals
            .Select(mp =>
            {
                var mark = practicalMarks.FirstOrDefault(pm => pm.MonthlyPracticalId == mp.Id);
                int? weekNumber = mp.StartDate.HasValue
                    ? ((mp.StartDate.Value.Day - 1) / 7) + 1
                    : null;
                return new ProgressCardMonthlyPracticalDto
                {
                    Month = mp.Month,
                    Year = mp.Year,
                    MonthName = mp.StartDate.HasValue
                        ? mp.StartDate.Value.ToString("MMM yyyy")
                        : $"{new DateTime(mp.Year, mp.Month, 1):MMM} {mp.Year}",
                    WeekNumber = weekNumber,
                    PracticalName = mp.Name,
                    ProfessionalSkillName = mp.ProfessionalSkillName,
                    TotalObtained = ComputePracticalTotal(mark),
                    TotalMarks = 100
                };
            })
            .ToList();

        var monthlyMarkDtos = monthPeriods.Select(period =>
        {
            var mp = monthlyPracticals.FirstOrDefault(x => x.Month == period.Month && x.Year == period.Year);
            var mark = mp is not null
                ? practicalMarks.FirstOrDefault(pm => pm.MonthlyPracticalId == mp.Id)
                : null;
            var rawTotal = ComputePracticalTotal(mark);
            return new ProgressCardMonthlyMarkDto
            {
                Month = period.Month,
                Year = period.Year,
                MonthName = $"{period.MonthName} {period.Year}",
                PracticalMarks = rawTotal,
                PracticalMarksScaled = rawTotal * 3,
                PartATT = 0,
                PartBES = 0,
                PartsRecalSci = 0,
                EngDrg = 0
            };
        }).ToList();

        var quarterDtos = new List<ProgressCardQuarterlyAssessmentDto>();
        for (int q = 0; q < 4; q++)
        {
            var qPeriods = monthPeriods.Skip(q * 3).Take(3).ToList();
            int possibleDays = 0;
            int workingDays = 0;
            int presentLate = 0;

            foreach (var (m, y, _) in qPeriods)
            {
                var daysInMonth = DateTime.DaysInMonth(y, m);
                var holidaysInMonth = holidaysByDate.Count(h => h.Month == m && h.Year == y);
                possibleDays += daysInMonth - holidaysInMonth;

                var monthRecords = attendanceRecords
                    .Where(a => a.Date.Month == m && a.Date.Year == y).ToList();
                workingDays += monthRecords.Count(r => r.Status != AttendanceStatus.HO);
                presentLate += monthRecords.Count(r =>
                    r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late);
            }

            var pct = workingDays > 0
                ? Math.Round((decimal)presentLate / workingDays * 100, 2) : 0;

            quarterDtos.Add(new ProgressCardQuarterlyAssessmentDto
            {
                Quarter = q + 1,
                PossibleDays = possibleDays,
                WorkingDays = workingDays,
                AttendancePercentage = pct,
                SessionalPRT = 0,
                SessionalTT = 0,
                SessionalWCalSci = 0,
                SessionalEngDrg = 0
            });
        }

        return Result<ProgressCardDto>.Success(new ProgressCardDto
        {
            InstituteName = settings?.Institute?.Name ?? institute?.Name,
            InstituteAddress = settings?.Address ?? institute?.Address,
            InstituteLogoPath = settings?.LogoPath ?? institute?.LogoPath,
            StudentName = student.GetFullName(),
            MotherName = student.MotherName,
            FatherName = student.FatherName,
            DateOfBirth = student.DateOfBirth,
            AdmissionDate = student.AdmissionDate,
            AdmissionNumber = student.AdmissionNumber,
            RollNumber = student.RollNumber,
            Religion = student.CasteCategory,
            Category = student.CasteCategory,
            EducationQualification = student.PreviousQualification,
            Address = student.Address,
            Phone = student.Phone,
            AadharNumber = student.AadharNumber,
            Gender = student.Gender.ToString(),
            TradeName = student.Trade?.Name,
            TradeCode = student.Trade?.Code,
            DurationInMonths = student.Trade?.DurationInMonths ?? 0,
            YearLevel = yearLevel,
            YearLevelLabel = yearResult.Label,
            MonthlyPracticals = monthlyPracticalDtos,
            MonthlyMarks = monthlyMarkDtos,
            QuarterlyAssessments = quarterDtos
        });
    }
}
