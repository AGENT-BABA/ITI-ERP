using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.AuditLog;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuditLogService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedList<AuditLogDto>>> GetAuditLogsAsync(PaginationRequest request, CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.AuditLogs
            .AsNoTracking();

        if (!isSuperAdmin)
        {
            query = query.Where(a => a.InstituteId == _currentUserService.InstituteId);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(a =>
                a.EntityName.ToLower().Contains(searchTerm) ||
                (a.UserName != null && a.UserName.ToLower().Contains(searchTerm)) ||
                a.Action.ToString().ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "timestamp" => request.SortDescending ? query.OrderByDescending(a => a.Timestamp) : query.OrderBy(a => a.Timestamp),
            "action" => request.SortDescending ? query.OrderByDescending(a => a.Action) : query.OrderBy(a => a.Action),
            "entityname" => request.SortDescending ? query.OrderByDescending(a => a.EntityName) : query.OrderBy(a => a.EntityName),
            _ => query.OrderByDescending(a => a.Timestamp)
        };

        var paginatedList = await PaginatedList<AuditLogDto>.CreateAsync(
            query.Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = a.UserName,
                Action = a.Action.ToString(),
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IPAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                CreatedAt = a.Timestamp
            }),
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<AuditLogDto>>.Success(paginatedList);
    }
}
