using ITI.ERP.Domain.Enums;

namespace ITI.ERP.Application.DTOs.TradeMaster;

public class TradeMasterHistoryDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ImportedByName { get; set; } = string.Empty;
    public DateTime ImportDate { get; set; }
    public int NumberOfTradesImported { get; set; }
    public ImportStatus Status { get; set; }
    public string? ValidationErrors { get; set; }
}
