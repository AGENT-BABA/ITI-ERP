using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Institute;

namespace ITI.ERP.Application.Interfaces;

public interface IInstituteService
{
    Task<Result<PaginatedList<InstituteDto>>> GetInstitutesAsync(PaginationRequest request, CancellationToken ct);
    Task<Result<InstituteDto>> GetInstituteByIdAsync(Guid id, CancellationToken ct);
    Task<Result<InstituteDto>> CreateInstituteAsync(CreateInstituteRequest request, CancellationToken ct);
    Task<Result<InstituteDto>> UpdateInstituteAsync(Guid id, UpdateInstituteRequest request, CancellationToken ct);
    Task<Result> DeleteInstituteAsync(Guid id, CancellationToken ct);
}
