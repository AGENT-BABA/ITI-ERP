namespace ITI.ERP.Domain.Common;

public abstract class TenantEntity : AuditableEntity
{
    public Guid? InstituteId { get; set; }
}
