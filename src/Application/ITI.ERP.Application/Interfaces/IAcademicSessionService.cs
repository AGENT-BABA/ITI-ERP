using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.AcademicSession;

namespace ITI.ERP.Application.Interfaces;

public interface IAcademicSessionService
{
    Task<Result<PaginatedList<AcademicSessionDto>>> GetAcademicSessionsAsync(PaginationRequest request, CancellationToken ct);
    Task<Result<AcademicSessionDto>> GetAcademicSessionByIdAsync(Guid id, CancellationToken ct);
    Task<Result<AcademicSessionDto>> CreateAcademicSessionAsync(CreateAcademicSessionRequest request, CancellationToken ct);
    Task<Result<AcademicSessionDto>> UpdateAcademicSessionAsync(Guid id, UpdateAcademicSessionRequest request, CancellationToken ct);
    Task<Result> ActivateSessionAsync(Guid id, CancellationToken ct);
    Task<Result> LockSessionAsync(Guid id, CancellationToken ct);
    Task<Result> DeleteAcademicSessionAsync(Guid id, CancellationToken ct);
}
