using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Settings;

namespace ITI.ERP.Application.Interfaces;

public interface ISettingsService
{
    Task<Result<InstituteSettingsDto>> GetSettingsAsync(CancellationToken ct);
    Task<Result<InstituteSettingsDto>> UpdateSettingsAsync(UpdateInstituteSettingsRequest request, CancellationToken ct);
    Task<Result<InstituteSettingsDto>> InitializeDefaultSettingsAsync(Guid instituteId, CancellationToken ct);
}
