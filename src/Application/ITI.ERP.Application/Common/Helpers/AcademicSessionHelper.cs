using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Domain.Entities;
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
}