using ITI.ERP.Domain.Common;

namespace ITI.ERP.Domain.Entities;

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid? InstituteId { get; set; }
    public Guid? AcademicSessionId { get; set; }
    public Guid? TradeId { get; set; }
    public Guid? BatchId { get; set; }
    public bool IsActive { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public Institute? Institute { get; set; }
    public AcademicSession? AcademicSession { get; set; }
    public Trade? Trade { get; set; }
    public Batch? Batch { get; set; }
}
