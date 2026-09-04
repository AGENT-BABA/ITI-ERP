using ITI.ERP.Domain.Common;
using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Domain.Entities;

public class Role : AuditableEntity
{
    public Guid? InstituteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RoleType RoleType { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }

    public Institute? Institute { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
