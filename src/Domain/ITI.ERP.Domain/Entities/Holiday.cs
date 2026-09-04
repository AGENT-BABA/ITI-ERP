using ITI.ERP.Domain.Common;

namespace ITI.ERP.Domain.Entities;

public class Holiday : SoftDeletableEntity
{
    public Guid AcademicSessionId { get; set; }
    public DateTime Date { get; set; }
    public string? Name { get; set; }
    public AcademicSession AcademicSession { get; set; } = null!;
}
