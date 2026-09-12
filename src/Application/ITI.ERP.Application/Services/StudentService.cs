using ITI.ERP.Application.Common.Helpers;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Mappers;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Student;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class StudentService : IStudentService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ITradeAccessService _tradeAccess;

    public StudentService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IFileStorageService fileStorageService,
        ITradeAccessService tradeAccess)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _fileStorageService = fileStorageService;
        _tradeAccess = tradeAccess;
    }

    public async Task<Result<PaginatedList<StudentDto>>> GetStudentsAsync(PaginationRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        IQueryable<Student> query = _context.Students
            .AsNoTracking()
            .Include(s => s.Trade)
            .Include(s => s.Batch);

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == _currentUserService.InstituteId);

        query = _tradeAccess.ApplyTradeFilter(query, s => s.TradeId);
        query = _tradeAccess.ApplyBatchFilter(query, s => s.BatchId);

        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);
        if (academicSessionId.HasValue)
        {
            query = query.Where(s => s.AcademicSessionId == academicSessionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(searchTerm) ||
                s.LastName.ToLower().Contains(searchTerm) ||
                s.RollNumber.ToLower().Contains(searchTerm) ||
                s.AdmissionNumber.ToLower().Contains(searchTerm) ||
                (s.FatherName != null && s.FatherName.ToLower().Contains(searchTerm)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(s => s.LastName).ThenByDescending(s => s.FirstName)
                : query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName),
            "rollnumber" => request.SortDescending
                ? query.OrderByDescending(s => s.RollNumber)
                : query.OrderBy(s => s.RollNumber),
            "status" => request.SortDescending
                ? query.OrderByDescending(s => s.Status)
                : query.OrderBy(s => s.Status),
            "admissiondate" => request.SortDescending
                ? query.OrderByDescending(s => s.AdmissionDate)
                : query.OrderBy(s => s.AdmissionDate),
            _ => query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var dtoItems = items.Select(s => s.ToDto()).ToList();
        var paginatedList = new PaginatedList<StudentDto>(dtoItems, totalCount, request.PageNumber, request.PageSize);

        return Result<PaginatedList<StudentDto>>.Success(paginatedList);
    }

    public async Task<Result<StudentDto>> GetStudentByIdAsync(Guid id, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(id, ct);
        if (!accessCheck.IsSuccess)
            return Result<StudentDto>.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.Students
            .AsNoTracking()
            .Include(s => s.Trade)
            .Include(s => s.Batch)
            .Where(s => s.Id == id);

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == _currentUserService.InstituteId);

        var student = await query.FirstOrDefaultAsync(ct);

        if (student is null)
            return Result<StudentDto>.Failure("Student not found.");

        return Result<StudentDto>.Success(student.ToDto());
    }

    public async Task<Result<StudentDto>> CreateStudentAsync(CreateStudentRequest request, CancellationToken ct)
    {
        var instituteId = _currentUserService.InstituteId;
        var academicSessionId = await AcademicSessionHelper.ResolveActiveSessionIdAsync(_context, _currentUserService, ct);

        if (instituteId is null)
            return Result<StudentDto>.Failure("Institute not found.");

        if (!academicSessionId.HasValue)
            return Result<StudentDto>.Failure("No active academic session found.");

        if (_tradeAccess.IsTradeHead)
        {
            if (!_tradeAccess.EffectiveTradeId.HasValue)
                return Result<StudentDto>.Failure("TradeHead trade assignment is not configured.");
            request.TradeId = _tradeAccess.EffectiveTradeId.Value;
            if (!_tradeAccess.EffectiveBatchId.HasValue)
                return Result<StudentDto>.Failure("TradeHead batch assignment is not configured.");
            request.BatchId = _tradeAccess.EffectiveBatchId.Value;
        }

        if (!await _context.Institutes.AnyAsync(i => i.Id == instituteId.Value, ct))
            return Result<StudentDto>.Failure("Institute not found.");

        if (!await _context.AcademicSessions.AnyAsync(a => a.Id == academicSessionId.Value && a.InstituteId == instituteId.Value && !a.IsDeleted, ct))
            return Result<StudentDto>.Failure("Academic Session not found.");

        if (!await _context.Trades.AnyAsync(t =>
            t.Id == request.TradeId && t.InstituteId == instituteId && !t.IsDeleted, ct))
            return Result<StudentDto>.Failure("Trade not found.");

        if (await _context.Students.AnyAsync(s =>
            s.InstituteId == instituteId &&
            s.AcademicSessionId == academicSessionId &&
            s.RollNumber == request.RollNumber, ct))
            return Result<StudentDto>.Failure("Roll number already exists in this session.");

        if (await _context.Students.AnyAsync(s =>
            s.InstituteId == instituteId &&
            s.AcademicSessionId == academicSessionId &&
            s.AdmissionNumber == request.AdmissionNumber, ct))
            return Result<StudentDto>.Failure("Admission number already exists in this session.");

        if (!string.IsNullOrWhiteSpace(request.AadharNumber))
        {
            if (request.AadharNumber.Length != 12 || !request.AadharNumber.All(char.IsDigit))
                return Result<StudentDto>.Failure("Aadhar number must be exactly 12 digits.");

            if (await _context.Students.AnyAsync(s =>
                s.InstituteId == instituteId && s.AadharNumber == request.AadharNumber, ct))
                return Result<StudentDto>.Failure("Student with this Aadhar number already exists.");
        }

        var trade = await _context.Trades
            .Include(t => t.AcademicSession)
            .FirstOrDefaultAsync(t => t.Id == request.TradeId, ct);

        if (trade!.DraftStatus == DraftStatus.Archived)
            return Result<StudentDto>.Failure("Cannot add student to an archived trade.");

        var seatCount = await _context.Students
            .CountAsync(s => s.TradeId == request.TradeId &&
                s.AcademicSessionId == academicSessionId &&
                s.Status == StudentStatus.Active, ct);

        if (trade!.TotalSeats > 0 && seatCount >= trade.TotalSeats)
            return Result<StudentDto>.Failure("No vacant seats available in this trade.");

        var student = new Student
        {
            InstituteId = instituteId.Value,
            AcademicSessionId = academicSessionId.Value,
            FirstName = request.FirstName?.Trim() ?? string.Empty,
            MiddleName = request.MiddleName?.Trim(),
            LastName = request.LastName?.Trim() ?? string.Empty,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            BloodGroup = request.BloodGroup?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address?.Trim(),
            City = request.City?.Trim(),
            State = request.State?.Trim(),
            PinCode = request.PinCode?.Trim(),
            FatherName = request.FatherName?.Trim(),
            MotherName = request.MotherName?.Trim(),
            GuardianPhone = request.GuardianPhone?.Trim(),
            GuardianRelation = request.GuardianRelation?.Trim(),
            TradeId = request.TradeId,
            BatchId = request.BatchId,
            RollNumber = request.RollNumber?.Trim() ?? string.Empty,
            AdmissionNumber = request.AdmissionNumber?.Trim() ?? string.Empty,
            AdmissionDate = request.AdmissionDate,
            AnnualIncome = request.AnnualIncome,
            CasteCategory = request.CasteCategory,
            IsPhysicallyHandicapped = request.IsPhysicallyHandicapped,
            PreviousSchool = request.PreviousSchool,
            PreviousQualification = request.PreviousQualification,
            PreviousPercentage = request.PreviousPercentage,
            AadharNumber = request.AadharNumber,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            EmergencyContactRelation = request.EmergencyContactRelation,
            Status = StudentStatus.Active,
            DraftStatus = DraftStatus.Draft
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await _context.Students.AddAsync(student, ct);
            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(AuditAction.Create, nameof(Student), student.Id, null, student, ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        student = await _context.Students
            .AsNoTracking()
            .Include(s => s.Trade)
            .FirstOrDefaultAsync(s => s.Id == student.Id, ct);

        return Result<StudentDto>.Success(student!.ToDto());
    }

    public async Task<Result<StudentDto>> UpdateStudentAsync(Guid id, UpdateStudentRequest request, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(id, ct);
        if (!accessCheck.IsSuccess)
            return Result<StudentDto>.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.Students
            .Include(s => s.Trade)
            .Include(s => s.Batch)
            .Where(s => s.Id == id);

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == _currentUserService.InstituteId);

        var student = await query.FirstOrDefaultAsync(ct);

        if (student is null)
            return Result<StudentDto>.Failure("Student not found.");

        if (student.DraftStatus == DraftStatus.Locked)
            return Result<StudentDto>.Failure("Cannot edit a locked student record.");

        var oldValues = new
        {
            student.FirstName,
            student.LastName,
            student.Phone,
            student.Email,
            student.Address,
            student.FatherName,
            student.MotherName,
            student.Status
        };

        if (request.FirstName is not null) student.FirstName = request.FirstName;
        if (request.MiddleName is not null) student.MiddleName = request.MiddleName;
        if (request.LastName is not null) student.LastName = request.LastName;
        if (request.DateOfBirth.HasValue) student.DateOfBirth = request.DateOfBirth.Value;
        if (request.Gender.HasValue) student.Gender = request.Gender.Value;
        if (request.BloodGroup is not null) student.BloodGroup = request.BloodGroup;
        if (request.Phone is not null) student.Phone = request.Phone;
        if (request.Email is not null) student.Email = request.Email;
        if (request.Address is not null) student.Address = request.Address;
        if (request.City is not null) student.City = request.City;
        if (request.State is not null) student.State = request.State;
        if (request.PinCode is not null) student.PinCode = request.PinCode;
        if (request.FatherName is not null) student.FatherName = request.FatherName;
        if (request.MotherName is not null) student.MotherName = request.MotherName;
        if (request.GuardianPhone is not null) student.GuardianPhone = request.GuardianPhone;
        if (request.GuardianRelation is not null) student.GuardianRelation = request.GuardianRelation;
        if (request.AadharNumber is not null) student.AadharNumber = request.AadharNumber;
        if (request.EmergencyContactName is not null) student.EmergencyContactName = request.EmergencyContactName;
        if (request.EmergencyContactPhone is not null) student.EmergencyContactPhone = request.EmergencyContactPhone;
        if (request.EmergencyContactRelation is not null) student.EmergencyContactRelation = request.EmergencyContactRelation;
        if (request.PreviousSchool is not null) student.PreviousSchool = request.PreviousSchool;
        if (request.PreviousQualification is not null) student.PreviousQualification = request.PreviousQualification;
        if (request.PreviousPercentage.HasValue) student.PreviousPercentage = request.PreviousPercentage;
        if (request.CasteCategory is not null) student.CasteCategory = request.CasteCategory;
        if (request.IsPhysicallyHandicapped.HasValue) student.IsPhysicallyHandicapped = request.IsPhysicallyHandicapped.Value;
        if (request.AnnualIncome.HasValue) student.AnnualIncome = request.AnnualIncome.Value;

        if (request.TradeId.HasValue && request.TradeId != student.TradeId)
        {
            if (_tradeAccess.IsTradeHead)
                return Result<StudentDto>.Failure("TradeHead cannot change a student's trade.");

            if (!await _context.Trades.AnyAsync(t =>
                t.Id == request.TradeId.Value && t.InstituteId == student.InstituteId && !t.IsDeleted, ct))
                return Result<StudentDto>.Failure("Trade not found.");

            student.TradeId = request.TradeId.Value;
        }

        if (request.BatchId.HasValue && request.BatchId != student.BatchId)
        {
            if (_tradeAccess.IsTradeHead)
                return Result<StudentDto>.Failure("TradeHead cannot change a student's batch.");

            if (request.BatchId.Value != Guid.Empty)
            {
                var batch = await _context.Batches
                    .FirstOrDefaultAsync(b => b.Id == request.BatchId.Value && !b.IsDeleted, ct);

                if (batch is null || batch.InstituteId != student.InstituteId)
                    return Result<StudentDto>.Failure("Batch not found or does not belong to this institute.");

                if (batch.TradeId != student.TradeId)
                    return Result<StudentDto>.Failure("Batch does not belong to the student's trade.");
            }

            student.BatchId = request.BatchId.Value == Guid.Empty ? null : request.BatchId.Value;
        }
        else if (request.BatchId.HasValue && request.BatchId.Value == Guid.Empty && student.BatchId.HasValue)
        {
            if (!_tradeAccess.IsTradeHead)
                student.BatchId = null;
        }

        if (request.RollNumber is not null && request.RollNumber != student.RollNumber)
        {
            if (await _context.Students.AnyAsync(s =>
                s.InstituteId == student.InstituteId &&
                s.AcademicSessionId == student.AcademicSessionId &&
                s.RollNumber == request.RollNumber &&
                s.Id != id, ct))
                return Result<StudentDto>.Failure("Roll number already exists in this session.");

            student.RollNumber = request.RollNumber;
        }

        if (request.AdmissionNumber is not null && request.AdmissionNumber != student.AdmissionNumber)
        {
            if (await _context.Students.AnyAsync(s =>
                s.InstituteId == student.InstituteId &&
                s.AcademicSessionId == student.AcademicSessionId &&
                s.AdmissionNumber == request.AdmissionNumber &&
                s.Id != id, ct))
                return Result<StudentDto>.Failure("Admission number already exists in this session.");

            student.AdmissionNumber = request.AdmissionNumber;
        }

        if (request.AdmissionDate.HasValue) student.AdmissionDate = request.AdmissionDate.Value;

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(AuditAction.Update, nameof(Student), student.Id, oldValues, student, ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        student = await _context.Students
            .AsNoTracking()
            .Include(s => s.Trade)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        return Result<StudentDto>.Success(student!.ToDto());
    }

    public async Task<Result> TransferStudentAsync(Guid id, TransferStudentRequest request, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(id, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.Students
            .Where(s => s.Id == id);

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == _currentUserService.InstituteId);

        var student = await query.FirstOrDefaultAsync(ct);

        if (student is null)
            return Result.Failure("Student not found.");

        if (student.Status != StudentStatus.Active)
            return Result.Failure("Only active students can be transferred.");

        if (!await _context.Trades.AnyAsync(t =>
            t.Id == request.NewTradeId && t.InstituteId == student.InstituteId && !t.IsDeleted, ct))
            return Result.Failure("Target trade not found.");

        if (request.NewTradeId == student.TradeId)
            return Result.Failure("Student is already in this trade.");

        var oldTradeId = student.TradeId;

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            student.TradeId = request.NewTradeId;
            student.StatusReason = request.Reason;
            student.StatusChangedAt = DateTime.UtcNow;
            student.StatusChangedBy = _currentUserService.UserId;

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Update, nameof(Student), student.Id,
                new { TradeId = oldTradeId, Status = student.Status },
                new { TradeId = request.NewTradeId, Status = student.Status, request.Reason }, ct);
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

    public async Task<Result> ChangeBatchAsync(Guid id, ChangeBatchRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var isInstituteAdmin = _currentUserService.HasRole(RoleConstants.InstituteAdmin);

        if (!isSuperAdmin && !isInstituteAdmin)
            return Result.Failure("Only SuperAdmin or InstituteAdmin can change a student's batch.");

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);

        if (student is null)
            return Result.Failure("Student not found.");

        if (!isSuperAdmin && student.InstituteId != _currentUserService.InstituteId)
            return Result.Failure("Access denied.");

        if (student.Status != StudentStatus.Active)
            return Result.Failure("Only active students can have their batch changed.");

        var targetBatch = await _context.Batches
            .FirstOrDefaultAsync(b => b.Id == request.NewBatchId && !b.IsDeleted, ct);

        if (targetBatch is null)
            return Result.Failure("Target batch not found.");

        if (!isSuperAdmin && targetBatch.InstituteId != _currentUserService.InstituteId)
            return Result.Failure("Access denied: batch does not belong to your institute.");

        if (!targetBatch.IsActive)
            return Result.Failure("Cannot assign student to an archived batch.");

        if (targetBatch.TradeId != student.TradeId)
            return Result.Failure("Target batch does not belong to the student's trade.");

        if (targetBatch.StartAcademicSessionId != student.AcademicSessionId)
            return Result.Failure("Target batch does not belong to the student's admission session.");

        if (student.BatchId == request.NewBatchId)
            return Result.Failure("Student is already in this batch.");

        var oldBatchId = student.BatchId;

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            student.BatchId = request.NewBatchId;
            student.StatusReason = request.Reason;
            student.StatusChangedAt = DateTime.UtcNow;
            student.StatusChangedBy = _currentUserService.UserId;

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Update, nameof(Student), student.Id,
                new { BatchId = oldBatchId },
                new { BatchId = request.NewBatchId, request.Reason }, ct);
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

    public async Task<Result> ChangeStatusAsync(Guid id, ChangeStudentStatusRequest request, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(id, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.Students
            .Where(s => s.Id == id);

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == _currentUserService.InstituteId);

        var student = await query.FirstOrDefaultAsync(ct);

        if (student is null)
            return Result.Failure("Student not found.");

        if (!student.CanTransitionToStatus(request.NewStatus))
            return Result.Failure($"Invalid status transition from {student.Status} to {request.NewStatus}.");

        var oldStatus = student.Status;

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            student.ChangeStatus(request.NewStatus, request.Reason, _currentUserService.UserId!.Value, DateTime.UtcNow);

            if (request.NewStatus == StudentStatus.Withdrawn)
            {
                student.WithdrawalDate = DateTime.UtcNow;
                student.WithdrawalReason = request.Reason;
            }

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Update, nameof(Student), student.Id,
                new { Status = oldStatus },
                new { Status = request.NewStatus, request.Reason }, ct);
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

    public async Task<Result> ArchiveStudentAsync(Guid id, string reason, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("Archive reason is required.");

        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(id, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.Students
            .Where(s => s.Id == id);

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == _currentUserService.InstituteId);

        var student = await query.Include(s => s.AcademicSession).FirstOrDefaultAsync(ct);

        if (student is null)
            return Result.Failure("Student not found.");

        if (student.Status == StudentStatus.Archived)
            return Result.Failure("Student is already archived.");

        var oldStatus = student.Status;

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            student.ChangeStatus(StudentStatus.Archived, reason, _currentUserService.UserId!.Value, DateTime.UtcNow);
            student.WithdrawalDate = DateTime.UtcNow;
            student.WithdrawalReason = reason;
            student.RetentionUntil = student.AcademicSession.EndDate.AddYears(1);

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Archive, nameof(Student), student.Id,
                new { Status = oldStatus },
                new { Status = StudentStatus.Archived, reason, student.RetentionUntil }, ct);
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

    public async Task<Result> DeleteStudentAsync(Guid id, string reason, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("Deletion reason is required.");

        var instituteId = _currentUserService.InstituteId;
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        if (!isSuperAdmin && instituteId is null)
            return Result.Failure("Institute not found.");

        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(id, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var query = _context.Students
            .Where(s => s.Id == id);

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == instituteId);

        var student = await query.FirstOrDefaultAsync(ct);

        if (student is null)
            return Result.Failure("Student not found.");

        var studentName = student.GetFullName();
        var batchId = student.BatchId;
        var tradeId = student.TradeId;

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await _context.PracticalMarks.Where(m => m.StudentId == id).ExecuteDeleteAsync(ct);
            await _context.YearlyPracticalMarks.Where(m => m.StudentId == id).ExecuteDeleteAsync(ct);
            await _context.AttendanceRecords.Where(a => a.StudentId == id).ExecuteDeleteAsync(ct);
            await _context.Students.Where(s => s.Id == id).ExecuteDeleteAsync(ct);

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Delete, nameof(Student), id,
                new { StudentName = studentName, student.RollNumber, student.AdmissionNumber, BatchId = batchId, TradeId = tradeId },
                new { Reason = reason, DeletedBy = _currentUserService.UserId }, ct);
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

    public async Task<Result> UploadPhotoAsync(Guid id, Stream fileStream, string fileName, CancellationToken ct)
    {
        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(id, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.Students
            .Where(s => s.Id == id);

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == _currentUserService.InstituteId);

        var student = await query.FirstOrDefaultAsync(ct);

        if (student is null)
            return Result.Failure("Student not found.");

        var path = $"students/{student.InstituteId}/{student.AcademicSessionId}/{student.Id}";
        var storedFileName = await _fileStorageService.UploadAsync(path, fileName, fileStream, ct);

        if (student.PhotoPath is not null)
        {
            await _fileStorageService.DeleteAsync(student.PhotoPath, ct);
        }

        student.PhotoPath = storedFileName;
        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditAction.Update, nameof(Student), student.Id,
            new { PhotoPath = student.PhotoPath },
            new { PhotoPath = storedFileName }, ct);

        return Result.Success();
    }

    public async Task<Result<PaginatedList<StudentDto>>> GetArchivedStudentsAsync(PaginationRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        IQueryable<Student> query = _context.Students
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(s => s.Trade)
            .Include(s => s.Batch)
            .Where(s => s.Status == StudentStatus.Archived);

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == _currentUserService.InstituteId);

        query = _tradeAccess.ApplyTradeFilter(query, s => s.TradeId);
        query = _tradeAccess.ApplyBatchFilter(query, s => s.BatchId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(searchTerm) ||
                s.LastName.ToLower().Contains(searchTerm) ||
                s.RollNumber.ToLower().Contains(searchTerm) ||
                s.AdmissionNumber.ToLower().Contains(searchTerm));
        }

        query = query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var dtoItems = items.Select(s => s.ToDto()).ToList();
        var paginatedList = new PaginatedList<StudentDto>(dtoItems, totalCount, request.PageNumber, request.PageSize);

        return Result<PaginatedList<StudentDto>>.Success(paginatedList);
    }

    public async Task<Result> UnarchiveStudentAsync(Guid id, string reason, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("Unarchive reason is required.");

        var accessCheck = await _tradeAccess.ValidateStudentAccessAsync(id, ct);
        if (!accessCheck.IsSuccess)
            return Result.Failure(accessCheck.Error!);

        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.Students
            .IgnoreQueryFilters()
            .Where(s => s.Id == id);

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == _currentUserService.InstituteId);

        var student = await query.FirstOrDefaultAsync(ct);

        if (student is null)
            return Result.Failure("Student not found.");

        if (student.Status != StudentStatus.Archived)
            return Result.Failure("Student is not archived.");

        var oldRetentionUntil = student.RetentionUntil;

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            student.ChangeStatus(StudentStatus.Active, reason, _currentUserService.UserId!.Value, DateTime.UtcNow);
            student.RetentionUntil = null;

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(AuditAction.Restore, nameof(Student), student.Id,
                new { Status = StudentStatus.Archived, RetentionUntil = oldRetentionUntil },
                new { Status = StudentStatus.Active, reason }, ct);
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
