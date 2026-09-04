using ITI.ERP.Application.Common.Interfaces;
using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Role;
using ITI.ERP.Application.Interfaces;
using ITI.ERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ITI.ERP.Application.Services;

public class RoleService : IRoleService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RoleService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<RoleDto>>> GetRolesAsync(CancellationToken ct)
    {
        var isSuperAdmin = _currentUserService.HasRole(RoleConstants.Admin);

        var query = _context.Roles
            .AsNoTracking()
            .Where(r => r.IsActive);

        if (!isSuperAdmin) query = query.Where(r => r.IsSystemRole || r.InstituteId == _currentUserService.InstituteId);

        var roles = await query
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            })
            .ToListAsync(ct);

        return Result<List<RoleDto>>.Success(roles);
    }
}
