using ITI.ERP.Domain.Common;
using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Domain.Entities;

public class StudentImportHistory : TenantEntity
{
    public string FileName { get; set; } = string.Empty;
    public DateTime ImportDate { get; set; }
    public int NumberOfStudentsImported { get; set; }
    public ImportStatus Status { get; set; }
    public string? ValidationErrors { get; set; }
    public Guid TradeId { get; set; }
    public Guid AcademicSessionId { get; set; }

    public Institute Institute { get; set; } = null!;
    public Trade Trade { get; set; } = null!;
    public AcademicSession AcademicSession { get; set; } = null!;
}
