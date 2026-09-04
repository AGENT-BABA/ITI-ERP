using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditAction action, string entityName, Guid? entityId, object? oldValues, object? newValues, CancellationToken ct);
}
