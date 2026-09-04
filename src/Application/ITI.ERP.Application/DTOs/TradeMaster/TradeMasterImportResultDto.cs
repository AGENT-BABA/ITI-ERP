namespace ITI.ERP.Application.DTOs.TradeMaster;

public class TradeMasterImportResultDto
{
    public bool Success { get; set; }
    public int TradesImported { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
