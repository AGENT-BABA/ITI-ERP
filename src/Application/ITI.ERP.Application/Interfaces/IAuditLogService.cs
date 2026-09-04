using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.AuditLog;

namespace ITI.ERP.Application.Interfaces;

public interface IAuditLogService
{
    Task<Result<PaginatedList<AuditLogDto>>> GetAuditLogsAsync(PaginationRequest request, CancellationToken ct);
}
