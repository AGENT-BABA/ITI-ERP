using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Mappers;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Settings;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class SettingsService : ISettingsService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public SettingsService(IApplicationDbContext context, ICurrentUserService currentUserService, IAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<InstituteSettingsDto>> GetSettingsAsync(CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        if (!isSuperAdmin && instituteId is null)
            return Result<InstituteSettingsDto>.Failure("Institute not found.");

        var query = _context.InstituteSettings.AsNoTracking();

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == instituteId);

        var settings = await query.FirstOrDefaultAsync(ct);

        if (settings is null)
        {
            if (isSuperAdmin)
                return Result<InstituteSettingsDto>.Failure("Settings not found.");

            var initResult = await InitializeDefaultSettingsAsync(instituteId!.Value, ct);
            if (!initResult.IsSuccess)
                return Result<InstituteSettingsDto>.Failure(initResult.Error ?? "Failed to initialize settings.");

            var retryQuery = _context.InstituteSettings.AsNoTracking();

            if (!isSuperAdmin)
                retryQuery = retryQuery.Where(s => s.InstituteId == instituteId);

            settings = await retryQuery.FirstOrDefaultAsync(ct);

            if (settings is null)
                return Result<InstituteSettingsDto>.Failure("Settings not found after initialization.");
        }

        return Result<InstituteSettingsDto>.Success(settings.ToDto());
    }

    public async Task<Result<InstituteSettingsDto>> UpdateSettingsAsync(UpdateInstituteSettingsRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);
        var instituteId = _currentUserService.InstituteId;
        if (!isSuperAdmin && instituteId is null)
            return Result<InstituteSettingsDto>.Failure("Institute not found.");

        IQueryable<InstituteSettings> query = _context.InstituteSettings;

        if (!isSuperAdmin)
            query = query.Where(s => s.InstituteId == instituteId);

        var settings = await query.FirstOrDefaultAsync(ct);

        if (settings is null)
        {
            if (isSuperAdmin)
                return Result<InstituteSettingsDto>.Failure("Settings not found.");

            var initResult = await InitializeDefaultSettingsAsync(instituteId!.Value, ct);
            if (!initResult.IsSuccess)
                return Result<InstituteSettingsDto>.Failure(initResult.Error ?? "Failed to initialize settings.");

            IQueryable<InstituteSettings> retryQuery = _context.InstituteSettings;

            if (!isSuperAdmin)
                retryQuery = retryQuery.Where(s => s.InstituteId == instituteId);

            settings = await retryQuery.FirstOrDefaultAsync(ct);

            if (settings is null)
                return Result<InstituteSettingsDto>.Failure("Settings not found after initialization.");
        }

        var oldValues = new
        {
            settings.AcademicSessionFormat,
            settings.AttendanceThresholdPercentage,
            settings.PassMarksPercentage,
            settings.EnableNotifications,
            settings.NotificationEmail,
            settings.Address,
            settings.City,
            settings.District,
            settings.State,
            settings.Phone,
            settings.Email,
            settings.Website,
            settings.PrincipalName,
            settings.AffiliationNumber,
            settings.RecognitionNumber
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            if (request.AcademicSessionFormat is not null) settings.AcademicSessionFormat = request.AcademicSessionFormat;
            if (request.AttendanceThresholdPercentage.HasValue) settings.AttendanceThresholdPercentage = request.AttendanceThresholdPercentage.Value;
            if (request.PassMarksPercentage.HasValue) settings.PassMarksPercentage = request.PassMarksPercentage.Value;
            if (request.EnableNotifications.HasValue) settings.EnableNotifications = request.EnableNotifications.Value;
            if (request.NotificationEmail is not null) settings.NotificationEmail = request.NotificationEmail;
            if (request.LogoPath is not null) settings.LogoPath = request.LogoPath;
            if (request.Address is not null) settings.Address = request.Address;
            if (request.City is not null) settings.City = request.City;
            if (request.District is not null) settings.District = request.District;
            if (request.State is not null) settings.State = request.State;
            if (request.Phone is not null) settings.Phone = request.Phone;
            if (request.Email is not null) settings.Email = request.Email;
            if (request.Website is not null) settings.Website = request.Website;
            if (request.PrincipalName is not null) settings.PrincipalName = request.PrincipalName;
            if (request.AffiliationNumber is not null) settings.AffiliationNumber = request.AffiliationNumber;
            if (request.RecognitionNumber is not null) settings.RecognitionNumber = request.RecognitionNumber;

            await _context.SaveChangesAsync(ct);

            await _auditService.LogAsync(Domain.Enums.AuditAction.Update, nameof(InstituteSettings), settings.Id, oldValues, settings, ct);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }

        return Result<InstituteSettingsDto>.Success(settings.ToDto());
    }

    public async Task<Result<InstituteSettingsDto>> InitializeDefaultSettingsAsync(Guid instituteId, CancellationToken ct)
    {
        if (await _context.InstituteSettings.AnyAsync(s => s.InstituteId == instituteId, ct))
        {
            var existing = await _context.InstituteSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.InstituteId == instituteId, ct);
            return Result<InstituteSettingsDto>.Success(existing!.ToDto());
        }

        var settings = new InstituteSettings
        {
            InstituteId = instituteId,
            AttendanceThresholdPercentage = 75,
            PassMarksPercentage = 40,
            EnableNotifications = true
        };

        await _context.InstituteSettings.AddAsync(settings, ct);
        await _context.SaveChangesAsync(ct);

        return Result<InstituteSettingsDto>.Success(settings.ToDto());
    }
}
