using ITI.ERP.Application.Common.Models;
using ITI.ERP.Application.DTOs.Role;

namespace ITI.ERP.Application.Interfaces;

public interface IRoleService
{
    Task<Result<List<RoleDto>>> GetRolesAsync(CancellationToken ct);
}
