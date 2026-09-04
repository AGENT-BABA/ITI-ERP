namespace ITI.ERP.Domain.Common;

public abstract class SoftDeletableEntity : TenantEntity
{
    public bool IsDeleted { get; set; }
}
