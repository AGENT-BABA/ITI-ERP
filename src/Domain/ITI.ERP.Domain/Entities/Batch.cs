using ITI.ERP.Domain.Common;
using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Domain.Entities;

public class Batch : SoftDeletableEntity
{
    public new Guid InstituteId { get; set; }
    public Guid TradeId { get; set; }
    public Guid StartAcademicSessionId { get; set; }
    public DateTime StartDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public Institute Institute { get; set; } = null!;
    public Trade Trade { get; set; } = null!;
    public AcademicSession StartAcademicSession { get; set; } = null!;
}
