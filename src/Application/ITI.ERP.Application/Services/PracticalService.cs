using ITI.ERP.Application.Common.Helpers;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Practical;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class PracticalService : IPracticalService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly ITradeAccessService _tradeAccess;

    private const int NSQF_MAX_SAFETY = 15;
    private const int NSQF_MAX_HYGIENE = 10;
    private const int NSQF_MAX_ATTENDANCE = 10;
    private const int NSQF_MAX_INSTRUCTIONS = 5;
    private const int NSQF_MAX_KNOWLEDGE = 10;
    private const int NSQF_MAX_TOOLS = 10;
    private const int NSQF_MAX_SPEED = 10;
    private const int NSQF_MAX_QUALITY = 15;
    private const int NSQF_MAX_VIVA = 15;
    private const int NSQF_TOTAL_MARKS = 100;

    private static readonly Dictionary<int, string> MonthNames = new()
    {
        { 1, "January" }, { 2, "February" }, { 3, "March" }, { 4, "April" },
        { 5, "May" }, { 6, "June" }, { 7, "July" }, { 8, "August" },
        { 9, "September" }, { 10, "October" }, { 11, "November" }, { 12, "December" }
    };

    public PracticalService(
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

    private static int ComputeTotalObtainedFromDto(StudentPracticalMarkItem m) =>
        m.SafetyConsciousness + m.WorkplaceHygiene + m.AttendancePunctuality +
        m.FollowInstructions + m.ApplicationKnowledge + m.SkillsToolsEquipment +
        m.SpeedDoingWork + m.QualityWorkmanship + m.Viva;

    public async Task<Result<PaginatedList<MonthlyPracticalDto>>> GetMonthlyPracticalsAsync(PaginationRequest request, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

        if (instituteId is null || academicSessionId is null)
            return Result<PaginatedList<MonthlyPracticalDto>>.Failure("Institute or Academic Session not found.");

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var passMarks = await GetPassMarksAsync(ct);

        IQueryable<MonthlyPractical> query = _context.MonthlyPracticals
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
            "month" => request.SortDescending ? query.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month) : query.OrderBy(p => p.Year).ThenBy(p => p.Month),
            _ => query.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
        };

        var paginatedList = await PaginatedList<MonthlyPracticalDto>.CreateAsync(
            query.Select(p => new MonthlyPracticalDto
            {
                Id = p.Id,
                InstituteId = p.InstituteId,
                AcademicSessionId = p.AcademicSessionId,
                TradeId = p.TradeId,
                BatchId = p.BatchId,
                TradeName = p.Trade.Name,
                TradeCode = p.Trade.Code,
                Month = p.Month,
                Year = p.Year,
                Name = p.Name,
                Description = p.Description,
                AssessorName = p.AssessorName,
                LearningOutcome = p.LearningOutcome,
                ProfessionalSkillName = p.ProfessionalSkillName,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsLocked = p.IsLocked,
                PassMarks = passMarks,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }),
            request.PageNumber,
            request.PageSize);

        if (paginatedList.Items.Count > 0)
        {
            var practicalIds = paginatedList.Items.Select(p => p.Id).ToList();

            var marksCounts = await _context.PracticalMarks
                .Where(m => practicalIds.Contains(m.MonthlyPracticalId))
                .GroupBy(m => m.MonthlyPracticalId)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count, ct);

            var batchIds = paginatedList.Items.Select(p => p.BatchId).Distinct().ToList();
            var tradeIds = paginatedList.Items.Select(p => p.TradeId).Distinct().ToList();

            var batchStudentCounts = await _context.Students
                .Where(s =>
                    s.Status == StudentStatus.Active &&
                    s.AcademicSessionId == academicSessionId &&
                    batchIds.Contains(s.BatchId ?? Guid.Empty) &&
                    tradeIds.Contains(s.TradeId))
                .GroupBy(s => new { s.TradeId, s.BatchId })
                .Select(g => new { Key = $"{g.Key.TradeId}_{g.Key.BatchId!.Value}", Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

            foreach (var item in paginatedList.Items)
            {
                item.MarksEnteredCount = marksCounts.GetValueOrDefault(item.Id, 0);
                var key = $"{item.TradeId}_{item.BatchId}";
                item.TotalStudents = batchStudentCounts.GetValueOrDefault(key, 0);
            }
        }

        return Result<PaginatedList<MonthlyPracticalDto>>.Success(paginatedList);
    }

    public async Task<Result<MonthlyPracticalDto>> GetMonthlyPracticalByIdAsync(Guid id, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(id, isMonthly: true, ct);
        if (!accessCheck.IsSuccess)
            return Result<MonthlyPracticalDto>.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<MonthlyPracticalDto>.Failure("Institute not found.");

        IQueryable<MonthlyPractical> query = _context.MonthlyPracticals
            .AsNoTracking()
            .Include(p => p.Trade);

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (practical is null)
            return Result<MonthlyPracticalDto>.Failure("Monthly practical not found.");

        var marksCount = await _context.PracticalMarks
            .CountAsync(m => m.MonthlyPracticalId == id, ct);

        var studentsQuery = _context.Students
            .Where(s =>
                s.AcademicSessionId == practical.AcademicSessionId &&
                s.TradeId == practical.TradeId &&
                s.BatchId == practical.BatchId &&
                s.Status == StudentStatus.Active);

        if (!isSuperAdmin) studentsQuery = studentsQuery.Where(s => s.InstituteId == instituteId);

        var totalStudents = await studentsQuery.CountAsync(ct);

        var passMarks = await GetPassMarksAsync(ct);

        return Result<MonthlyPracticalDto>.Success(new MonthlyPracticalDto
        {
            Id = practical.Id,
            InstituteId = practical.InstituteId,
            AcademicSessionId = practical.AcademicSessionId,
            TradeId = practical.TradeId,
            BatchId = practical.BatchId,
            TradeName = practical.Trade.Name,
            TradeCode = practical.Trade.Code,
            Month = practical.Month,
            Year = practical.Year,
            Name = practical.Name,
            Description = practical.Description,
            AssessorName = practical.AssessorName,
            LearningOutcome = practical.LearningOutcome,
            ProfessionalSkillName = practical.ProfessionalSkillName,
            StartDate = practical.StartDate,
            EndDate = practical.EndDate,
            IsLocked = practical.IsLocked,
            MarksEnteredCount = marksCount,
            TotalStudents = totalStudents,
            PassMarks = passMarks,
            CreatedAt = practical.CreatedAt,
            UpdatedAt = practical.UpdatedAt
        });
    }

    public async Task<Result<MonthlyPracticalDto>> CreateMonthlyPracticalAsync(CreateMonthlyPracticalRequest request, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

        if (instituteId is null || academicSessionId is null)
            return Result<MonthlyPracticalDto>.Failure("Institute or Academic Session not found.");

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        if (_tradeAccess.IsTradeHead)
        {
            if (!_tradeAccess.EffectiveTradeId.HasValue)
                return Result<MonthlyPracticalDto>.Failure("TradeHead trade assignment is not configured.");
            request.TradeId = _tradeAccess.EffectiveTradeId.Value;
        }

        Guid effectiveBatchId;
        if (_tradeAccess.IsTradeHead)
        {
            if (!_tradeAccess.EffectiveBatchId.HasValue)
                return Result<MonthlyPracticalDto>.Failure("TradeHead batch assignment is not configured.");
            effectiveBatchId = _tradeAccess.EffectiveBatchId.Value;
        }
        else
        {
            if (!request.BatchId.HasValue)
                return Result<MonthlyPracticalDto>.Failure("BatchId is required.");
            effectiveBatchId = request.BatchId.Value;
        }

        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == effectiveBatchId, ct);
        if (batch is null || !batch.IsActive)
            return Result<MonthlyPracticalDto>.Failure("Cannot create practical for an archived batch.");

        if (request.Month < 1 || request.Month > 12)
            return Result<MonthlyPracticalDto>.Failure("Month must be between 1 and 12.");

        if (request.Year < 2000 || request.Year > 2100)
            return Result<MonthlyPracticalDto>.Failure("Year must be between 2000 and 2100.");

        if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
            return Result<MonthlyPracticalDto>.Failure("Date of Completion must be on or after Date of Starting.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<MonthlyPracticalDto>.Failure("Name is required.");

        if (string.IsNullOrWhiteSpace(request.ProfessionalSkillName))
            return Result<MonthlyPracticalDto>.Failure("Professional Skill Name is required.");

        var tradeQuery = _context.Trades
            .Where(t => t.Id == request.TradeId && !t.IsDeleted);

        if (!isSuperAdmin) tradeQuery = tradeQuery.Where(t => t.InstituteId == instituteId);

        if (!await tradeQuery.AnyAsync(ct))
            return Result<MonthlyPracticalDto>.Failure("Trade not found.");

        var trimmedSkillName = request.ProfessionalSkillName.Trim();
        var duplicateQuery = _context.MonthlyPracticals
            .Where(p =>
                p.AcademicSessionId == academicSessionId &&
                p.TradeId == request.TradeId &&
                p.Month == request.Month &&
                p.Year == request.Year &&
                p.ProfessionalSkillName != null &&
                p.ProfessionalSkillName.Trim().ToLower() == trimmedSkillName.ToLower());

        if (!isSuperAdmin) duplicateQuery = duplicateQuery.Where(p => p.InstituteId == instituteId);

        if (await duplicateQuery.AnyAsync(ct))
            return Result<MonthlyPracticalDto>.Failure("A practical already exists for this trade, month/year, and professional skill.");

        if (!academicSessionId.HasValue) return Result<MonthlyPracticalDto>.Failure("No active academic session found.");

        var practical = new MonthlyPractical
        {
            InstituteId = instituteId.Value,
            AcademicSessionId = academicSessionId.Value,
            TradeId = request.TradeId,
            BatchId = effectiveBatchId,
            Month = request.Month,
            Year = request.Year,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            AssessorName = request.AssessorName?.Trim(),
            LearningOutcome = request.LearningOutcome?.Trim(),
            ProfessionalSkillName = trimmedSkillName,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        await _context.MonthlyPracticals.AddAsync(practical, ct);
        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.Create, nameof(MonthlyPractical), practical.Id, null, practical, ct);

        practical = await _context.MonthlyPracticals
            .AsNoTracking()
            .Include(p => p.Trade)
            .FirstOrDefaultAsync(p => p.Id == practical.Id, ct);

        var passMarks = await GetPassMarksAsync(ct);

        return Result<MonthlyPracticalDto>.Success(new MonthlyPracticalDto
        {
            Id = practical!.Id,
            InstituteId = practical.InstituteId,
            AcademicSessionId = practical.AcademicSessionId,
            TradeId = practical.TradeId,
            BatchId = practical.BatchId,
            TradeName = practical.Trade.Name,
            TradeCode = practical.Trade.Code,
            Month = practical.Month,
            Year = practical.Year,
            Name = practical.Name,
            Description = practical.Description,
            AssessorName = practical.AssessorName,
            LearningOutcome = practical.LearningOutcome,
            ProfessionalSkillName = practical.ProfessionalSkillName,
            StartDate = practical.StartDate,
            EndDate = practical.EndDate,
            IsLocked = practical.IsLocked,
            PassMarks = passMarks,
            CreatedAt = practical.CreatedAt,
            UpdatedAt = practical.UpdatedAt
        });
    }

    public async Task<Result<MonthlyPracticalDto>> UpdateMonthlyPracticalAsync(Guid id, UpdateMonthlyPracticalRequest request, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(id, isMonthly: true, ct);
        if (!accessCheck.IsSuccess)
            return Result<MonthlyPracticalDto>.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<MonthlyPracticalDto>.Failure("Institute not found.");

        IQueryable<MonthlyPractical> query = _context.MonthlyPracticals
            .Include(p => p.Trade);

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (practical is null)
            return Result<MonthlyPracticalDto>.Failure("Monthly practical not found.");

        if (practical.IsLocked)
            return Result<MonthlyPracticalDto>.Failure("Cannot edit a locked practical.");

        var oldValues = new { practical.Name, practical.Description };

        if (request.Name is not null) practical.Name = request.Name.Trim();
        if (request.Description is not null) practical.Description = request.Description.Trim();

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.Update, nameof(MonthlyPractical), practical.Id, oldValues, practical, ct);

        var passMarks = await GetPassMarksAsync(ct);

        return Result<MonthlyPracticalDto>.Success(new MonthlyPracticalDto
        {
            Id = practical.Id,
            InstituteId = practical.InstituteId,
            AcademicSessionId = practical.AcademicSessionId,
            TradeId = practical.TradeId,
            BatchId = practical.BatchId,
            TradeName = practical.Trade.Name,
            TradeCode = practical.Trade.Code,
            Month = practical.Month,
            Year = practical.Year,
            Name = practical.Name,
            Description = practical.Description,
            AssessorName = practical.AssessorName,
            LearningOutcome = practical.LearningOutcome,
            ProfessionalSkillName = practical.ProfessionalSkillName,
            StartDate = practical.StartDate,
            EndDate = practical.EndDate,
            IsLocked = practical.IsLocked,
            PassMarks = passMarks,
            CreatedAt = practical.CreatedAt,
            UpdatedAt = practical.UpdatedAt
        });
    }

    public async Task<Result> DeleteMonthlyPracticalAsync(Guid id, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(id, isMonthly: true, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        var userId = _currentUserService.UserId;

        if (instituteId is null || userId is null)
            return Result.Failure("Institute or User not found.");

        IQueryable<MonthlyPractical> query = _context.MonthlyPracticals;

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (practical is null)
            return Result.Failure("Monthly practical not found.");

        if (practical.IsLocked)
            return Result.Failure("Cannot delete a locked practical.");

        var hasMarks = await _context.PracticalMarks
            .AnyAsync(m => m.MonthlyPracticalId == id, ct);

        if (hasMarks)
            return Result.Failure("Cannot delete practical that has marks entered. Remove all marks first.");

        practical.IsDeleted = true;

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.Delete, nameof(MonthlyPractical), practical.Id,
            new { practical.Name, practical.Month, practical.Year }, null, ct);

        return Result.Success();
    }

    public async Task<Result<List<PracticalMarkDto>>> GetPracticalMarksAsync(Guid monthlyPracticalId, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(monthlyPracticalId, isMonthly: true, ct);
        if (!accessCheck.IsSuccess)
            return Result<List<PracticalMarkDto>>.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<List<PracticalMarkDto>>.Failure("Institute not found.");

        var practicalQuery = _context.MonthlyPracticals
            .AsNoTracking();

        if (!isSuperAdmin) practicalQuery = practicalQuery.Where(p => p.InstituteId == instituteId);

        var practical = await practicalQuery.FirstOrDefaultAsync(p => p.Id == monthlyPracticalId, ct);

        if (practical is null)
            return Result<List<PracticalMarkDto>>.Failure("Monthly practical not found.");

        var passMarks = await GetPassMarksAsync(ct);

        var marks = await _context.PracticalMarks
            .AsNoTracking()
            .Include(m => m.Student)
            .Include(m => m.MarkedByUser)
            .Where(m => m.MonthlyPracticalId == monthlyPracticalId)
            .OrderBy(m => m.Student.RollNumber)
            .Select(m => new PracticalMarkDto
            {
                Id = m.Id,
                MonthlyPracticalId = m.MonthlyPracticalId,
                StudentId = m.StudentId,
                StudentName = $"{m.Student.FirstName} {m.Student.LastName}",
                RollNumber = m.Student.RollNumber,
                SafetyConsciousness = m.SafetyConsciousness,
                WorkplaceHygiene = m.WorkplaceHygiene,
                AttendancePunctuality = m.AttendancePunctuality,
                FollowInstructions = m.FollowInstructions,
                ApplicationKnowledge = m.ApplicationKnowledge,
                SkillsToolsEquipment = m.SkillsToolsEquipment,
                SpeedDoingWork = m.SpeedDoingWork,
                QualityWorkmanship = m.QualityWorkmanship,
                Viva = m.Viva,
                TotalObtained = m.SafetyConsciousness + m.WorkplaceHygiene + m.AttendancePunctuality +
                    m.FollowInstructions + m.ApplicationKnowledge + m.SkillsToolsEquipment +
                    m.SpeedDoingWork + m.QualityWorkmanship + m.Viva,
                TotalMarks = NSQF_TOTAL_MARKS,
                IsPassed = (m.SafetyConsciousness + m.WorkplaceHygiene + m.AttendancePunctuality +
                    m.FollowInstructions + m.ApplicationKnowledge + m.SkillsToolsEquipment +
                    m.SpeedDoingWork + m.QualityWorkmanship + m.Viva) >= passMarks,
                SignedByTrainee = m.SignedByTrainee,
                Remarks = m.Remarks,
                MarkedByUserName = $"{m.MarkedByUser.FirstName} {m.MarkedByUser.LastName}",
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(ct);

        if (marks.Count == 0)
        {
            var studentsQuery = _context.Students
                .AsNoTracking()
                .Where(s => s.AcademicSessionId == practical.AcademicSessionId
                    && s.TradeId == practical.TradeId
                    && s.BatchId == practical.BatchId
                    && s.Status == StudentStatus.Active);

            if (!isSuperAdmin) studentsQuery = studentsQuery.Where(s => s.InstituteId == instituteId);

            var students = await studentsQuery
                .OrderBy(s => s.RollNumber)
                .Select(s => new PracticalMarkDto
                {
                    StudentId = s.Id,
                    StudentName = $"{s.FirstName} {s.LastName}",
                    RollNumber = s.RollNumber,
                    TotalMarks = NSQF_TOTAL_MARKS
                })
                .ToListAsync(ct);

            return Result<List<PracticalMarkDto>>.Success(students);
        }

        return Result<List<PracticalMarkDto>>.Success(marks);
    }

    public async Task<Result> SubmitPracticalMarksAsync(SubmitPracticalMarksRequest request, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(request.MonthlyPracticalId, isMonthly: true, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);
        var userId = _currentUserService.UserId;

        if (instituteId is null || academicSessionId is null || userId is null)
            return Result.Failure("Institute, Academic Session, or User not found.");

        IQueryable<MonthlyPractical> practicalQuery = _context.MonthlyPracticals;

        if (!isSuperAdmin) practicalQuery = practicalQuery.Where(p => p.InstituteId == instituteId);

        var practical = await practicalQuery
            .FirstOrDefaultAsync(p => p.Id == request.MonthlyPracticalId, ct);

        if (practical is null)
            return Result.Failure("Monthly practical not found.");

        if (practical.IsLocked)
            return Result.Failure("Cannot submit marks for a locked practical.");

        var studentIds = request.Students.Select(s => s.StudentId).ToList();

        var validStudentsQuery = _context.Students
            .Where(s =>
                studentIds.Contains(s.Id) &&
                s.AcademicSessionId == practical.AcademicSessionId &&
                s.TradeId == practical.TradeId &&
                s.BatchId == practical.BatchId &&
                s.Status == StudentStatus.Active);

        if (!isSuperAdmin) validStudentsQuery = validStudentsQuery.Where(s => s.InstituteId == instituteId);

        var validStudents = await validStudentsQuery
            .Select(s => s.Id)
            .ToListAsync(ct);

        var invalidIds = studentIds.Except(validStudents).ToList();
        if (invalidIds.Any())
            return Result.Failure($"Invalid or inactive student IDs: {string.Join(", ", invalidIds)}");

        foreach (var item in request.Students)
        {
            if (item.SafetyConsciousness < 0 || item.SafetyConsciousness > NSQF_MAX_SAFETY)
                return Result.Failure($"Safety consciousness must be between 0 and {NSQF_MAX_SAFETY}.");
            if (item.WorkplaceHygiene < 0 || item.WorkplaceHygiene > NSQF_MAX_HYGIENE)
                return Result.Failure($"Workplace hygiene must be between 0 and {NSQF_MAX_HYGIENE}.");
            if (item.AttendancePunctuality < 0 || item.AttendancePunctuality > NSQF_MAX_ATTENDANCE)
                return Result.Failure($"Attendance/Punctuality must be between 0 and {NSQF_MAX_ATTENDANCE}.");
            if (item.FollowInstructions < 0 || item.FollowInstructions > NSQF_MAX_INSTRUCTIONS)
                return Result.Failure($"Follow Instructions must be between 0 and {NSQF_MAX_INSTRUCTIONS}.");
            if (item.ApplicationKnowledge < 0 || item.ApplicationKnowledge > NSQF_MAX_KNOWLEDGE)
                return Result.Failure($"Application of Knowledge must be between 0 and {NSQF_MAX_KNOWLEDGE}.");
            if (item.SkillsToolsEquipment < 0 || item.SkillsToolsEquipment > NSQF_MAX_TOOLS)
                return Result.Failure($"Skills to handle tools must be between 0 and {NSQF_MAX_TOOLS}.");
            if (item.SpeedDoingWork < 0 || item.SpeedDoingWork > NSQF_MAX_SPEED)
                return Result.Failure($"Speed in doing work must be between 0 and {NSQF_MAX_SPEED}.");
            if (item.QualityWorkmanship < 0 || item.QualityWorkmanship > NSQF_MAX_QUALITY)
                return Result.Failure($"Quality of workmanship must be between 0 and {NSQF_MAX_QUALITY}.");
            if (item.Viva < 0 || item.Viva > NSQF_MAX_VIVA)
                return Result.Failure($"Viva must be between 0 and {NSQF_MAX_VIVA}.");
        }

        var existingMarks = await _context.PracticalMarks
            .Where(m => m.MonthlyPracticalId == request.MonthlyPracticalId)
            .Where(m => studentIds.Contains(m.StudentId))
            .ToDictionaryAsync(m => m.StudentId, ct);

        foreach (var item in request.Students)
        {
            if (existingMarks.TryGetValue(item.StudentId, out var existing))
            {
                existing.SafetyConsciousness = item.SafetyConsciousness;
                existing.WorkplaceHygiene = item.WorkplaceHygiene;
                existing.AttendancePunctuality = item.AttendancePunctuality;
                existing.FollowInstructions = item.FollowInstructions;
                existing.ApplicationKnowledge = item.ApplicationKnowledge;
                existing.SkillsToolsEquipment = item.SkillsToolsEquipment;
                existing.SpeedDoingWork = item.SpeedDoingWork;
                existing.QualityWorkmanship = item.QualityWorkmanship;
                existing.Viva = item.Viva;
                existing.SignedByTrainee = item.SignedByTrainee;
                existing.Remarks = item.Remarks;
                existing.MarkedBy = userId.Value;
            }
            else
            {
                var mark = new PracticalMark
                {
                    InstituteId = instituteId.Value,
                    MonthlyPracticalId = request.MonthlyPracticalId,
                    StudentId = item.StudentId,
                    SafetyConsciousness = item.SafetyConsciousness,
                    WorkplaceHygiene = item.WorkplaceHygiene,
                    AttendancePunctuality = item.AttendancePunctuality,
                    FollowInstructions = item.FollowInstructions,
                    ApplicationKnowledge = item.ApplicationKnowledge,
                    SkillsToolsEquipment = item.SkillsToolsEquipment,
                    SpeedDoingWork = item.SpeedDoingWork,
                    QualityWorkmanship = item.QualityWorkmanship,
                    Viva = item.Viva,
                    SignedByTrainee = item.SignedByTrainee,
                    Remarks = item.Remarks,
                    MarkedBy = userId.Value
                };

                await _context.PracticalMarks.AddAsync(mark, ct);
            }
        }

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.PracticalCreate, nameof(PracticalMark), null,
            new { MonthlyPracticalId = request.MonthlyPracticalId, StudentCount = request.Students.Count },
            new { MarkedBy = userId }, ct);

        return Result.Success();
    }

    public async Task<Result> LockPracticalAsync(Guid monthlyPracticalId, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(monthlyPracticalId, isMonthly: true, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        var userId = _currentUserService.UserId;

        if (instituteId is null || userId is null)
            return Result.Failure("Institute or User not found.");

        IQueryable<MonthlyPractical> practicalQuery = _context.MonthlyPracticals;

        if (!isSuperAdmin) practicalQuery = practicalQuery.Where(p => p.InstituteId == instituteId);

        var practical = await practicalQuery
            .FirstOrDefaultAsync(p => p.Id == monthlyPracticalId, ct);

        if (practical is null)
            return Result.Failure("Monthly practical not found.");

        if (practical.IsLocked)
            return Result.Failure("Practical is already locked.");

        practical.IsLocked = true;
        practical.LockedAt = DateTime.UtcNow;
        practical.LockedBy = userId.Value;

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.DraftLock, nameof(MonthlyPractical), practical.Id,
            null, null, ct);

        return Result.Success();
    }

    public async Task<Result> UnlockPracticalAsync(Guid monthlyPracticalId, string reason, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(monthlyPracticalId, isMonthly: true, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result.Failure("Institute not found.");

        IQueryable<MonthlyPractical> practicalQuery = _context.MonthlyPracticals;

        if (!isSuperAdmin) practicalQuery = practicalQuery.Where(p => p.InstituteId == instituteId);

        var practical = await practicalQuery
            .FirstOrDefaultAsync(p => p.Id == monthlyPracticalId, ct);

        if (practical is null)
            return Result.Failure("Monthly practical not found.");

        if (!practical.IsLocked)
            return Result.Failure("Practical is already unlocked.");

        practical.IsLocked = false;
        practical.LockedAt = null;
        practical.LockedBy = null;

        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.DraftUnlock, nameof(MonthlyPractical), practical.Id,
            new { reason }, null, ct);

        return Result.Success();
    }

    public async Task<Result<PracticalReportDto>> GetPracticalReportAsync(Guid monthlyPracticalId, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidatePracticalAccessAsync(monthlyPracticalId, isMonthly: true, ct);
        if (!accessCheck.IsSuccess)
            return Result<PracticalReportDto>.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;

        if (instituteId is null)
            return Result<PracticalReportDto>.Failure("Institute not found.");

        IQueryable<MonthlyPractical> query = _context.MonthlyPracticals
            .AsNoTracking()
            .Include(p => p.Trade);

        if (!isSuperAdmin) query = query.Where(p => p.InstituteId == instituteId);

        var practical = await query.FirstOrDefaultAsync(p => p.Id == monthlyPracticalId, ct);

        if (practical is null)
            return Result<PracticalReportDto>.Failure("Monthly practical not found.");

        var passMarks = await GetPassMarksAsync(ct);

        var marks = await _context.PracticalMarks
            .AsNoTracking()
            .Include(m => m.Student)
            .Include(m => m.MarkedByUser)
            .Where(m => m.MonthlyPracticalId == monthlyPracticalId)
            .OrderBy(m => m.Student.RollNumber)
            .ToListAsync(ct);

        var totals = marks.Select(ComputeTotalObtained).ToList();
        var passedCount = totals.Count(t => t >= passMarks);
        var failedCount = totals.Count - passedCount;
        var average = totals.Any() ? (decimal)Math.Round(totals.Average(), 2) : 0m;
        var highest = totals.Any() ? totals.Max() : 0;
        var lowest = totals.Any() ? totals.Min() : 0;
        var passPercentage = totals.Any() ? Math.Round((decimal)passedCount / totals.Count * 100, 2) : 0m;

        return Result<PracticalReportDto>.Success(new PracticalReportDto
        {
            MonthlyPracticalId = monthlyPracticalId,
            PracticalName = practical.Name,
            TradeName = practical.Trade.Name,
            TradeCode = practical.Trade.Code,
            Month = practical.Month,
            Year = practical.Year,
            TotalMarks = NSQF_TOTAL_MARKS,
            PassMarks = passMarks,
            TotalStudents = marks.Count,
            PassedCount = passedCount,
            FailedCount = failedCount,
            AverageMarks = average,
            HighestMarks = highest,
            LowestMarks = lowest,
            PassPercentage = passPercentage,
            Marks = marks.Select(m => new PracticalMarkDto
            {
                Id = m.Id,
                MonthlyPracticalId = m.MonthlyPracticalId,
                StudentId = m.StudentId,
                StudentName = $"{m.Student.FirstName} {m.Student.LastName}",
                RollNumber = m.Student.RollNumber,
                SafetyConsciousness = m.SafetyConsciousness,
                WorkplaceHygiene = m.WorkplaceHygiene,
                AttendancePunctuality = m.AttendancePunctuality,
                FollowInstructions = m.FollowInstructions,
                ApplicationKnowledge = m.ApplicationKnowledge,
                SkillsToolsEquipment = m.SkillsToolsEquipment,
                SpeedDoingWork = m.SpeedDoingWork,
                QualityWorkmanship = m.QualityWorkmanship,
                Viva = m.Viva,
                TotalObtained = ComputeTotalObtained(m),
                TotalMarks = NSQF_TOTAL_MARKS,
                IsPassed = ComputeTotalObtained(m) >= passMarks,
                SignedByTrainee = m.SignedByTrainee,
                Remarks = m.Remarks,
                MarkedByUserName = $"{m.MarkedByUser.FirstName} {m.MarkedByUser.LastName}",
                CreatedAt = m.CreatedAt
            }).ToList()
        });
    }
}
