using ITI.ERP.Domain.Common;
using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Domain.Entities;

public class TradeImportHistory : TenantEntity
{
    public string FileName { get; set; } = string.Empty;
    public DateTime ImportDate { get; set; }
    public int NumberOfTradesImported { get; set; }
    public ImportStatus Status { get; set; }
    public string? ValidationErrors { get; set; }

    public Institute Institute { get; set; } = null!;
}
