using System.Text.Json;
using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Domain.Enums;
using ITI.ERP.Application.Common.Helpers;

namespace ITI.ERP.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuditService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public Task LogAsync(AuditAction action, string entityName, Guid? entityId, object? oldValues, object? newValues, CancellationToken ct)
    {
        var sanitizedOld = AuditSanitizerHelper.Sanitize(oldValues);
        var sanitizedNew = AuditSanitizerHelper.Sanitize(newValues);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            InstituteId = _currentUserService.InstituteId,
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = sanitizedOld != null ? JsonSerializer.Serialize(sanitizedOld) : null,
            NewValues = sanitizedNew != null ? JsonSerializer.Serialize(sanitizedNew) : null,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        return Task.CompletedTask;
    }
}
