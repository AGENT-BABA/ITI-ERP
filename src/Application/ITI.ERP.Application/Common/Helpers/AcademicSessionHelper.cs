using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Domain.Entities;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Common.Helpers;

public static class AcademicSessionHelper
{
    public static async Task<Guid?> ResolveActiveSessionIdAsync(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        CancellationToken ct)
    {
        if (currentUserService.AcademicSessionId.HasValue)
            return currentUserService.AcademicSessionId.Value;

        var instituteId = currentUserService.InstituteId;
        if (instituteId is null)
            return null;

        return await context.AcademicSessions
            .Where(s => s.InstituteId == instituteId.Value && s.IsActive && !s.IsDeleted)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);
    }

    public static async Task<AcademicSession?> ResolveActiveSessionEntityAsync(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        CancellationToken ct)
    {
        if (currentUserService.AcademicSessionId.HasValue)
            return await context.AcademicSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == currentUserService.AcademicSessionId.Value, ct);

        var instituteId = currentUserService.InstituteId;
        if (instituteId is null) return null;

        return await context.AcademicSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.InstituteId == instituteId.Value && s.IsActive && !s.IsDeleted, ct);
    }

    public static async Task<AcademicSession?> ResolveSessionOrDefaultAsync(
        Guid? requestedSessionId,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        CancellationToken ct)
    {
        if (requestedSessionId.HasValue)
            return await context.AcademicSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == requestedSessionId.Value, ct);

        return await ResolveActiveSessionEntityAsync(context, currentUserService, ct);
    }

    public static async Task<List<Guid>> ResolveSessionIdsByYearAsync(
        string sessionYear,
        Guid? instituteId,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        CancellationToken ct)
    {
        var query = context.AcademicSessions
            .AsNoTracking()
            .Where(s => s.SessionYear == sessionYear && !s.IsDeleted);

        var isInstituteAdmin = currentUserService.HasRole(RoleConstants.InstituteAdmin)
            || currentUserService.HasRole(RoleConstants.TradeHead);

        if (isInstituteAdmin && currentUserService.InstituteId.HasValue)
            query = query.Where(s => s.InstituteId == currentUserService.InstituteId.Value);
        else if (instituteId.HasValue)
            query = query.Where(s => s.InstituteId == instituteId.Value);

        return await query.Select(s => s.Id).ToListAsync(ct);
    }
}